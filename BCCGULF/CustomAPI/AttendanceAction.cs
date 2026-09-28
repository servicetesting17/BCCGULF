using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCCGULF.CustomAPI
{
    /// <summary>
    /// Custom Action handler for bcc_AttendanceAction.
    /// Input Parameters : action (Edm.String), selectedRecords (Edm.String — comma-separated GUIDs), userId (mscrm.systemuser), comment (Edm.String, optional)
    /// Output Parameters: result (Edm.String)
    /// comment is written to bcc_requestercomments on sendForApproval and bcc_approvercomments on reject.
    /// On sendForApproval: updates statuscode then sends one consolidated HTML email per manager.
    /// </summary>
    public class AttendanceAction : IPlugin
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

            tracingService.Trace("AttendanceAction plugin started.");

            try
            {
                string action          = GetRequiredString(context, "action");
                string selectedRecords = GetRequiredString(context, "selectedRecords");
                string comment         = GetOptionalString(context, "comment");

                tracingService.Trace("Action: {0} | SelectedRecords: {1} | HasComment: {2}", action, selectedRecords, !string.IsNullOrWhiteSpace(comment));

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

                // Update statuscode (and comment) on each attendance record
                foreach (Guid recordId in validIds)
                {
                    tracingService.Trace("Updating {0} ({1}) -> statuscode={2}", Attendence.EntityName, recordId, targetStatus);
                    var update = new Entity(Attendence.EntityName, recordId);
                    update[Attendence.StatusReason] = new OptionSetValue(targetStatus);
                    if (targetStatus == 2) // Only set statecode to Inactive if we're approving — otherwise keep as is (e.g. allow sending for approval multiple times)
                        update[Attendence.Status] = new OptionSetValue(Attendence.Status_OptionSet.Inactive.GetHashCode());

                    if (!string.IsNullOrWhiteSpace(comment))
                    {
                        if (action.Equals("sendForApproval", StringComparison.OrdinalIgnoreCase))
                            update["bcc_requestercomments"] = comment;
                        else if (action.Equals("reject", StringComparison.OrdinalIgnoreCase))
                            update["bcc_approvercomments"] = comment;
                    }

                    service.Update(update);
                }

                string result = string.Format("{0} record(s) updated successfully.", validIds.Count);
                if (skipped > 0)
                    result += string.Format(" {0} invalid ID(s) skipped.", skipped);

                // Send manager notification emails only on sendForApproval
                if (action.Equals("sendForApproval", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        int emailsSent = SendManagerNotifications(service, tracingService, context.UserId, validIds);
                        if (emailsSent > 0)
                            result += string.Format(" {0} manager notification email(s) sent.", emailsSent);
                        else
                            result += " No manager emails sent (records may have no assigned manager).";
                    }
                    catch (Exception emailEx)
                    {
                        tracingService.Trace("Email sending failed (non-fatal): {0}", emailEx.ToString());
                        result += " Warning: manager notifications could not be sent.";
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
                tracingService.Trace("AttendanceAction unhandled error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Error in AttendanceAction: " + ex.Message, ex);
            }
        }

        // ─── Email Orchestration ──────────────────────────────────────────────────

        private int SendManagerNotifications(IOrganizationService service, ITracingService trace,
                                              Guid senderId, List<Guid> recordIds)
        {
            trace.Trace("Fetching attendance records with manager info for {0} ID(s).", recordIds.Count);

            string fetchXml = BuildFetchXml(recordIds);
            trace.Trace("FetchXML:\n{0}", fetchXml);

            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            trace.Trace("FetchXML returned {0} row(s).", results.Entities.Count);

            if (results.Entities.Count == 0)
                return 0;

            var managerGroups = GroupByManager(results.Entities, trace);
            trace.Trace("Manager groups: {0}", managerGroups.Count);

            int sent = 0;
            foreach (var group in managerGroups.Values)
            {
                if (string.IsNullOrWhiteSpace(group.ManagerEmail))
                {
                    trace.Trace("Skipping manager '{0}' — no email address on file.", group.ManagerName);
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
    <attribute name='{5}' />
    <attribute name='{6}' />
    <attribute name='{7}' />
    <order attribute='{8}' descending='false' />
    <filter type='and'>
      <condition attribute='{1}' operator='in'>
        {9}
      </condition>
    </filter>
    <link-entity name='{10}' from='{11}' to='{8}' link-type='inner' alias='br'>
      <attribute name='{12}' />
      <filter type='and'>
        <condition attribute='{13}' operator='not-null' />
      </filter>
      <link-entity name='systemuser' from='systemuserid' to='{13}' link-type='inner' alias='mgr'>
        <attribute name='systemuserid' />
        <attribute name='fullname' />
        <attribute name='internalemailaddress' />
      </link-entity>
    </link-entity>
  </entity>
</fetch>",
                Attendence.EntityName,           // {0}  new_biometric
                Attendence.PrimaryKey,            // {1}  new_biometricid
                Attendence.PrimaryName,           // {2}  new_name
                Attendence.Date,                  // {3}  new_date
                Attendence.InTime,                // {4}  bcc_intime
                Attendence.OutTime,               // {5}  bcc_outtime
                Attendence.TotalWorkingHours,     // {6}  bcc_totalworkinghours
                Attendence.OvertimeHours,         // {7}  bcc_overtimehours
                Attendence.Employee_ID,           // {8}  new_employee_id  (also order + link-from)
                idNodes.ToString(),               // {9}  <value>…</value> nodes
                BookableResource.EntityName,      // {10} bookableresource
                BookableResource.PrimaryKey,      // {11} bookableresourceid
                BookableResource.PrimaryName,     // {12} name  (employee display name)
                BookableResource.Manager          // {13} bcc_managerid
            );
        }

        // ─── Grouping ─────────────────────────────────────────────────────────────

        private static Dictionary<Guid, ManagerGroup> GroupByManager(
            DataCollection<Entity> entities, ITracingService trace)
        {
            var groups = new Dictionary<Guid, ManagerGroup>();

            foreach (var entity in entities)
            {
                var managerIdAlias = GetAliasedValue(entity, "mgr.systemuserid");
                if (managerIdAlias == null) continue;

                var managerId = (Guid)managerIdAlias;

                if (!groups.TryGetValue(managerId, out ManagerGroup managerGroup))
                {
                    managerGroup = new ManagerGroup
                    {
                        ManagerId    = managerId,
                        ManagerName  = GetAliasedString(entity, "mgr.fullname"),
                        ManagerEmail = GetAliasedString(entity, "mgr.internalemailaddress"),
                        Records      = new List<AttendanceRow>()
                    };
                    groups[managerId] = managerGroup;
                    trace.Trace("Manager found: {0} <{1}>", managerGroup.ManagerName, managerGroup.ManagerEmail);
                }

                string dateValue = "-";
                if (entity.Contains(Attendence.Date) && entity[Attendence.Date] != null)
                    dateValue = ((DateTime)entity[Attendence.Date]).ToString("dd MMM yyyy");

                managerGroup.Records.Add(new AttendanceRow
                {
                    EmployeeName = GetAliasedString(entity, "br.name"),
                    Date         = dateValue,
                    InTime       = GetString(entity, Attendence.InTime),
                    OutTime      = GetString(entity, Attendence.OutTime),
                    TotalHours   = GetString(entity, Attendence.TotalWorkingHours),
                    Overtime     = GetString(entity, Attendence.OvertimeHours)
                });
            }

            return groups;
        }

        // ─── Email Sending ────────────────────────────────────────────────────────

        private static void SendEmail(IOrganizationService service, ITracingService trace,
                                       Guid senderId, ManagerGroup group)
        {
            string subject = string.Format(
                "Attendance Pending Approval — {0}", DateTime.UtcNow.ToString("dd MMM yyyy"));

            var email = new Entity("email");
            email["subject"]     = subject;
            email["description"] = BuildEmailBody(group);

            var fromParty = new Entity("activityparty");
            fromParty["partyid"] = new EntityReference("systemuser", senderId);
            email["from"] = new EntityCollection(new[] { fromParty });

            var toParty = new Entity("activityparty");
            toParty["partyid"] = new EntityReference("systemuser", group.ManagerId);
            email["to"] = new EntityCollection(new[] { toParty });

            Guid emailId = service.Create(email);
            trace.Trace("Email activity created: {0} — sending to {1}", emailId, group.ManagerEmail);

            var sendRequest = new OrganizationRequest("SendEmail");
            sendRequest["EmailId"]       = emailId;
            sendRequest["TrackingToken"] = string.Empty;
            sendRequest["IssueSend"]     = true;
            service.Execute(sendRequest);
        }

        // ─── HTML Builder ─────────────────────────────────────────────────────────

        private static string BuildEmailBody(ManagerGroup group)
        {
            var sb = new StringBuilder();
            sb.Append("<html><body style=\"font-family:Segoe UI,Arial,sans-serif;font-size:13px;color:#222;\">");

            sb.AppendFormat("<p>Dear <strong>{0}</strong>,</p>", Encode(group.ManagerName));
            sb.Append("<p>The following attendance record(s) have been submitted and are pending your approval.</p>");

            // Table
            sb.Append("<table border=\"1\" cellpadding=\"7\" cellspacing=\"0\" "
                    + "style=\"border-collapse:collapse;width:100%;max-width:800px;\">");

            // Header row
            sb.Append("<thead>");
            sb.Append("<tr style=\"background-color:#1F4E79;color:#ffffff;text-align:left;\">");
            foreach (string col in new[] { "#", "Employee", "Date", "In Time", "Out Time", "Total Hours", "Overtime" })
                sb.AppendFormat("<th style=\"padding:8px;\">{0}</th>", col);
            sb.Append("</tr></thead>");

            // Data rows
            sb.Append("<tbody>");
            for (int i = 0; i < group.Records.Count; i++)
            {
                AttendanceRow row    = group.Records[i];
                string        bgColor = i % 2 == 0 ? "#ffffff" : "#EBF3FB";
                sb.AppendFormat("<tr style=\"background-color:{0};\">", bgColor);
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>",  i + 1);
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>",  Encode(row.EmployeeName));
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>",  Encode(row.Date));
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>",  Encode(row.InTime));
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>",  Encode(row.OutTime));
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>",  Encode(row.TotalHours));
                sb.AppendFormat("<td style=\"padding:7px;\">{0}</td>",  Encode(row.Overtime));
                sb.Append("</tr>");
            }
            sb.Append("</tbody></table>");

            sb.Append("<br/>");
            sb.Append("<p>Please review and approve or reject these records by clicking the link below:</p>");
            sb.Append("<p><a href=\"https://bccgulf.crm15.dynamics.com/main.aspx?appid=ba98fb8b-88b5-f011-bbd2-6045bd6aa790&amp;pagetype=entitylist&amp;etn=new_biometric&amp;viewid=5500cdc0-f35f-f111-a826-00224869fbcc&amp;viewType=1039\" "
                    + "style=\"background-color:#0078d4;color:#ffffff;padding:8px 16px;text-decoration:none;border-radius:4px;font-weight:600;display:inline-block;\">Open Attendance Records</a></p>");
            sb.Append("<p style=\"color:#888;font-size:11px;\">This is an automated notification. Please do not reply to this email.</p>");
            sb.Append("</body></html>");

            return sb.ToString();
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static string GetOptionalString(IPluginExecutionContext context, string paramName)
        {
            if (!context.InputParameters.Contains(paramName) || context.InputParameters[paramName] == null)
                return null;

            string value = context.InputParameters[paramName].ToString();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

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

        private class ManagerGroup
        {
            public Guid             ManagerId    { get; set; }
            public string           ManagerName  { get; set; }
            public string           ManagerEmail { get; set; }
            public List<AttendanceRow> Records   { get; set; }
        }

        private class AttendanceRow
        {
            public string EmployeeName { get; set; }
            public string Date         { get; set; }
            public string InTime       { get; set; }
            public string OutTime      { get; set; }
            public string TotalHours   { get; set; }
            public string Overtime     { get; set; }
        }
    }
}
