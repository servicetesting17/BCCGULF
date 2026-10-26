using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    /// <summary>
    /// Leave Request — Update of bcc_attendance (Pre-Operation, filtering attribute: bcc_attendance).
    /// For Compensatory Leave, sets Number of Days (bcc_leavedays) to the count of attendance
    /// records in the bcc_attendance JSON.
    /// Optional Pre Image "PreImage" with bcc_leavetypeid avoids an extra retrieve.
    /// </summary>
    public class OnUpdateLeaveRequestAttendance : IPlugin
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

            if (target.LogicalName != LeaveRequest.EntityName || !target.Contains(LeaveRequest.Attendance))
                return;

            try
            {
                tracingService.Trace("OnUpdateLeaveRequestAttendance: Plugin execution started.");

                // ── Step 1: Leave Type from Target, then Pre Image, then the record ──
                EntityReference leaveTypeRef = target.GetAttributeValue<EntityReference>(LeaveRequest.LeaveType);

                if (leaveTypeRef == null && !target.Contains(LeaveRequest.LeaveType)
                    && context.PreEntityImages.Contains("PreImage"))
                {
                    leaveTypeRef = context.PreEntityImages["PreImage"].GetAttributeValue<EntityReference>(LeaveRequest.LeaveType);
                }

                if (leaveTypeRef == null && !target.Contains(LeaveRequest.LeaveType))
                {
                    var leaveRequest = service.Retrieve(LeaveRequest.EntityName, target.Id, new ColumnSet(LeaveRequest.LeaveType));
                    leaveTypeRef = leaveRequest.GetAttributeValue<EntityReference>(LeaveRequest.LeaveType);
                }

                string leaveTypeName = Common.Common.GetLeaveTypeName(service, leaveTypeRef);
                tracingService.Trace("Leave Type: {0}", leaveTypeName);

                if (!Common.Common.IsCompensatoryLeave(leaveTypeName))
                {
                    tracingService.Trace("Not a Compensatory Leave. Exiting.");
                    return;
                }

                // ── Step 2: Number of Days = count of attendance records ──
                int attendanceCount = Common.Common.CountAttendanceRecords(target.GetAttributeValue<string>(LeaveRequest.Attendance));
                target[LeaveRequest.LeaveDays] = (decimal)attendanceCount;

                tracingService.Trace("Compensatory Leave: LeaveDays set from attendance count: {0}", attendanceCount);
                tracingService.Trace("OnUpdateLeaveRequestAttendance: Plugin execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnUpdateLeaveRequestAttendance: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in OnUpdateLeaveRequestAttendance plugin: " + ex.Message, ex);
            }
        }
    }
}
