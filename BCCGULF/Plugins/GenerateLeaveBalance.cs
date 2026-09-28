using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class GenerateLeaveBalance : IPlugin
    {
        private const string AnnualLeaveKeyword = "Annual Leave";

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

            var bookableResource = (Entity)context.InputParameters["Target"];

            if (bookableResource.LogicalName != BookableResource.EntityName)
                return;

            try
            {
                tracingService.Trace("GenerateLeaveBalance: Plugin execution started.");

                // ── Step 1: Get Gender from the Bookable Resource ──
                if (!bookableResource.Contains(BookableResource.Gender))
                {
                    tracingService.Trace("Gender is not set on the Bookable Resource. Exiting.");
                    return;
                }

                int genderValue = bookableResource.GetAttributeValue<OptionSetValue>(BookableResource.Gender).Value;
                tracingService.Trace("Gender value: {0}", genderValue);

                // ── Step 2a: Get Type (Staff / Worker) from the Bookable Resource ──
                if (!bookableResource.Contains(BookableResource.Type) || bookableResource[BookableResource.Type] == null)
                {
                    tracingService.Trace("Type is not set on the Bookable Resource. Exiting.");
                    return;
                }

                int typeValue = bookableResource.GetAttributeValue<OptionSetValue>(BookableResource.Type).Value;
                tracingService.Trace("Type value: {0}", typeValue);

                // ── Step 2b: Get Joining Date (used for Annual Leave pro-ration) ──
                // Check both JoiningDate (bcc_joiningdate) and DateofJoining (new_dateofjoining)
                DateTime joiningDate = DateTime.MinValue;

                if (bookableResource.Contains(BookableResource.JoiningDate) && bookableResource[BookableResource.JoiningDate] != null)
                {
                    joiningDate = bookableResource.GetAttributeValue<DateTime>(BookableResource.JoiningDate).AddHours(6);
                }
                else if (bookableResource.Contains(BookableResource.DateofJoining) && bookableResource[BookableResource.DateofJoining] != null)
                {
                    joiningDate = bookableResource.GetAttributeValue<DateTime>(BookableResource.DateofJoining).AddHours(6);
                }

                tracingService.Trace("Joining Date: {0}", joiningDate != DateTime.MinValue ? joiningDate.ToString("yyyy-MM-dd") : "Not Set");

                // ── Step 3: Fetch Leave Types using FetchXML ──
                // bcc_genderapplicableto is a multi-select picklist, use "contain-values" operator.
                // Retrieve active Leave Types where the applicable gender contains the resource's gender
                // and the Type matches the resource's Type (Staff / Worker).
                string fetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <attribute name='{3}' />
                        <attribute name='{4}' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='{2}' operator='contain-values'>
                            <value>{5}</value>
                          </condition>
                          <condition attribute='{6}' operator='eq' value='{7}' />
                        </filter>
                      </entity>
                    </fetch>",
                    LeaveType.EntityName,        // {0} bcc_leavetype
                    LeaveType.PrimaryName,       // {1} bcc_name
                    LeaveType.GenderApplicableTo, // {2} bcc_genderapplicableto
                    LeaveType.LeaveDays,         // {3} bcc_leavedays
                    LeaveType.PrimaryKey,        // {4} bcc_leavetypeid
                    genderValue,                 // {5} gender option value
                    LeaveType.RessourceType,     // {6} bcc_resourcetype
                    typeValue);                  // {7} type option value

                var leaveTypes = service.RetrieveMultiple(new FetchExpression(fetchXml));
                tracingService.Trace("Leave Types found: {0}", leaveTypes.Entities.Count);

                if (leaveTypes.Entities.Count == 0)
                {
                    tracingService.Trace("No matching Leave Types found. Exiting.");
                    return;
                }

                // ── Step 4: Create a Leave Balance record for each Leave Type ──
                foreach (var leaveType in leaveTypes.Entities)
                {
                    string leaveTypeName = leaveType.GetAttributeValue<string>(LeaveType.PrimaryName) ?? "Leave";
                    int totalLeaveDays = leaveType.Contains(LeaveType.LeaveDays)
                        ? leaveType.GetAttributeValue<int>(LeaveType.LeaveDays)
                        : 0;

                    decimal balanceDays = totalLeaveDays;

                    // ── Pro-rate Annual Leave based on Joining Date ──
                    bool isAnnualLeave = leaveTypeName.IndexOf(AnnualLeaveKeyword, StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isAnnualLeave && joiningDate != DateTime.MinValue)
                    {
                        if (joiningDate.Day <= 15)
                            balanceDays = typeValue == 100000001 ? 1.83m : 2.5m;
                        else
                            balanceDays = 0;
                    }

                    tracingService.Trace("Creating Leave Balance - Type: {0}, Total: {1}, Balance: {2}",
                        leaveTypeName, totalLeaveDays, balanceDays);

                    var leaveBalance = new Entity(LeaveBalance.EntityName);
                    leaveBalance[LeaveBalance.PrimaryName] = leaveTypeName;
                    leaveBalance[LeaveBalance.BookableResource] = new EntityReference(BookableResource.EntityName, bookableResource.Id);
                    leaveBalance[LeaveBalance.LeaveType] = new EntityReference(LeaveType.EntityName, leaveType.Id);
                    leaveBalance[LeaveBalance.TotalLeaves] = balanceDays;
                    leaveBalance[LeaveBalance.UsedLeaves] = 0m;
                    //if (leaveTypeName.Contains("Annual"))
                    //{
                    //    //leaveBalance[LeaveBalance.LeaveBal] = balanceDays;
                    //}
                    service.Create(leaveBalance);
                    tracingService.Trace("Leave Balance created for: {0}", leaveTypeName);
                }

                tracingService.Trace("GenerateLeaveBalance: Plugin execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("GenerateLeaveBalance: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in GenerateLeaveBalance plugin: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Pro-rates the annual leave entitlement based on the remaining calendar days
        /// from the joining date to year-end.
        /// Formula: (Remaining days in year / Total days in year) × Total Annual Leave Days
        /// </summary>
        private static int CalculateProRatedAnnualLeave(int totalAnnualDays, DateTime joiningDate, ITracingService tracingService)
        {
            int year = joiningDate.Year;
            var yearEnd = new DateTime(year, 12, 31);

            int totalDaysInYear = DateTime.IsLeapYear(year) ? 366 : 365;
            int remainingDaysInYear = (yearEnd - joiningDate.Date).Days + 1; // includes joining day

            decimal proRatedDays = Math.Round((remainingDaysInYear / (decimal)totalDaysInYear) * totalAnnualDays, 2);

            tracingService.Trace(
                "Annual Leave pro-ration - Year: {0}, Total days: {1}, Remaining: {2}, Entitled: {3}, Pro-rated: {4}",
                year, totalDaysInYear, remainingDaysInYear, totalAnnualDays, proRatedDays);
            int days = (int)Math.Ceiling(proRatedDays); // Round up to nearest whole day
            return days;
        }
    }
}
