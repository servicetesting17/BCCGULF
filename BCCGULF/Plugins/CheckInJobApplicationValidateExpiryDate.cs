using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using System;

namespace BCCGULF.Plugins
{
    public class CheckInJobApplicationValidateExpiryDate : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters == null || !context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
            {
                return;
            }
            var target = context.PostEntityImages["PostImage"];

            if (target.LogicalName != JobApplication.EntityName)
                return;

            try
            {
                tracingService.Trace("CheckInJobApplicationValidateExpiryDate: Plugin execution started.");

                // ── Step 1: Get ApplicationFrom (country origin) from target ──
                if (!target.Contains(JobApplication.ApplicationFrom) || target[JobApplication.ApplicationFrom] == null)
                {
                    return;                  
                }

                var applicationFrom = target.GetAttributeValue<OptionSetValue>(JobApplication.ApplicationFrom).Value;               

                // ── Step 2: Get ExpiryDate from target ──
                if (!target.Contains(JobApplication.ExpiryDate) || target[JobApplication.ExpiryDate] == null)
                {
                    tracingService.Trace("ExpiryDate is not set on the Job Application. Exiting.");
                    return;
                }

                DateTime expiryDate = target.GetAttributeValue<DateTime>(JobApplication.ExpiryDate).AddHours(6);
                tracingService.Trace("ExpiryDate: {0}", expiryDate.ToString("dd/MM/yyyy"));

                // ── Step 3: Get the OptionSet value for ApplicationFrom ──
                // Note: ApplicationFrom is a lookup field, we need to get its OptionSet value
                // For this plugin, we'll assume the description or name indicates inside/outside country
                // Alternative: We retrieve the BookableResource to check a field that indicates country
                // For now, we'll check the name or use a custom field on the lookup

                var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = serviceFactory.CreateOrganizationService(context.UserId);
                // ── Step 4: Calculate days remaining until expiry ──
                DateTime today = DateTime.Today;
                TimeSpan daysRemaining = expiryDate.Date - today;
                int daysRemainingInt = (int)daysRemaining.TotalDays;

                tracingService.Trace("Today: {0}, Days Remaining: {1}", today.ToString("dd/MM/yyyy"), daysRemainingInt);

                // ── Step 5: Validate based on ApplicationFrom ──
                int requiredDays = 0;
                string countryType = string.Empty;

                if (applicationFrom == (int)JobApplication.ApplicatioFrom_OptionSet.InsideCountry)
                {
                    requiredDays = 240;
                    countryType = "Inside Country";
                }
                else if (applicationFrom == (int)JobApplication.ApplicatioFrom_OptionSet.OutsideCountry)
                {
                    requiredDays = 300;
                    countryType = "Outside Country";
                }
                else
                {
                    tracingService.Trace("Unknown ApplicationFrom value: {0}", applicationFrom);
                    throw new InvalidPluginExecutionException("Invalid Application From value.");
                }

                tracingService.Trace("Validation: Country Type = {0}, Required Days = {1}, Actual Days = {2}",
                    countryType, requiredDays, daysRemainingInt);

                // ── Step 6: Throw error if expiry date does not meet requirement ──
                if (daysRemainingInt <= requiredDays)
                {
                    tracingService.Trace("Expiry date validation failed. Country: {0}, Required: {1} days, Actual: {2} days", countryType, requiredDays, daysRemainingInt);
                    throw new InvalidPluginExecutionException(
                        string.Format(
                            "Job Application Check-In failed. For {0} applications, the Expiry Date must be at more than {1} days from today. " +
                            "Current Expiry Date ({2}) only has {3} day(s) remaining.",
                            countryType, requiredDays, expiryDate.ToString("dd/MM/yyyy"), daysRemainingInt));
                }

                tracingService.Trace("Expiry date validation passed. Days remaining ({0}) meets requirement ({1} days).",
                    daysRemainingInt, requiredDays);
                tracingService.Trace("CheckInJobApplicationValidateExpiryDate: Plugin execution completed successfully.");
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace("CheckInJobApplicationValidateExpiryDate: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in CheckInJobApplicationValidateExpiryDate plugin: " + ex.Message, ex);
            }
        }
    }
}
