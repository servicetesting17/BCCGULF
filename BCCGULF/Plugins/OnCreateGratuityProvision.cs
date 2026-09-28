using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class OnCreateGratuityProvision : IPlugin
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

            if (target.LogicalName != GratuityProvision.EntityName)
                return;

            try
            {
                tracingService.Trace("OnCreateGratuityProvision: Plugin execution started.");

                // ── Step 1: Get BookableResource from Gratuity Provision ──
                if (!target.Contains(GratuityProvision.BookableResource) || target[GratuityProvision.BookableResource] == null)
                {
                    tracingService.Trace("BookableResource is not set on Gratuity Provision. Exiting.");
                    return;
                }

                var bookableResourceRef = target.GetAttributeValue<EntityReference>(GratuityProvision.BookableResource);
                tracingService.Trace("BookableResource: {0}", bookableResourceRef.Id);

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
                    bookableResourceRef.Id);             // {3} bookable resource id

                var provisionRecords = service.RetrieveMultiple(new FetchExpression(provisionFetchXml));
                tracingService.Trace("Gratuity Provision records found: {0}", provisionRecords.Entities.Count);

                decimal totalProvisionAmount = 0m;
                foreach (var provision in provisionRecords.Entities)
                {
                    totalProvisionAmount += provision.GetAttributeValue<Money>(GratuityProvision.Amount).Value;
                }

                // On Post-Operation the current record is already saved and included in the query.
                //// On Pre-Operation the current record is not yet saved, so add it manually.
                //if (context.Stage == 20)
                //{
                //    var currentAmount = target.GetAttributeValue<decimal?>(GratuityProvision.Amount) ?? 0m;
                //    totalProvisionAmount += currentAmount;
                //    tracingService.Trace("PreOperation detected. Current Gratuity Provision amount added: {0}", currentAmount);
                //}

                tracingService.Trace("Total Gratuity Provision amount: {0}", totalProvisionAmount);

                // ── Step 3: Get Gratuity Opening Balance from Bookable Resource ──
                var resource = service.Retrieve(
                    BookableResource.EntityName,
                    bookableResourceRef.Id,
                    new ColumnSet(BookableResource.GratuityOpeningBalance));

                decimal gratuityOpeningBalance = 0m;
                if (resource.Contains(BookableResource.GratuityOpeningBalance) && resource[BookableResource.GratuityOpeningBalance] != null)
                {
                    gratuityOpeningBalance = resource.GetAttributeValue<Money>(BookableResource.GratuityOpeningBalance).Value;
                }

                tracingService.Trace("Gratuity Opening Balance: {0}", gratuityOpeningBalance);

                // ── Step 4: Accumulated Gratuity = Opening Balance + Sum of Provisions ──
                decimal accumulatedGratuity = gratuityOpeningBalance + totalProvisionAmount;

                tracingService.Trace("Opening Balance: {0}, Total Provisions: {1}, Accumulated Gratuity: {2}",
                    gratuityOpeningBalance, totalProvisionAmount, accumulatedGratuity);

                // ── Step 5: Update Accumulated Gratuity on Bookable Resource ──
                var updateResource = new Entity(BookableResource.EntityName, bookableResourceRef.Id);
                updateResource[BookableResource.AccumulatedGratuity] = new Money(accumulatedGratuity);
                service.Update(updateResource);

                tracingService.Trace("Bookable Resource updated. Accumulated Gratuity set to: {0}", accumulatedGratuity);
                tracingService.Trace("OnCreateGratuityProvision: Plugin execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnCreateGratuityProvision: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in OnCreateGratuityProvision plugin: " + ex.Message, ex);
            }
        }
    }
}
