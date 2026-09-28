using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class OnUpdateGratuityOpeningBalance : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters == null
                || !context.InputParameters.Contains("Target")
                || !(context.InputParameters["Target"] is Entity))
            {
                return;
            }

            var target = (Entity)context.InputParameters["Target"];

            if (target.LogicalName != BookableResource.EntityName)
                return;

            try
            {
                tracingService.Trace("OnUpdateGratuityOpeningBalance: Plugin execution started.");

                // ── Step 1: Check if Gratuity Opening Balance was updated ──
                if (!target.Contains(BookableResource.GratuityOpeningBalance))
                {
                    tracingService.Trace("GratuityOpeningBalance not in target attributes. Exiting.");
                    return;
                }

                decimal gratuityOpeningBalance = 0m;
                if (target[BookableResource.GratuityOpeningBalance] != null)
                {
                    gratuityOpeningBalance = target.GetAttributeValue<Money>(BookableResource.GratuityOpeningBalance).Value;
                }

                tracingService.Trace("Gratuity Opening Balance: {0}", gratuityOpeningBalance);

                // ── Step 2: Get all active Gratuity Provisions for this resource and sum Amount ──
                string provisionFetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='{2}' operator='eq' value='{3}' />
                        </filter>
                      </entity>
                    </fetch>",
                    GratuityProvision.EntityName,       // {0} bcc_gratuityprovision
                    GratuityProvision.Amount,            // {1} bcc_amount
                    GratuityProvision.BookableResource,  // {2} bcc_bookableresourceid
                    target.Id);                          // {3} bookable resource id

                var provisionRecords = service.RetrieveMultiple(new FetchExpression(provisionFetchXml));
                tracingService.Trace("Gratuity Provision records found: {0}", provisionRecords.Entities.Count);

                decimal totalProvisionAmount = 0m;
                foreach (var provision in provisionRecords.Entities)
                {
                    totalProvisionAmount += provision.GetAttributeValue<Money>(GratuityProvision.Amount).Value;
                }

                tracingService.Trace("Total Gratuity Provision amount: {0}", totalProvisionAmount);

                // ── Step 3: Accumulated Gratuity = Opening Balance + Sum of Provisions ──
                decimal accumulatedGratuity = gratuityOpeningBalance + totalProvisionAmount;

                tracingService.Trace("Opening Balance: {0}, Total Provisions: {1}, Accumulated Gratuity: {2}",
                    gratuityOpeningBalance, totalProvisionAmount, accumulatedGratuity);
                var ResourceUpdate = new Entity(BookableResource.EntityName)
                {
                    Id = target.Id
                };

                // ── Step 4: Set Accumulated Gratuity on the same target (Pre-Operation) ──
                ResourceUpdate[BookableResource.AccumulatedGratuity] = new Money(accumulatedGratuity);
                service.Update(ResourceUpdate);


                tracingService.Trace("AccumulatedGratuity set to: {0}", accumulatedGratuity);
                tracingService.Trace("OnUpdateGratuityOpeningBalance: Plugin execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnUpdateGratuityOpeningBalance: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in OnUpdateGratuityOpeningBalance plugin: " + ex.Message, ex);
            }
        }
    }
}
