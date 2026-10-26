using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class OnApproveLeaveRequest : IPlugin
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

            if (target.LogicalName != LeaveRequest.EntityName)
                return;

            try
            {
                tracingService.Trace("OnApproveLeaveRequest: Plugin execution started.");

                // ── Step 1: Check if StatusReason changed to Approved ──
                if (!target.Contains(LeaveRequest.StatusReason))
                {
                    tracingService.Trace("StatusReason not in target attributes. Exiting.");
                    return;
                }

                int statusReason = target.GetAttributeValue<OptionSetValue>(LeaveRequest.StatusReason).Value;
                tracingService.Trace("StatusReason value: {0}", statusReason);

                if (statusReason != (int)LeaveRequest.StatusReason_OptionSet.HRApproved)
                {
                    tracingService.Trace("StatusReason is not Approved ({0}). Exiting.", (int)LeaveRequest.StatusReason_OptionSet.HRApproved);
                    return;
                }

                // ── Step 2: Get Post Image ──
                if (!context.PostEntityImages.Contains("PostImage") || !(context.PostEntityImages["PostImage"] is Entity))
                {
                    tracingService.Trace("PostImage not found. Exiting.");
                    return;
                }

                var postImage = context.PostEntityImages["PostImage"];
                tracingService.Trace("PostImage retrieved for Leave Request: {0}", postImage.Id);

                // ── Step 3: Get BookableResource and LeaveType lookups ──
                if (!postImage.Contains(LeaveRequest.BookableResource) || postImage[LeaveRequest.BookableResource] == null)
                {
                    tracingService.Trace("BookableResource is not set on the Leave Request. Exiting.");
                    return;
                }

                if (!postImage.Contains(LeaveRequest.LeaveType) || postImage[LeaveRequest.LeaveType] == null)
                {
                    tracingService.Trace("LeaveType is not set on the Leave Request. Exiting.");
                    return;
                }

                var bookableResourceRef = postImage.GetAttributeValue<EntityReference>(LeaveRequest.BookableResource);
                var leaveTypeRef = postImage.GetAttributeValue<EntityReference>(LeaveRequest.LeaveType);

                tracingService.Trace("BookableResource: {0}, LeaveType: {1}", bookableResourceRef.Id, leaveTypeRef.Id);

                // ── Step 3a: Copy approved Leave Start / End dates to the Bookable Resource ──
                var resourceUpdate = new Entity(BookableResource.EntityName, bookableResourceRef.Id);
                resourceUpdate[BookableResource.LeaveStartDate] = postImage.GetAttributeValue<DateTime?>(LeaveRequest.StartTime);
                resourceUpdate[BookableResource.LeaveEndDate] = postImage.GetAttributeValue<DateTime?>(LeaveRequest.EndTime);
                service.Update(resourceUpdate);
                tracingService.Trace("Bookable Resource leave dates set. Start: {0}, End: {1}",
                    resourceUpdate[BookableResource.LeaveStartDate], resourceUpdate[BookableResource.LeaveEndDate]);

                // ── Step 4: Get LeaveDays (null → 0) ──
                decimal leaveDays = postImage.Contains(LeaveRequest.LeaveDays) && postImage[LeaveRequest.LeaveDays] != null
                    ? postImage.GetAttributeValue<decimal>(LeaveRequest.LeaveDays)
                    : 0m;

                tracingService.Trace("LeaveDays from approved request: {0}", leaveDays);

                // ── Step 5: Query Leave Balance matching BookableResource AND LeaveType ──
                string fetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <attribute name='{7}' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='{3}' operator='eq' value='{5}' />
                          <condition attribute='{4}' operator='eq' value='{6}' />
                        </filter>
                      </entity>
                    </fetch>",
                    LeaveBalance.EntityName,       // {0} bcc_leavebalance
                    LeaveBalance.PrimaryKey,        // {1} bcc_leavebalanceid
                    LeaveBalance.UsedLeaves,        // {2} bcc_usedleaves
                    LeaveBalance.BookableResource,  // {3} bcc_bookableresourceid
                    LeaveBalance.LeaveType,         // {4} bcc_leavetypeid
                    bookableResourceRef.Id,         // {5} bookable resource id
                    leaveTypeRef.Id,                // {6} leave type id
                    LeaveBalance.TotalLeaves);      // {7} bcc_totalleaves

                var leaveBalances = service.RetrieveMultiple(new FetchExpression(fetchXml));
                tracingService.Trace("Leave Balance records found: {0}", leaveBalances.Entities.Count);

                // ── Compensatory Leave: approval EARNS leave — credit Total Leaves instead of adding to Used Leaves ──
                string leaveTypeName = Common.Common.GetLeaveTypeName(service, leaveTypeRef);
                if (Common.Common.IsCompensatoryLeave(leaveTypeName))
                {
                    CreditCompensatoryLeave(service, leaveBalances, bookableResourceRef, leaveTypeRef, leaveTypeName, leaveDays, tracingService);
                    tracingService.Trace("OnApproveLeaveRequest: Plugin execution completed successfully.");
                    return;
                }

                if (leaveBalances.Entities.Count == 0)
                {
                    tracingService.Trace("No matching Leave Balance found for this resource and leave type. Exiting.");
                    return;
                }

                var leaveBalance = leaveBalances.Entities[0];
                tracingService.Trace("Leave Balance record found: {0}", leaveBalance.Id);

                // ── Step 6: Calculate new UsedLeaves = (existing ?? 0) + leaveDays ──
                decimal existingUsedLeaves = leaveBalance.Contains(LeaveBalance.UsedLeaves) && leaveBalance[LeaveBalance.UsedLeaves] != null
                    ? leaveBalance.GetAttributeValue<decimal>(LeaveBalance.UsedLeaves)
                    : 0m;

                decimal newUsedLeaves = existingUsedLeaves + leaveDays;

                tracingService.Trace("Existing UsedLeaves: {0}, LeaveDays: {1}, New UsedLeaves: {2}",
                    existingUsedLeaves, leaveDays, newUsedLeaves);

                // ── Step 7: Update the Leave Balance record ──
                var updateEntity = new Entity(LeaveBalance.EntityName, leaveBalance.Id);
                updateEntity[LeaveBalance.UsedLeaves] = newUsedLeaves;
                service.Update(updateEntity);

                tracingService.Trace("Leave Balance updated successfully. UsedLeaves set to: {0}", newUsedLeaves);

               

                tracingService.Trace("OnApproveLeaveRequest: Plugin execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnApproveLeaveRequest: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in OnApproveLeaveRequest plugin: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Adds the approved days to Total Leaves of the resource's Compensatory Leave balance.
        /// Creates the balance record when the resource doesn't have one yet.
        /// </summary>
        private static void CreditCompensatoryLeave(IOrganizationService service, EntityCollection leaveBalances,
            EntityReference bookableResourceRef, EntityReference leaveTypeRef, string leaveTypeName,
            decimal leaveDays, ITracingService tracingService)
        {
            if (leaveBalances.Entities.Count == 0)
            {
                var newBalance = new Entity(LeaveBalance.EntityName);
                newBalance[LeaveBalance.PrimaryName] = string.IsNullOrEmpty(leaveTypeName) ? "Compensatory Leave" : leaveTypeName;
                newBalance[LeaveBalance.BookableResource] = new EntityReference(BookableResource.EntityName, bookableResourceRef.Id);
                newBalance[LeaveBalance.LeaveType] = new EntityReference(LeaveType.EntityName, leaveTypeRef.Id);
                newBalance[LeaveBalance.TotalLeaves] = leaveDays;
                newBalance[LeaveBalance.UsedLeaves] = 0m;
                service.Create(newBalance);

                tracingService.Trace("Compensatory Leave balance created with Total Leaves: {0}", leaveDays);
                return;
            }

            var leaveBalance = leaveBalances.Entities[0];
            decimal existingTotalLeaves = leaveBalance.GetAttributeValue<decimal?>(LeaveBalance.TotalLeaves) ?? 0m;
            decimal newTotalLeaves = existingTotalLeaves + leaveDays;

            var updateEntity = new Entity(LeaveBalance.EntityName, leaveBalance.Id);
            updateEntity[LeaveBalance.TotalLeaves] = newTotalLeaves;
            service.Update(updateEntity);

            tracingService.Trace("Compensatory Leave credited. Existing Total Leaves: {0}, Approved Days: {1}, New Total Leaves: {2}",
                existingTotalLeaves, leaveDays, newTotalLeaves);
        }
    }
}