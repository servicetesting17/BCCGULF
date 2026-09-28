using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;

namespace BCCGULF.CustomAPI
{
    /// <summary>
    /// Custom API: bcc_CreateAbsentTimeEntries
    ///
    /// Input parameters:
    ///   resourceIds  (String) — comma-separated BookableResource GUIDs
    ///   fromDate     (String) — start date inclusive, YYYY-MM-DD
    ///   toDate       (String) — end date inclusive,   YYYY-MM-DD
    ///   absentReason (Int32)  — AbsentReason_OptionSet value
    ///
    /// Output parameters:
    ///   result (String) — summary message
    /// </summary>
    public class CreateAbsentTimeEntries : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var trace   = (ITracingService)             serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)      serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            trace.Trace("CreateAbsentTimeEntries started.");

            string resourceIdsRaw = GetRequiredString(context, "resourceIds");
            string fromDateStr    = GetRequiredString(context, "fromDate");
            string toDateStr      = GetRequiredString(context, "toDate");
            int    absentReason   = GetRequiredInt(context, "absentReason");

            DateTime fromDate = DateTime.ParseExact(fromDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
            DateTime toDate   = DateTime.ParseExact(toDateStr,   "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;

            if (toDate < fromDate)
                throw new InvalidPluginExecutionException("toDate cannot be earlier than fromDate.");

            string[] resourceIds = resourceIdsRaw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            int created = 0;
            int skipped = 0;

            foreach (string rawId in resourceIds)
            {
                string cleanId = rawId.Trim().Replace("{", "").Replace("}", "");
                if (!Guid.TryParse(cleanId, out Guid resourceGuid))
                {
                    trace.Trace("Skipping invalid GUID: {0}", rawId);
                    continue;
                }

                // Look up project and approver from the BookableResource
                Entity resource;
                try
                {
                    resource = service.Retrieve(
                        BookableResource.EntityName,
                        resourceGuid,
                        new ColumnSet(BookableResource.Project, BookableResource.Manager)
                    );
                }
                catch (Exception ex)
                {
                    trace.Trace("Could not retrieve resource {0}: {1}", cleanId, ex.Message);
                    continue;
                }

                EntityReference projectRef  = resource.Contains(BookableResource.Project)
                    ? resource.GetAttributeValue<EntityReference>(BookableResource.Project)
                    : null;
                EntityReference approverRef = resource.Contains(BookableResource.Manager)
                    ? resource.GetAttributeValue<EntityReference>(BookableResource.Manager)
                    : null;

                // Walk each day in the range
                for (DateTime day = fromDate; day <= toDate; day = day.AddDays(1))
                {
                    string dayStr = day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

                    // Duplicate check
                    var query = new QueryExpression(TimeEntry.EntityName)
                    {
                        ColumnSet = new ColumnSet(TimeEntry.PrimaryKey),
                        TopCount  = 1
                    };
                    query.Criteria.AddCondition(TimeEntry.BookableResource, ConditionOperator.Equal, resourceGuid);
                    query.Criteria.AddCondition(TimeEntry.Date, ConditionOperator.On, day);

                    var existing = service.RetrieveMultiple(query);
                    if (existing.Entities.Count > 0)
                    {
                        trace.Trace("Duplicate on {0} for resource {1} — skipping.", dayStr, cleanId);
                        skipped++;
                        continue;
                    }

                    var record = new Entity(TimeEntry.EntityName);
                    record[TimeEntry.Date]            = day;
                    record[TimeEntry.DayofDate]       = day.ToString("dddd", CultureInfo.InvariantCulture);
                    record[TimeEntry.EntryType]       = new OptionSetValue((int)TimeEntry.EntryType_OptionSet.Absent);
                    record[TimeEntry.AbsentReason]    = new OptionSetValue(absentReason);
                    record[TimeEntry.Duration]        = absentReason == 755700000 ? new decimal(8 * 60) : decimal.Zero;
                    record[TimeEntry.Total_Hours] = absentReason == 755700000 ? new decimal(8) : decimal.Zero;
                    record[TimeEntry.Type]            = new OptionSetValue((int)TimeEntry.Type_OptionSet.Absence);
                    record[TimeEntry.BookableResource] = new EntityReference(BookableResource.EntityName, resourceGuid);

                    if (projectRef != null)
                        record[TimeEntry.Project] = projectRef;

                    if (approverRef != null)
                        record[TimeEntry.Approver] = new EntityReference("systemuser", approverRef.Id);

                    try
                    {
                        service.Create(record);
                        trace.Trace("Created absent entry for resource {0} on {1}.", cleanId, dayStr);
                        created++;
                    }
                    catch (Exception ex)
                    {
                        trace.Trace("Failed to create entry for {0} on {1}: {2}", cleanId, dayStr, ex.Message);
                    }
                }
            }

            string summary = string.Format(
                "Absent time entries: {0} created, {1} skipped (duplicates).",
                created, skipped);
            trace.Trace(summary);

            context.OutputParameters["result"] = summary;
        }

        private static string GetRequiredString(IPluginExecutionContext context, string key)
        {
            if (!context.InputParameters.Contains(key) || !(context.InputParameters[key] is string val))
                throw new InvalidPluginExecutionException(string.Format("Required parameter '{0}' is missing.", key));
            if (string.IsNullOrWhiteSpace(val))
                throw new InvalidPluginExecutionException(string.Format("Required parameter '{0}' is empty.", key));
            return val;
        }

        private static int GetRequiredInt(IPluginExecutionContext context, string key)
        {
            if (!context.InputParameters.Contains(key))
                throw new InvalidPluginExecutionException(string.Format("Required parameter '{0}' is missing.", key));
            if (context.InputParameters[key] is int i) return i;
            throw new InvalidPluginExecutionException(string.Format("Parameter '{0}' is not an integer.", key));
        }
    }
}
