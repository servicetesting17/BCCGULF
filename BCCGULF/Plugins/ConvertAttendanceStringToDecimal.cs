using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using System;

namespace BCCGULF.Plugins
{
    /// <summary>
    /// Plugin: ConvertAttendanceStringToDecimal
    /// Description: On Create/Update (Post-Operation):
    ///              1. Converts TotalWorkingHours (string) to TotalWorkingHoursDec (decimal).
    ///              2. Converts Overtime (string) to OvertimeHours (decimal).
    ///              3. If TotalWorkingHoursDec >= 8, sets statuscode to Approved.
    /// Execution Stage: Post-Operation (40)
    /// Execution Mode: Synchronous
    /// Event: Create, Update
    /// Entity: Attendance (new_biometric)
    /// </summary>
    public class ConvertAttendanceStringToDecimal : IPlugin
    {
        private const int     StatusApproved      = 2;
        private const decimal AutoApproveThreshold = 8m;

        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context        = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            if (context.InputParameters == null
                || !context.InputParameters.Contains("Target")
                || !(context.InputParameters["Target"] is Entity))
                return;

            var target = (Entity)context.InputParameters["Target"];

            if (target.LogicalName != Attendence.EntityName)
                return;

            try
            {
                tracingService.Trace("ConvertAttendanceStringToDecimal: started. Event: {0}, RecordId: {1}", context.MessageName, target.Id);

                var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                var service = factory.CreateOrganizationService(context.UserId);

                var update = new Entity(Attendence.EntityName, target.Id);

                // ── Convert TotalWorkingHours (string) → TotalWorkingHoursDec (decimal) ──
                decimal totalHours = 0m;
                if (target.Contains(Attendence.TotalWorkingHours) && target[Attendence.TotalWorkingHours] != null)
                {
                    string raw = target.GetAttributeValue<string>(Attendence.TotalWorkingHours);
                    tracingService.Trace("TotalWorkingHours (string): {0}", raw);

                    if (!string.IsNullOrWhiteSpace(raw) && decimal.TryParse(raw, out decimal parsed))
                    {
                        totalHours = parsed;
                        update[Attendence.TotalWorkingHoursDec] = totalHours;
                        tracingService.Trace("TotalWorkingHoursDec: {0}", totalHours);
                    }
                    else
                        tracingService.Trace("WARNING: Could not convert TotalWorkingHours '{0}' to decimal.", raw);
                }

                // ── Convert Overtime (string) → OvertimeHours (decimal) ──
                if (target.Contains(Attendence.Overtime) && target[Attendence.Overtime] != null)
                {
                    string raw = target.GetAttributeValue<string>(Attendence.Overtime);
                    tracingService.Trace("Overtime (string): {0}", raw);

                    if (!string.IsNullOrWhiteSpace(raw) && decimal.TryParse(raw, out decimal overtimeDec))
                    {
                        update[Attendence.OvertimeHours] = overtimeDec;
                        tracingService.Trace("OvertimeHours: {0}", overtimeDec);
                    }
                    else
                        tracingService.Trace("WARNING: Could not convert Overtime '{0}' to decimal.", raw);
                }

                // ── Auto-approve if TotalWorkingHoursDec >= 8 ──
                if (totalHours >= AutoApproveThreshold)
                {
                    update[Attendence.StatusReason] = new OptionSetValue(StatusApproved);
                    update[Attendence.Status] = new OptionSetValue(Attendence.Status_OptionSet.Inactive.GetHashCode());

                    tracingService.Trace("TotalWorkingHours ({0}) >= {1}. Status set to Approved.", totalHours, AutoApproveThreshold);
                }
                else
                    tracingService.Trace("TotalWorkingHours ({0}) < {1}. No auto-approval.", totalHours, AutoApproveThreshold);

                service.Update(update);

                tracingService.Trace("ConvertAttendanceStringToDecimal: completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("ConvertAttendanceStringToDecimal: Exception — {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred in ConvertAttendanceStringToDecimal plugin.", ex);
            }
        }
    }
}
