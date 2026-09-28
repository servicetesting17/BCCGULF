using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCCGULF.CustomAPI
{
    /// <summary>
    /// Custom Action handler for bcc_EvaluationAction.
    /// Input Parameters : action (Edm.String), selectedRecords (Edm.String — comma-separated GUIDs), userId (mscrm.systemuser)
    /// Output Parameters: result (Edm.String)
    /// On sendForApproval: updates statuscode then sends one consolidated HTML email per project manager.
    /// </summary>
    public class EvaluationAction : IPlugin
    {
        private const int StatusPendingApproval = 755700001;
        private const int StatusApproved        = 2;
        private const int StatusRejected        = 755700002;

        private static readonly char[] CommaSeparator = { ',' };

        public void Execute(IServiceProvider serviceProvider)
        {
            var context        = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory        = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service        = factory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("EvaluationAction plugin started.");

            try
            {
                string action          = GetRequiredString(context, "action");
                string selectedRecords = GetRequiredString(context, "selectedRecords");

                tracingService.Trace("Action: {0} | SelectedRecords: {1}", action, selectedRecords);

                int targetStatus = ResolveTargetStatus(action);

                // Parse and validate IDs
                var validIds = new List<Guid>();
                int skipped  = 0;
                foreach (string raw in selectedRecords.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    string cleaned = raw.Trim().Trim('{', '}');
                    if (Guid.TryParse(cleaned, out Guid id))
                        validIds.Add(id);
                    else
                    {
                        tracingService.Trace("Skipping invalid GUID: '{0}'", raw);
                        skipped++;
                    }
                }

                if (validIds.Count == 0)
                    throw new InvalidPluginExecutionException("No valid record IDs found in selectedRecords.");

                // Update statuscode on each evaluation record
                foreach (Guid recordId in validIds)
                {
                    tracingService.Trace("Updating {0} ({1}) -> statuscode={2}", Evaluation.EntityName, recordId, targetStatus);
                    var update = new Entity(Evaluation.EntityName, recordId);
                    update[Evaluation.StatusReason] = new OptionSetValue(targetStatus);
                    if (targetStatus == StatusApproved)
                        update[Evaluation.Status] = new OptionSetValue((int)Evaluation.Status_OptionSet.Inactive);
                    service.Update(update);
                }

                string result = string.Format("{0} record(s) updated successfully.", validIds.Count);
                if (skipped > 0)
                    result += string.Format(" {0} invalid ID(s) skipped.", skipped);

                // Send PM notification emails only on sendForApproval
                if (action.Equals("sendForApproval", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        int emailsSent = SendPMNotifications(service, tracingService, context.UserId, validIds);
                        if (emailsSent > 0)
                            result += string.Format(" {0} project manager notification email(s) sent.", emailsSent);
                        else
                            result += " No PM emails sent (records may have no assigned project manager).";
                    }
                    catch (Exception emailEx)
                    {
                        tracingService.Trace("Email sending failed (non-fatal): {0}", emailEx.ToString());
                        result += " Warning: project manager notifications could not be sent.";
                    }
                }

                tracingService.Trace("Result: {0}", result);
                context.OutputParameters["result"] = result;
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace("EvaluationAction unhandled error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Error in EvaluationAction: " + ex.Message, ex);
            }
        }

        // ─── Email Orchestration ──────────────────────────────────────────────────

        private int SendPMNotifications(IOrganizationService service, ITracingService trace,
                                         Guid senderId, List<Guid> recordIds)
        {
            trace.Trace("Fetching evaluation records with PM info for {0} ID(s).", recordIds.Count);

            string fetchXml = BuildFetchXml(recordIds);
            trace.Trace("FetchXML:\n{0}", fetchXml);

            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            trace.Trace("FetchXML returned {0} row(s).", results.Entities.Count);

            if (results.Entities.Count == 0)
                return 0;

            var pmGroups = GroupByPM(results.Entities, trace);
            trace.Trace("PM groups: {0}", pmGroups.Count);

            int sent = 0;
            foreach (var group in pmGroups.Values)
            {
                if (string.IsNullOrWhiteSpace(group.PMEmail))
                {
                    trace.Trace("Skipping PM '{0}' — no email address on file.", group.PMName);
                    continue;
                }

                SendEmail(service, trace, senderId, group);
                sent++;
            }

            return sent;
        }

        // ─── FetchXML ─────────────────────────────────────────────────────────────

        private static string BuildFetchXml(List<Guid> recordIds)
        {
            var idNodes = new StringBuilder();
            foreach (Guid id in recordIds)
                idNodes.AppendFormat("<value>{0}</value>", id.ToString("D").ToUpperInvariant());

            return string.Format(@"
<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='{0}'>
    <attribute name='{1}' />
    <attribute name='{2}' />
    <attribute name='{3}' />
    <attribute name='{4}' />
    <order attribute='{1}' descending='false' />
    <filter type='and'>
      <condition attribute='{1}' operator='in'>
        {5}
      </condition>
    </filter>
    <link-entity name='bookableresource' from='bookableresourceid' to='{6}' link-type='inner' alias='res'>
      <attribute name='name' />
    </link-entity>
    <link-entity name='bcc_project' from='bcc_projectid' to='{7}' link-type='inner' alias='proj'>
      <attribute name='bcc_name' />
      <filter type='and'>
        <condition attribute='bcc_pmsmanagerid' operator='not-null' />
      </filter>
      <link-entity name='systemuser' from='systemuserid' to='bcc_pmsmanagerid' link-type='inner' alias='pm'>
        <attribute name='systemuserid' />
        <attribute name='fullname' />
        <attribute name='internalemailaddress' />
      </link-entity>
    </link-entity>
  </entity>
</fetch>",
                Evaluation.EntityName,      // {0}  bcc_evaluation
                Evaluation.PrimaryKey,      // {1}  bcc_evaluationid
                Evaluation.PrimaryName,     // {2}  bcc_name
                Evaluation.Month,           // {3}  bcc_month
                Evaluation.Year,            // {4}  bcc_year
                idNodes.ToString(),         // {5}  <value>…</value> nodes
                Evaluation.BookableResource,// {6}  bcc_bookableresourceid
                Evaluation.Project          // {7}  bcc_projectid
            );
        }

        // ─── Grouping ─────────────────────────────────────────────────────────────

        private static Dictionary<Guid, PMGroup> GroupByPM(
            DataCollection<Entity> entities, ITracingService trace)
        {
            var groups = new Dictionary<Guid, PMGroup>();

            foreach (var entity in entities)
            {
                var pmIdAlias = GetAliasedValue(entity, "pm.systemuserid");
                if (pmIdAlias == null) continue;

                var pmId = (Guid)pmIdAlias;

                if (!groups.TryGetValue(pmId, out PMGroup pmGroup))
                {
                    pmGroup = new PMGroup
                    {
                        PMId    = pmId,
                        PMName  = GetAliasedString(entity, "pm.fullname"),
                        PMEmail = GetAliasedString(entity, "pm.internalemailaddress"),
                        Records = new List<EvaluationRow>()
                    };
                    groups[pmId] = pmGroup;
                    trace.Trace("PM found: {0} <{1}>", pmGroup.PMName, pmGroup.PMEmail);
                }

                // Month — OptionSetValue; use formatted value when available
                string monthLabel = "-";
                if (entity.Contains(Evaluation.Month) && entity[Evaluation.Month] != null)
                {
                    monthLabel = entity.FormattedValues.Contains(Evaluation.Month)
                        ? entity.FormattedValues[Evaluation.Month]
                        : ((OptionSetValue)entity[Evaluation.Month]).Value.ToString();
                }

                pmGroup.Records.Add(new EvaluationRow
                {
                    EmployeeName = GetAliasedString(entity, "res.name"),
                    ProjectName  = GetAliasedString(entity, "proj.bcc_name"),
                    Month        = monthLabel,
                    Year         = GetString(entity, Evaluation.Year)
                });
            }

            return groups;
        }

        // ─── Email Sending ────────────────────────────────────────────────────────

        private static void SendEmail(IOrganizationService service, ITracingService trace,
                                       Guid senderId, PMGroup group)
        {
            string subject = string.Format(
                "Evaluation Pending Approval — {0}", DateTime.UtcNow.ToString("dd MMM yyyy"));

            var email = new Entity("email");
            email["subject"]     = subject;
            email["description"] = BuildEmailBody(group);

            var fromParty = new Entity("activityparty");
            fromParty["partyid"] = new EntityReference("systemuser", senderId);
            email["from"] = new EntityCollection(new[] { fromParty });

            var toParty = new Entity("activityparty");
            toParty["partyid"] = new EntityReference("systemuser", group.PMId);
            email["to"] = new EntityCollection(new[] { toParty });

            Guid emailId = service.Create(email);
            trace.Trace("Email activity created: {0} — sending to {1}", emailId, group.PMEmail);

            var sendRequest = new OrganizationRequest("SendEmail");
            sendRequest["EmailId"]       = emailId;
            sendRequest["TrackingToken"] = string.Empty;
            sendRequest["IssueSend"]     = true;
            service.Execute(sendRequest);
        }

        // ─── HTML Builder ─────────────────────────────────────────────────────────

        private static string BuildEmailBody(PMGroup group)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style=\"font-family:Segoe UI,Arial,sans-serif;font-size:13px;color:#222;\">");

            sb.AppendFormat("<p>Dear <strong>{0}</strong>,</p>", Encode(group.PMName));
            sb.Append("<p>The following evaluation record(s) have been submitted and are pending your approval.</p>");

            sb.Append("<table border=\"1\" cellpadding=\"7\" cellspacing=\"0\" "
                    + "style=\"border-collapse:collapse;width:100%;max-width:800px;\">");

            sb.Append("<thead>");
            sb.Append("<tr style=\"background-color:#1F4E79;color:#ffffff;text-align:left;\">");
            foreach (string col in new[] { "#", "Employee", "Project", "Month", "Year" })
                sb.AppendFormat("<th style=\"padding:8px;\">{0}</th>", col);
            sb.Append("</tr></thead>");

            sb.Append("<tbody>");
            for (int i = 0; i < group.Records.Count; i++)
            {
                EvaluationRow row     = group.Records[i];
                string        bgColor = i % 2 == 0 ? "#ffffff" : "#EBF3FB";
                sb.AppendFormat("<tr style=\"background-color:{0};\">", bgColor);
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>", i + 1);
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>", Encode(row.EmployeeName));
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>", Encode(row.ProjectName));
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>", Encode(row.Month));
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>", Encode(row.Year));
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");

            sb.Append("<br/>");
            sb.Append("<p>Please log in to the system to review and approve or reject these records.</p>");
            sb.Append("<p style=\"color:#888;font-size:11px;\">This is an automated notification. Please do not reply to this email.</p>");
            sb.Append("</body></html>");

            return sb.ToString();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static string GetRequiredString(IPluginExecutionContext context, string paramName)
        {
            if (!context.InputParameters.Contains(paramName) || context.InputParameters[paramName] == null)
                throw new InvalidPluginExecutionException(
                    string.Format("Input parameter '{0}' is required.", paramName));

            string value = context.InputParameters[paramName].ToString();
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidPluginExecutionException(
                    string.Format("Input parameter '{0}' must not be empty.", paramName));

            return value;
        }

        private static int ResolveTargetStatus(string action)
        {
            switch (action.ToLowerInvariant())
            {
                case "sendforapproval": return StatusPendingApproval;
                case "approve":        return StatusApproved;
                case "reject":         return StatusRejected;
                default:
                    throw new InvalidPluginExecutionException(
                        string.Format("Unknown action '{0}'. Valid values: sendForApproval, approve, reject.", action));
            }
        }

        private static object GetAliasedValue(Entity entity, string key)
        {
            if (!entity.Contains(key)) return null;
            var aliased = entity[key] as AliasedValue;
            return aliased?.Value;
        }

        private static string GetAliasedString(Entity entity, string key)
        {
            object val = GetAliasedValue(entity, key);
            return val != null ? val.ToString() : "-";
        }

        private static string GetString(Entity entity, string attribute)
        {
            return entity.Contains(attribute) && entity[attribute] != null
                ? entity[attribute].ToString()
                : "-";
        }

        private static string Encode(string value)
        {
            if (string.IsNullOrEmpty(value)) return "-";
            return value
                .Replace("&",  "&amp;")
                .Replace("<",  "&lt;")
                .Replace(">",  "&gt;")
                .Replace("\"", "&quot;");
        }

        // ─── Inner Types ──────────────────────────────────────────────────────────

        private class PMGroup
        {
            public Guid              PMId    { get; set; }
            public string            PMName  { get; set; }
            public string            PMEmail { get; set; }
            public List<EvaluationRow> Records { get; set; }
        }

        private class EvaluationRow
        {
            public string EmployeeName { get; set; }
            public string ProjectName  { get; set; }
            public string Month        { get; set; }
            public string Year         { get; set; }
        }
    }
}
