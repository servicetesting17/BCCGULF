using System;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace BCCGulf_PayrollConfig_OnUpdate
{
    public class PreventDuplicateTimeEntry : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            tracingService.Trace("PreventDuplicateTimeEntry plugin execution started.");

            // Only handle Create and Update messages
            if (context.MessageName != "Create" && context.MessageName != "Update")
                return;

            if (!context.InputParameters.Contains("Target"))
            {
                tracingService.Trace("No Target in input parameters. Exiting.");
                return;
            }

            var target = context.InputParameters["Target"] as Entity;
            if (target == null || target.LogicalName != "msdyn_timeentry")
                return;

            try
            {
                // Determine Bookable Resource and date to check
                EntityReference bookableResource = null;
                DateTime? entryDate = null;

                if (context.MessageName == "Create")
                {
                    if (!target.Contains("msdyn_bookableresource") || !target.Contains("new_date"))
                    {
                        tracingService.Trace("Required fields not present in Create. Exiting.");
                        return;
                    }

                    bookableResource = target.GetAttributeValue<EntityReference>("msdyn_bookableresource");
                    entryDate = target.GetAttributeValue<DateTime>("new_date");
                }
                else // Update
                {
                    bool resourceUpdated = target.Contains("msdyn_bookableresource");
                    bool dateUpdated = target.Contains("new_date");

                    if (!resourceUpdated && !dateUpdated)
                    {
                        tracingService.Trace("Neither resource nor date is being updated. Exiting.");
                        return;
                    }

                    // Try to get values from the target first, then fallback to pre-image
                    Entity preImage = null;
                    if (context.PreEntityImages != null && context.PreEntityImages.Contains("PreImage"))
                        preImage = context.PreEntityImages["PreImage"];

                    if (resourceUpdated)
                        bookableResource = target.GetAttributeValue<EntityReference>("msdyn_bookableresource");
                    else if (preImage != null && preImage.Contains("msdyn_bookableresource"))
                        bookableResource = preImage.GetAttributeValue<EntityReference>("msdyn_bookableresource");

                    if (dateUpdated)
                        entryDate = target.GetAttributeValue<DateTime>("new_date");
                    else if (preImage != null && preImage.Contains("new_date"))
                        entryDate = preImage.GetAttributeValue<DateTime>("new_date");

                    if (bookableResource == null || !entryDate.HasValue)
                    {
                        tracingService.Trace("Could not determine resource or date. Exiting.");
                        return;
                    }
                }

                if (bookableResource == null || !entryDate.HasValue)
                {
                    tracingService.Trace("Resource or Date is null. Exiting.");
                    return;
                }

                // Convert to Dubai local date (UTC +4) and build UTC range for that local date
                var dubaiDateTime = entryDate.Value.AddHours(4);
                var dubaiDate = dubaiDateTime.Date;

                tracingService.Trace("Resource Id: {0}", bookableResource.Id);
                tracingService.Trace("UTC DateTime: {0}", entryDate.Value.ToString("yyyy-MM-dd HH:mm:ss"));
                tracingService.Trace("Dubai DateTime: {0}", dubaiDateTime.ToString("yyyy-MM-dd HH:mm:ss"));
                tracingService.Trace("Dubai Date: {0}", dubaiDate.ToString("yyyy-MM-dd"));

                var rangeStartUtc = dubaiDate.AddHours(-4); // start of dubaiDate in UTC
                var rangeEndUtc = dubaiDate.AddDays(1).AddHours(-4).AddSeconds(-1); // end of dubaiDate in UTC

                tracingService.Trace("Query Range UTC: {0} to {1}", rangeStartUtc.ToString("yyyy-MM-dd HH:mm:ss"), rangeEndUtc.ToString("yyyy-MM-dd HH:mm:ss"));

                // Query for existing time entries for the same resource within the computed UTC range
                var query = new QueryExpression("msdyn_timeentry")
                {
                    ColumnSet = new ColumnSet("msdyn_timeentryid"),
                    TopCount = 2,
                    Criteria = new FilterExpression()
                };

                query.Criteria.AddCondition("msdyn_bookableresource", ConditionOperator.Equal, bookableResource.Id);
                query.Criteria.AddCondition("new_date", ConditionOperator.OnOrAfter, rangeStartUtc);
                query.Criteria.AddCondition("new_date", ConditionOperator.OnOrBefore, rangeEndUtc);
                query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

                // Exclude current record when updating
                if (context.MessageName == "Update")
                {
                    query.Criteria.AddCondition("msdyn_timeentryid", ConditionOperator.NotEqual, target.Id);
                    tracingService.Trace("Update mode: excluding current record {0} from duplicate check", target.Id);
                }

                var results = service.RetrieveMultiple(query);

                if (results.Entities.Any())
                {
                    var existingId = results.Entities.First().Id;
                    tracingService.Trace("Duplicate found! Existing record ID: {0}", existingId);
                    throw new InvalidPluginExecutionException($"A time entry already exists for this resource on {dubaiDate.ToString("dd-MMM-yyyy")}. Only one entry per resource per date is allowed. Select another date or change the Bookable Resource.");
                }
                var Duration = target.GetAttributeValue<int?>("msdyn_duration") ?? 0;
                tracingService.Trace("Duration: {0}", Duration);
                if (Duration >= 480)
                {
                    target["bcc_durationinhours"] = 8.00m;
                }
                else
                {
                    target["bcc_durationinhours"] = Duration / 60m;
                }
                
                tracingService.Trace("No duplicate found. Allowing operation.");
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace("ERROR: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("Error in PreventDuplicateTimeEntry plugin: " + ex.Message, ex);
            }
        }
    }
}
