using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class OnCreateLeaveRequest : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters == null || !context.InputParameters.Contains("Target") || !(context.InputParameters["Target"] is Entity))
            {
                return;
            }

            var target = (Entity)context.InputParameters["Target"];

            if (target.LogicalName != LeaveRequest.EntityName)
                return;

            try
            {
                tracingService.Trace("OnCreateLeaveRequest: Plugin execution started.");
                // ── Step 1: Get BookableResource and LeaveType from target ──
                if (!target.Contains(LeaveRequest.BookableResource) || target[LeaveRequest.BookableResource] == null)
                {
                    tracingService.Trace("BookableResource is not set on the Leave Request. Exiting.");
                    return;
                }

                if (!target.Contains(LeaveRequest.LeaveType) || target[LeaveRequest.LeaveType] == null)
                {
                    tracingService.Trace("LeaveType is not set on the Leave Request. Exiting.");
                    return;
                }

                var bookableResourceRef = target.GetAttributeValue<EntityReference>(LeaveRequest.BookableResource);
                var leaveTypeRef = target.GetAttributeValue<EntityReference>(LeaveRequest.LeaveType);

                tracingService.Trace("BookableResource: {0}, LeaveType: {1}", bookableResourceRef.Id, leaveTypeRef.Id);

                // ── Step 2: Get LeaveDays from target (null → 0) ──
                decimal leaveDays = target.Contains(LeaveRequest.LeaveDays) && target[LeaveRequest.LeaveDays] != null
                    ? target.GetAttributeValue<decimal>(LeaveRequest.LeaveDays)
                    : 0m;

                tracingService.Trace("LeaveDays requested: {0}", leaveDays);

                // ── Step 3: Query Leave Balance matching BookableResource AND LeaveType ──
                string fetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='{3}' operator='eq' value='{5}' />
                          <condition attribute='{4}' operator='eq' value='{6}' />
                        </filter>
                      </entity>
                    </fetch>",
                    LeaveBalance.EntityName,        // {0} bcc_leavebalance
                    LeaveBalance.PrimaryKey,         // {1} bcc_leavebalanceid
                    LeaveBalance.AvailableLeaves,    // {2} bcc_availableleaves
                    LeaveBalance.BookableResource,   // {3} bcc_bookableresourceid
                    LeaveBalance.LeaveType,          // {4} bcc_leavetypeid
                    bookableResourceRef.Id,          // {5} bookable resource id
                    leaveTypeRef.Id);                // {6} leave type id

                var leaveBalances = service.RetrieveMultiple(new FetchExpression(fetchXml));
                tracingService.Trace("Leave Balance records found: {0}", leaveBalances.Entities.Count);

                if (leaveBalances.Entities.Count == 0)
                {
                    tracingService.Trace("No matching Leave Balance found for this resource and leave type.");
                    throw new InvalidPluginExecutionException("No Leave Balance record found for this resource and leave type.");
                }

                var leaveBalance = leaveBalances.Entities[0];
                tracingService.Trace("Leave Balance record found: {0}", leaveBalance.Id);

                // ── Step 4: Get Available Balance (null → 0) ──
                decimal availableBalance = leaveBalance.Contains(LeaveBalance.AvailableLeaves) && leaveBalance[LeaveBalance.AvailableLeaves] != null
                    ? leaveBalance.GetAttributeValue<decimal>(LeaveBalance.AvailableLeaves)
                    : 0m;

                tracingService.Trace("Available Balance: {0}, Requested Days: {1}", availableBalance, leaveDays);

                // ── Step 5: Validate — if requested days exceed available balance, throw error ──
                if (leaveDays > availableBalance)
                {
                    tracingService.Trace("Insufficient balance. Available: {0}, Requested: {1}", availableBalance, leaveDays);
                    throw new InvalidPluginExecutionException(
                        string.Format("Insufficient leave balance. Available balance is {0} day(s), but you requested {1} day(s).", availableBalance, leaveDays));
                }

                // ── Step 6: Validate — overlapping Leave Requests are not allowed ──
                DateTime? newStartTime = target.Contains(LeaveRequest.StartTime) && target[LeaveRequest.StartTime] != null
                    ? target.GetAttributeValue<DateTime?>(LeaveRequest.StartTime)
                    : null;

                DateTime? newEndTime = target.Contains(LeaveRequest.EndTime) && target[LeaveRequest.EndTime] != null
                    ? target.GetAttributeValue<DateTime?>(LeaveRequest.EndTime)
                    : null;

                if (newStartTime.HasValue && newEndTime.HasValue)
                {
                    tracingService.Trace("Checking for overlapping leave requests. Start: {0}, End: {1}",
                        newStartTime.Value.ToString("dd/MM/yyyy"), newEndTime.Value.ToString("dd/MM/yyyy"));

                    // Overlap condition: ExistingStart < NewEnd AND ExistingEnd > NewStart
                    // Only check Active (1) and Approved (2) status — ignore Rejected
                    string overlapFetchXml = string.Format(@"
                        <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>
                          <entity name='{0}'>
                            <attribute name='{1}' />
                            <filter type='and'>
                              <condition attribute='{2}' operator='eq' value='{6}' />
                              <condition attribute='{3}' operator='eq' value='{7}' />
                              <condition attribute='{4}' operator='lt' value='{8}' />
                              <condition attribute='{5}' operator='gt' value='{9}' />
                              <condition attribute='statuscode' operator='in'>
                                <value>{10}</value>
                                <value>{11}</value>
                              </condition>
                            </filter>
                          </entity>
                        </fetch>",
                        LeaveRequest.EntityName,        // {0}
                        LeaveRequest.PrimaryKey,        // {1}
                        LeaveRequest.BookableResource,  // {2}
                        LeaveRequest.LeaveType,         // {3}
                        LeaveRequest.StartTime,         // {4} ExistingStart < NewEnd
                        LeaveRequest.EndTime,           // {5} ExistingEnd > NewStart
                        bookableResourceRef.Id,         // {6}
                        leaveTypeRef.Id,                // {7}
                        newEndTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"),   // {8}
                        newStartTime.Value.ToString("yyyy-MM-ddTHH:mm:ssZ"), // {9}
                        (int)LeaveRequest.StatusReason_OptionSet.PendingApproval,     // {10}
                        (int)LeaveRequest.StatusReason_OptionSet.HRApproved);  // {11}

                    var overlappingRequests = service.RetrieveMultiple(new FetchExpression(overlapFetchXml));
                    tracingService.Trace("Overlapping Leave Requests found: {0}", overlappingRequests.Entities.Count);

                    if (overlappingRequests.Entities.Count > 0)
                    {
                        tracingService.Trace("Overlapping leave request detected. Blocking creation.");
                        throw new InvalidPluginExecutionException(
                            string.Format("An overlapping leave request already exists for this resource and leave type between {0} and {1}.",
                                newStartTime.Value.ToString("dd/MM/yyyy"), newEndTime.Value.ToString("dd/MM/yyyy")));
                    }

                    tracingService.Trace("No overlapping leave requests found.");
                }

                // ── Step 7: Set bcc_name = ResourceName - LeaveTypeName - StartDate - EndDate - LeaveDays ──
                string resourceName = bookableResourceRef.Name ?? string.Empty;
                string leaveTypeName = leaveTypeRef.Name ?? string.Empty;

                // If EntityReference.Name is not populated, retrieve the names
                if (string.IsNullOrEmpty(resourceName))
                {
                    var resource = service.Retrieve(BookableResource.EntityName, bookableResourceRef.Id, new ColumnSet(BookableResource.PrimaryName));
                    resourceName = resource.GetAttributeValue<string>(BookableResource.PrimaryName) ?? string.Empty;
                    tracingService.Trace("Retrieved Resource Name: {0}", resourceName);
                }

                if (string.IsNullOrEmpty(leaveTypeName))
                {
                    var leaveTypeEntity = service.Retrieve(LeaveType.EntityName, leaveTypeRef.Id, new ColumnSet(LeaveType.PrimaryName));
                    leaveTypeName = leaveTypeEntity.GetAttributeValue<string>(LeaveType.PrimaryName) ?? string.Empty;
                    tracingService.Trace("Retrieved Leave Type Name: {0}", leaveTypeName);
                }

                string startTime = target.Contains(LeaveRequest.StartTime) && target[LeaveRequest.StartTime] != null
                    ? target.GetAttributeValue<DateTime>(LeaveRequest.StartTime).ToString("dd/MM/yyyy")
                    : string.Empty;

                string endTime = target.Contains(LeaveRequest.EndTime) && target[LeaveRequest.EndTime] != null
                    ? target.GetAttributeValue<DateTime>(LeaveRequest.EndTime).ToString("dd/MM/yyyy")
                    : string.Empty;

                string name = string.Format("{0} - {1} - {2} - {3} - {4}", resourceName, leaveTypeName, startTime, endTime, leaveDays);
                target[LeaveRequest.PrimaryName] = name;
                // ———— Step 8: Fetch previous HR-approved leave request for this booker ————
                string previousLeaveFetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <attribute name='{3}' />
                        <filter type='and'>
                          <condition attribute='{4}' operator='eq' value='{5}' />
                          <condition attribute='{6}' operator='eq' value='2' />
                          <condition attribute='{7}' operator='eq' value='{8}' />
                        </filter>
                        <order attribute='createdon' descending='true' />
                      </entity>
                    </fetch>",
                    LeaveRequest.EntityName,           // {0} bcc_leaverequest
                    LeaveRequest.StartTime,            // {1} bcc_starttime
                    LeaveRequest.EndTime,              // {2} bcc_endtime
                    LeaveRequest.LeaveDays,            // {3} bcc_leavedays
                    LeaveRequest.BookableResource,     // {4} bcc_bookableresourceid
                    bookableResourceRef.Id,            // {5} booker id
                    LeaveRequest.StatusReason,          // {6} statuscode (2 = HRApproved),
                    LeaveRequest.LeaveType,
                    leaveTypeRef.Id
                    );                     

                var previousLeaveRequests = service.RetrieveMultiple(new FetchExpression(previousLeaveFetchXml));
                tracingService.Trace("Previous HR-approved leave requests found: {0}", previousLeaveRequests.Entities.Count);

                if (previousLeaveRequests.Entities.Count > 0)
                {
                    var previousLeaveRequest = previousLeaveRequests.Entities[0];
                    tracingService.Trace("Previous leave request found: {0}", previousLeaveRequest.Id);

                    // Extract dates and days from previous request
                    DateTime? previousStartTime = previousLeaveRequest.Contains(LeaveRequest.StartTime) && previousLeaveRequest[LeaveRequest.StartTime] != null
                        ? previousLeaveRequest.GetAttributeValue<DateTime>(LeaveRequest.StartTime)
                        : (DateTime?)null;

                    DateTime? previousEndTime = previousLeaveRequest.Contains(LeaveRequest.EndTime) && previousLeaveRequest[LeaveRequest.EndTime] != null
                        ? previousLeaveRequest.GetAttributeValue<DateTime>(LeaveRequest.EndTime)
                        : (DateTime?)null;

                    decimal previousLeaveDays = previousLeaveRequest.Contains(LeaveRequest.LeaveDays) && previousLeaveRequest[LeaveRequest.LeaveDays] != null
                        ? previousLeaveRequest.GetAttributeValue<decimal>(LeaveRequest.LeaveDays)
                        : 0m;

                    tracingService.Trace("Previous leave data - StartTime: {0}, EndTime: {1}, LeaveDays: {2}",
                        previousStartTime ?? DateTime.MinValue, previousEndTime ?? DateTime.MinValue, previousLeaveDays);

                    //// Update current leave request with previous leave information
                    //var leaveRequestUpdateEntity = new Entity(LeaveRequest.EntityName, postImage.Id);

                    if (previousStartTime.HasValue)
                        target[LeaveRequest.PreviousLeaveDateFrom] = previousStartTime.Value;

                    if (previousEndTime.HasValue)
                        target[LeaveRequest.PreviousLeaveDateTo] = previousEndTime.Value;

                    target[LeaveRequest.PreviousLeaveDays] = previousLeaveDays;
                    tracingService.Trace("Current leave request updated with previous leave information.");
                }
                else
                {
                    tracingService.Trace("No previous HR-approved leave request found for this booker. Skipping previous leave data update.");
                }

                tracingService.Trace("bcc_name set to: {0}", name);

                tracingService.Trace("Balance check passed. Available: {0}, Requested: {1}", availableBalance, leaveDays);
                tracingService.Trace("OnCreateLeaveRequest: Plugin execution completed successfully.");
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnCreateLeaveRequest: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in OnCreateLeaveRequest plugin: " + ex.Message, ex);
            }
        }
    }
}
