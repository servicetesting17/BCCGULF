using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCCGULF.CustomAPI
{
    /// <summary>
    /// Custom Action handler for bcc_EscalationAction.
    /// Input Parameters : selectedRecords (Edm.String — comma-separated Evaluation GUIDs)
    /// Output Parameters: result (Edm.String)
    /// Creates one bcc_escalation record per evaluation, copying evaluation and bookable resource details.
    /// </summary>
    public class EscalationAction : IPlugin
    {
        private static readonly char[] CommaSeparator = { ',' };

        public void Execute(IServiceProvider serviceProvider)
        {
            var context        = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory        = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service        = factory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("EscalationAction plugin started.");

            try
            {
                string selectedRecords = GetRequiredString(context, "selectedRecords");

                tracingService.Trace("SelectedRecords: {0}", selectedRecords);

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

                // Retrieve evaluation + bookable resource data in one query
                string fetchXml = BuildFetchXml(validIds);
                tracingService.Trace("FetchXML:\n{0}", fetchXml);

                var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                tracingService.Trace("FetchXML returned {0} row(s).", results.Entities.Count);

                if (results.Entities.Count == 0)
                    throw new InvalidPluginExecutionException("No evaluation records found for the provided IDs.");

                // Create one escalation per evaluation
                int created = 0;
                foreach (var entity in results.Entities)
                {
                    Entity escalation = BuildEscalationRecord(entity, tracingService);
                    service.Create(escalation);
                    created++;
                    tracingService.Trace("Escalation created for evaluation {0}.", entity.Id);
                }

                string result = string.Format("{0} escalation record(s) created successfully.", created);
                if (skipped > 0)
                    result += string.Format(" {0} invalid ID(s) skipped.", skipped);

                tracingService.Trace("Result: {0}", result);
                context.OutputParameters["result"] = result;
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace("EscalationAction unhandled error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Error in EscalationAction: " + ex.Message, ex);
            }
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
    <filter type='and'>
      <condition attribute='{1}' operator='in'>
        {7}
      </condition>
    </filter>
    <link-entity name='bookableresource' from='bookableresourceid' to='{6}' link-type='inner' alias='br'>
      <attribute name='{8}' />
      <attribute name='{9}' />
      <attribute name='{10}' />
    </link-entity>
  </entity>
</fetch>",
                Evaluation.EntityName,          // {0}  bcc_evaluation
                Evaluation.PrimaryKey,          // {1}  bcc_evaluationid
                Evaluation.PrimaryName,         // {2}  bcc_name
                Evaluation.Month,               // {3}  bcc_month
                Evaluation.Quater,              // {4}  bcc_quater
                Evaluation.Rating,              // {5}  bcc_rating
                Evaluation.BookableResource,    // {6}  bcc_bookableresourceid
                idNodes.ToString(),             // {7}  <value>…</value> nodes
                BookableResource.Department,    // {8}  bcc_department
                BookableResource.EmployeeID,    // {9}  bcc_userid
                BookableResource.DateofJoining  // {10} new_dateofjoining
            );
        }

        // ─── Record Builder ───────────────────────────────────────────────────────

        private static Entity BuildEscalationRecord(Entity entity, ITracingService trace)
        {
            var escalation = new Entity(Escalation.EntityName);

            // Fields from Evaluation
            if (entity.Contains(Evaluation.PrimaryName))
                escalation[Escalation.PrimaryName] = entity[Evaluation.PrimaryName];

            if (entity.Contains(Evaluation.Month) && entity[Evaluation.Month] != null)
                escalation[Escalation.Month] = entity[Evaluation.Month];

            if (entity.Contains(Evaluation.Quater) && entity[Evaluation.Quater] != null)
                escalation[Escalation.Quater] = entity[Evaluation.Quater];

            if (entity.Contains(Evaluation.Rating) && entity[Evaluation.Rating] != null)
                escalation[Escalation.Rating] = entity[Evaluation.Rating];

            // Lookup to the source Evaluation
            escalation[Escalation.Evaluation] = new EntityReference(Evaluation.EntityName, entity.Id);

            // Lookup to BookableResource
            if (entity.Contains(Evaluation.BookableResource) && entity[Evaluation.BookableResource] != null)
                escalation[Escalation.BookableResource] = (EntityReference)entity[Evaluation.BookableResource];

            // Fields from BookableResource (aliased via FetchXML)
            object dept = GetAliasedValue(entity, "br." + BookableResource.Department);
            if (dept != null)
                escalation[Escalation.Department] = (OptionSetValue)dept;

            object empId = GetAliasedValue(entity, "br." + BookableResource.EmployeeID);
            if (empId != null)
                escalation[Escalation.EmployeeCode] = empId.ToString();

            object doj = GetAliasedValue(entity, "br." + BookableResource.DateofJoining);
            if (doj != null)
                escalation[Escalation.DateOfJoining] = (DateTime)doj;

            trace.Trace("Built escalation record for evaluation '{0}'.", entity.Id);
            return escalation;
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

        private static object GetAliasedValue(Entity entity, string key)
        {
            if (!entity.Contains(key)) return null;
            var aliased = entity[key] as AliasedValue;
            return aliased?.Value;
        }
    }
}
