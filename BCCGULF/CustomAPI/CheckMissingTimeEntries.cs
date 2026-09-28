using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Text;

namespace BCCGULF.CustomAPI
{
    /// <summary>
    /// Custom Action: bcc_CheckMissingTimeEntries
    /// Input:  resourceIds (Edm.String — comma-separated bookable resource GUIDs)
    ///         checkDate   (Edm.String — YYYY-MM-DD)
    /// Output: hasMissing           (Edm.Boolean)
    ///         missingResourcesJson (Edm.String — JSON array: [{name, bcc_userid}])
    /// Resources whose bcc_joiningdate equals checkDate are excluded from the missing list.
    /// </summary>
    public class CheckMissingTimeEntries : IPlugin
    {
        private static readonly char[] CommaSeparator = { ',' };

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            trace.Trace("CheckMissingTimeEntries started.");

            try
            {
                string resourceIdsParam = GetRequiredString(context, "resourceIds");
                string checkDate = GetRequiredString(context, "checkDate");

                trace.Trace("resourceIds: {0} | checkDate: {1}", resourceIdsParam, checkDate);

                // Parse resource GUIDs
                var resourceGuids = new List<Guid>();
                foreach (string raw in resourceIdsParam.Split(CommaSeparator, StringSplitOptions.RemoveEmptyEntries))
                {
                    string cleaned = raw.Trim().Trim('{', '}');
                    if (Guid.TryParse(cleaned, out Guid id))
                        resourceGuids.Add(id);
                    else
                        trace.Trace("Skipping invalid GUID: '{0}'", raw);
                }

                if (resourceGuids.Count == 0)
                {
                    context.OutputParameters["hasMissing"] = false;
                    context.OutputParameters["missingResourcesJson"] = "[]";
                    return;
                }

                // Step 1: which resources already have a time entry on checkDate
                var withEntry = GetResourcesWithTimeEntry(service, trace, resourceGuids, checkDate);
                trace.Trace("Resources with entry on {0}: {1}", checkDate, withEntry.Count);

                // Step 2: full details for all requested resources
                var allResources = GetResourceDetails(service, resourceGuids, trace);
                trace.Trace("Total resources fetched: {0}", allResources.Count);

                // Parse checkDate once for joining-date comparison
                DateTime checkDateParsed = DateTime.Parse(checkDate, System.Globalization.CultureInfo.InvariantCulture).Date;

                // Step 3: missing = in the list, no entry found, and not their joining date
                var missing = new List<ResourceInfo>();
                foreach (var res in allResources)
                {
                    if (withEntry.Contains(res.Id))
                        continue;
                    trace.Trace("Resource '{0}' has no entry on {1} and checkDateParsed {2} Joining Date {3}.", res.Name, checkDate, checkDateParsed.AddDays(1).Date, res.JoiningDate.Value.Date);
                    if (res.JoiningDate.Value.Date == checkDateParsed.AddDays(1).Date)
                    {
                        trace.Trace("Skipping '{0}' — joining date matches checkDate ({1}).", res.Name, checkDate);
                        continue;
                    }

                    missing.Add(res);
                }

                trace.Trace("Missing: {0}", missing.Count);

                context.OutputParameters["hasMissing"] = missing.Count > 0;
                context.OutputParameters["missingResourcesJson"] = SerializeToJson(missing);
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                trace.Trace("CheckMissingTimeEntries error: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Error in CheckMissingTimeEntries: " + ex.Message, ex);
            }
        }

        // ─── Queries ──────────────────────────────────────────────────────────────

        private static HashSet<Guid> GetResourcesWithTimeEntry(
            IOrganizationService service, ITracingService trace,
            List<Guid> resourceGuids, string checkDate)
        {
            var idNodes = new StringBuilder();
            foreach (Guid id in resourceGuids)
                idNodes.AppendFormat("<value>{0}</value>", id.ToString("D").ToUpperInvariant());

            string fetchXml = string.Format(@"
<fetch version='1.0' mapping='logical' distinct='true'>
  <entity name='msdyn_timeentry'>
    <attribute name='msdyn_bookableresource' />
    <filter type='and'>
      <condition attribute='new_date' operator='on' value='{0}' />
      <condition attribute='msdyn_bookableresource' operator='in'>
        {1}
      </condition>
    </filter>
  </entity>
</fetch>", checkDate, idNodes);

            trace.Trace("Time entry FetchXML:\n{0}", fetchXml);

            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            var withEntry = new HashSet<Guid>();

            foreach (var entity in result.Entities)
            {
                if (entity.Contains("msdyn_bookableresource") &&
                    entity["msdyn_bookableresource"] is EntityReference er)
                    withEntry.Add(er.Id);
            }

            return withEntry;
        }

        private static List<ResourceInfo> GetResourceDetails(
            IOrganizationService service,
            List<Guid> resourceGuids, ITracingService tracingService)
        {
            var idNodes = new StringBuilder();
            foreach (Guid id in resourceGuids)
                idNodes.AppendFormat("<value>{0}</value>", id.ToString("D").ToUpperInvariant());

            string fetchXml = string.Format(@"
<fetch version='1.0' mapping='logical' distinct='false'>
  <entity name='bookableresource'>
    <attribute name='bookableresourceid' />
    <attribute name='name' />
    <attribute name='bcc_userid' />
    <attribute name='{1}' />
    <filter type='and'>
      <condition attribute='bookableresourceid' operator='in'>
        {0}
      </condition>
    </filter>
  </entity>
</fetch>", idNodes, BookableResource.JoiningDate);

            var result = service.RetrieveMultiple(new FetchExpression(fetchXml));
            var resources = new List<ResourceInfo>();

            foreach (var entity in result.Entities)
            {
                DateTime? joiningDate = null;
                if (entity.Contains(BookableResource.JoiningDate) && entity[BookableResource.JoiningDate] is DateTime jd)
                {
                    joiningDate = jd.AddHours(6).Date;
                    tracingService.Trace("Resource '{0}' joining date " + joiningDate);
                }


                resources.Add(new ResourceInfo
                {
                    Id = entity.Id,
                    Name = entity.Contains("name") ? entity["name"].ToString() : "-",
                    EmployeeId = entity.Contains("bcc_userid") ? entity["bcc_userid"].ToString() : "-",
                    JoiningDate = joiningDate
                });
            }

            return resources;
        }

        // ─── Serialization ────────────────────────────────────────────────────────

        private static string SerializeToJson(List<ResourceInfo> resources)
        {
            var sb = new StringBuilder("[");
            for (int i = 0; i < resources.Count; i++)
            {
                if (i > 0) sb.Append(",");
                sb.AppendFormat("{{\"name\":\"{0}\",\"bcc_userid\":\"{1}\"}}",
                    EscapeJson(resources[i].Name),
                    EscapeJson(resources[i].EmployeeId));
            }
            sb.Append("]");
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

        private static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            return value
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        // ─── Inner Type ───────────────────────────────────────────────────────────

        private class ResourceInfo
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string EmployeeId { get; set; }
            public DateTime? JoiningDate { get; set; }
        }
    }
}
