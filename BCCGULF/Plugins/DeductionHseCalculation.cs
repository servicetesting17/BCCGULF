using System;
using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;


namespace BCCGULF
{
    public class DeductionHseCalculation : IPlugin
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
                    tracingService.Trace("No Target in input parameters.");
                    return;
                }

                var target = context.InputParameters["Target"] as Entity;
                if (target == null)
                {
                    tracingService.Trace("Target is not an Entity.");
                    return;
                }

                if (target.LogicalName != Deduction.EntityName)
                {
                    tracingService.Trace("Not a Deduction record. Exiting.");
                    return;
                }

                // Only proceed when Days is present on the Target (Create or Update execution)
                if (!target.Contains(Deduction.Days))
                {
                    tracingService.Trace("Days field not present on Target. Exiting.");
                    return;
                }

                decimal days = target.GetAttributeValue<decimal?>(Deduction.Days) ?? 0m;
                tracingService.Trace($"Days: {days}");

                // Determine Bookable Resource reference from Target or PreImage
                EntityReference brRef = null;
                if (target.Contains(Deduction.BookableResource))
                {
                    brRef = target.GetAttributeValue<EntityReference>(Deduction.BookableResource);
                }
                else if (context.PostEntityImages != null && context.PostEntityImages.Contains("PostImage"))
                {
                    var pre = context.PostEntityImages["PostImage"];
                    if (pre != null && pre.Contains(Deduction.BookableResource))
                        brRef = pre.GetAttributeValue<EntityReference>(Deduction.BookableResource);
                }

                if (brRef == null)
                {
                    tracingService.Trace("No Bookable Resource found on Deduction record. Exiting.");
                    return;
                }

                var cols = new ColumnSet(BookableResource.TotalSalary);
                var bookable = service.Retrieve(BookableResource.EntityName, brRef.Id, cols);

                decimal totalSalary = 0m;
                if (bookable != null && bookable.Contains(BookableResource.TotalSalary))
                {
                    // BookableResource.TotalSalary is a Decimal attribute
                    totalSalary = bookable.GetAttributeValue<decimal?>(BookableResource.TotalSalary) ?? 0m;
                }

                tracingService.Trace($"Bookable Resource TotalSalary: {totalSalary}");

                decimal hseDeduction = 0m;
                if (totalSalary > 0m && days > 0m)
                {
                    hseDeduction = (totalSalary / 30.41m) * days;
                    hseDeduction = Math.Round(hseDeduction, 2, MidpointRounding.AwayFromZero);
                }

                var update = new Entity(Deduction.EntityName) { Id = target.Id };
                update[Deduction.HSEDeduction] = hseDeduction;
                service.Update(update);

                tracingService.Trace($"HSE Deduction set to: {hseDeduction}");
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error in DeductionHseCalculation plugin: " + ex.Message, ex);
            }
        }
    }
}
