using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class OnCreateLeaveProvision : IPlugin
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

            if (target.LogicalName != LeaveProvision.EntityName)
                return;

            try
            {
                tracingService.Trace("OnCreateLeaveProvision: Plugin execution started.");

                if (!target.Contains(LeaveProvision.BookableResource) || target[LeaveProvision.BookableResource] == null)
                {
                    tracingService.Trace("BookableResource is not set on Leave Provision. Exiting.");
                    return;
                }

                var bookableResourceRef = target.GetAttributeValue<EntityReference>(LeaveProvision.BookableResource);
                tracingService.Trace("BookableResource: {0}", bookableResourceRef.Id);

                // ── Step 2: Query all active Leave Provisions for this resource ──
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
                    LeaveProvision.EntityName,       // {0} bcc_leaveprovision
                    LeaveProvision.LeaveDays,        // {1} bcc_leavedays
                    LeaveProvision.BookableResource,  // {2} bcc_bookableresourceid
                    bookableResourceRef.Id);          // {3} bookable resource id

                var leaveProvisionRecords = service.RetrieveMultiple(new FetchExpression(provisionFetchXml));
                tracingService.Trace("Leave Provision records found: {0}", leaveProvisionRecords.Entities.Count);

                decimal totalProvisionLeaveDays = target.GetAttributeValue<decimal?>(LeaveProvision.LeaveDays) ?? 0m;
                //foreach (var leaveProvision in leaveProvisionRecords.Entities)
                //{
                //    totalProvisionLeaveDays += leaveProvision.GetAttributeValue<decimal?>(LeaveProvision.LeaveDays) ?? 0m;
                //}

                //// On Pre-Operation the current record is not yet saved, so add it manually
                //if (context.Stage == 20)
                //{
                //    var currentLeaveDays = target.GetAttributeValue<decimal?>(LeaveProvision.LeaveDays) ?? 0m;
                //    totalProvisionLeaveDays += currentLeaveDays;
                //    tracingService.Trace("PreOperation detected. Current Leave Provision days added: {0}", currentLeaveDays);
                //}

                tracingService.Trace("Total Leave Provision days: {0}", totalProvisionLeaveDays);

                // ── Step 3: Get Annual Leave Balance for this resource ──
                string balanceFetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='{3}' operator='eq' value='{5}' />
                        </filter>
                        <link-entity name='{4}' from='{6}' to='{7}' link-type='inner'>
                          <filter type='and'>
                            <condition attribute='{8}' operator='like' value='%Annual Leave%' />
                            <condition attribute='statecode' operator='eq' value='0' />
                          </filter>
                        </link-entity>
                      </entity>
                    </fetch>",
                    LeaveBalance.EntityName,        // {0} bcc_leavebalance
                    LeaveBalance.PrimaryKey,         // {1} bcc_leavebalanceid
                    LeaveBalance.TotalLeaves,    // {2} bcc_totalleaves
                    LeaveBalance.BookableResource,   // {3} bcc_bookableresourceid
                    LeaveType.EntityName,            // {4} bcc_leavetype
                    bookableResourceRef.Id,          // {5} bookable resource id
                    LeaveType.PrimaryKey,            // {6} bcc_leavetypeid (from)
                    LeaveBalance.LeaveType,          // {7} bcc_leavetypeid (to)
                    LeaveType.PrimaryName);          // {8} bcc_name

                var leaveBalances = service.RetrieveMultiple(new FetchExpression(balanceFetchXml));
                tracingService.Trace("Annual Leave Balance records found: {0}", leaveBalances.Entities.Count);

                if (leaveBalances.Entities.Count == 0)
                {
                    tracingService.Trace("No Annual Leave Balance found for this resource. Creating Annual Leave Balance records.");

                    // ── Retrieve Bookable Resource with Gender, Type, and Joining Date ──
                    var bookableResource = service.Retrieve(
                        BookableResource.EntityName,
                        bookableResourceRef.Id,
                        new ColumnSet(BookableResource.Gender, BookableResource.Type, BookableResource.JoiningDate, BookableResource.DateofJoining));

                    tracingService.Trace("Bookable Resource retrieved: {0}", bookableResource.Id);

                    // ── Check if Gender and Type are set ──
                    if (!bookableResource.Contains(BookableResource.Gender) || bookableResource[BookableResource.Gender] == null)
                    {
                        tracingService.Trace("Gender is not set on the Bookable Resource. Cannot create Annual Leave Balance.");
                        return;
                    }

                    if (!bookableResource.Contains(BookableResource.Type) || bookableResource[BookableResource.Type] == null)
                    {
                        tracingService.Trace("Type is not set on the Bookable Resource. Cannot create Annual Leave Balance.");
                        return;
                    }

                    int genderValue = bookableResource.GetAttributeValue<OptionSetValue>(BookableResource.Gender).Value;
                    int typeValue = bookableResource.GetAttributeValue<OptionSetValue>(BookableResource.Type).Value;
                    tracingService.Trace("Gender: {0}, Type: {1}", genderValue, typeValue);

                    // ── Get Joining Date ──
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

                    // ── Fetch Annual Leave Types using FetchXML ──
                    string annualLeaveFetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <attribute name='{3}' />
                        <attribute name='{4}' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='{1}' operator='like' value='%Annual Leave%' />
                          <condition attribute='{2}' operator='contain-values'>
                            <value>{5}</value>
                          </condition>
                          <condition attribute='{6}' operator='eq' value='{7}' />
                        </filter>
                      </entity>
                    </fetch>",
                        LeaveType.EntityName,         // {0} bcc_leavetype
                        LeaveType.PrimaryName,        // {1} bcc_name
                        LeaveType.GenderApplicableTo, // {2} bcc_genderapplicableto
                        LeaveType.LeaveDays,          // {3} bcc_leavedays
                        LeaveType.PrimaryKey,         // {4} bcc_leavetypeid
                        genderValue,                  // {5} gender option value
                        LeaveType.RessourceType,      // {6} bcc_resourcetype
                        typeValue);                   // {7} type option value

                    var annualLeaveTypes = service.RetrieveMultiple(new FetchExpression(annualLeaveFetchXml));
                    tracingService.Trace("Annual Leave Types found: {0}", annualLeaveTypes.Entities.Count);

                    if (annualLeaveTypes.Entities.Count == 0)
                    {
                        tracingService.Trace("No matching Annual Leave Types found. Cannot create Annual Leave Balance.");
                        return;
                    }

                    // ── Create Annual Leave Balance record(s) ──
                    foreach (var leaveType in annualLeaveTypes.Entities)
                    {
                        string leaveTypeName = leaveType.GetAttributeValue<string>(LeaveType.PrimaryName) ?? "Annual Leave";
                        decimal balanceDays = 1.83m; // Default to 1.83 days/month (22 days/year) if LeaveDays is not set
                        tracingService.Trace("Creating Annual Leave Balance - Type: {0}, Total: {1}, ", leaveTypeName, balanceDays);
                        var newLeaveBalance = new Entity(LeaveBalance.EntityName);
                        newLeaveBalance[LeaveBalance.PrimaryName] = leaveTypeName;
                        newLeaveBalance[LeaveBalance.BookableResource] = new EntityReference(BookableResource.EntityName, bookableResource.Id);
                        newLeaveBalance[LeaveBalance.LeaveType] = new EntityReference(LeaveType.EntityName, leaveType.Id);
                        newLeaveBalance[LeaveBalance.TotalLeaves] = balanceDays;
                        newLeaveBalance[LeaveBalance.UsedLeaves] = 0m;
                        service.Create(newLeaveBalance);
                        tracingService.Trace("Annual Leave Balance created for: {0}", leaveTypeName);
                    }
                    return;
                }

                var leaveBalanceRecord = leaveBalances.Entities[0];
                decimal existingAvailableLeaves = leaveBalanceRecord.GetAttributeValue<decimal?>(LeaveBalance.TotalLeaves) ?? 0m;
                decimal newAvailableLeaves = existingAvailableLeaves + totalProvisionLeaveDays;

                tracingService.Trace("Existing Available Leaves: {0}, Total Provision Leave Days: {1}, New Available Leaves: {2}",
                    existingAvailableLeaves, totalProvisionLeaveDays, newAvailableLeaves);

                var updateLeaveBalance = new Entity(LeaveBalance.EntityName, leaveBalanceRecord.Id);
                updateLeaveBalance[LeaveBalance.TotalLeaves] = newAvailableLeaves;
                service.Update(updateLeaveBalance);

                tracingService.Trace("Leave Balance updated successfully. Total Leaves set to: {0}", newAvailableLeaves);
                tracingService.Trace("OnCreateLeaveProvision: Plugin execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnCreateLeaveProvision: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in OnCreateLeaveProvision plugin: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Pro-rates the annual leave entitlement based on the remaining calendar days
        /// from the joining date to year-end.
        /// Formula: (Remaining days in year / Total days in year) × Total Annual Leave Days
        /// </summary>

    }
}
