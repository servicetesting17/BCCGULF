using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace BCCGulf_PayrollConfig_OnUpdate
{
    public class TimeEntryPayrollCalculation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {
                if (!context.InputParameters.Contains("Target"))
                {
                    tracingService.Trace("No valid Target found.");
                    return;
                }

                var targetObj = context.InputParameters["Target"];
                var target = targetObj as Entity;
                if (target == null)
                {
                    tracingService.Trace("Target is not an Entity.");
                    return;
                }

                if (target.LogicalName != "msdyn_timeentry")
                {
                    tracingService.Trace("Not a time entry record. Exiting.");
                    return;
                }

                tracingService.Trace("PLUGIN: Started payroll calculation.");

                //var cols = new ColumnSet("msdyn_duration", "new_overtime1", "new_overtime2", "new_overtime3", "msdyn_bookableresource");
                var timeEntry = context.PostEntityImages["PostImage"];
                tracingService.Trace("PLUGIN: Retrieved time entry post image.");

                decimal duration = timeEntry.GetAttributeValue<decimal?>("bcc_durationinhours") ?? decimal.Zero;
                tracingService.Trace($"PLUGIN: Duration in hours: {duration}");

                decimal overtime1 = timeEntry.GetAttributeValue<decimal?>("new_overtime1") ?? 0m;
                decimal overtime2 = timeEntry.GetAttributeValue<decimal?>("new_overtime2") ?? 0m;
                decimal overtime3 = timeEntry.GetAttributeValue<decimal?>("new_overtime3") ?? 0m;

                if (!timeEntry.Contains("msdyn_bookableresource") || timeEntry["msdyn_bookableresource"] == null)
                {
                    tracingService.Trace("No bookable resource assigned.");
                    return;
                }
                tracingService.Trace("PLUGIN: Bookable resource found.");

                var bookableRef = timeEntry.GetAttributeValue<EntityReference>("msdyn_bookableresource");
                if (bookableRef == null)
                {
                    tracingService.Trace("Bookable resource reference is null.");
                    return;
                }
                tracingService.Trace($"PLUGIN: Bookable resource ID: {bookableRef.Id}");

                var brCols = new ColumnSet("bcc_basichourlyrate", "bcc_ot1hourlyrate", "bcc_ot2hourlyrate", "bcc_ot3hourlyrate");
                var bookable = service.Retrieve("bookableresource", bookableRef.Id, brCols);

                decimal basicRate = bookable.GetAttributeValue<Money>("bcc_basichourlyrate")?.Value ?? 0m;
                decimal ot1Rate = bookable.GetAttributeValue<Money>("bcc_ot1hourlyrate")?.Value ?? 0m;
                decimal ot2Rate = bookable.GetAttributeValue<Money>("bcc_ot2hourlyrate")?.Value ?? 0m;
                decimal ot3Rate = bookable.GetAttributeValue<Money>("bcc_ot3hourlyrate")?.Value ?? 0m;

                tracingService.Trace($"PLUGIN: Payroll calculation inputs - DurationMinutes: {duration}, BasicRate: {basicRate}, OT1Rate: {ot1Rate}, OT2Rate: {ot2Rate}, OT3Rate: {ot3Rate}");

                // convert minutes to hours
                //decimal hours = (duration / 60m;
                decimal basicCost = duration * basicRate;
                decimal ot1Cost = overtime1 * ot1Rate;
                decimal ot2Cost = overtime2 * ot2Rate;
                decimal ot3Cost = overtime3 * ot3Rate;
                decimal totalCost = basicCost + ot1Cost + ot2Cost + ot3Cost;

                tracingService.Trace($"PLUGIN: Payroll calculation completed. Total Cost: {totalCost} Hours: {duration}");

                var update = new Entity("msdyn_timeentry") { Id = target.Id };
                update["bcc_basictotalcost"] = new Money(basicCost);
                update["bcc_overtime1totalcost"] = new Money(ot1Cost);
                update["bcc_overtime2totalcost"] = new Money(ot2Cost);
                update["bcc_overtime3totalcost"] = new Money(ot3Cost);
                update["bcc_totalpayrollcost"] = new Money(totalCost);
                update["bcc_total_hours"] = duration + overtime1 + overtime2 + overtime3;
                service.Update(update);

                tracingService.Trace("PLUGIN: Payroll calculation completed.");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in Payroll Calculation Plugin: " + ex.Message, ex);
            }
        }
    }
}
