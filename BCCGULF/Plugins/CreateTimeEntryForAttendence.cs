using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class CreateTimeEntryForAttendence : IPlugin
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

            if (target.LogicalName != Attendence.EntityName)
                return;

            try
            {
                tracingService.Trace("CreateTimeEntryForAttendence: Plugin execution started.");

                // ── Step 1: Get BookableResource from Attendence ──
                if (!target.Contains(Attendence.Employee_ID) || target[Attendence.Employee_ID] == null)
                {
                    tracingService.Trace("Employee_ID (BookableResource) is not set on Attendence. Exiting.");
                    return;
                }

                var bookableResourceRef = target.GetAttributeValue<EntityReference>(Attendence.Employee_ID);

                var BookableRecord = service.Retrieve(bookableResourceRef.LogicalName, bookableResourceRef.Id, new ColumnSet(BookableResource.Type));
                if (!BookableRecord.Contains(BookableResource.Type)) return;
                if (BookableRecord.GetAttributeValue<OptionSetValue>(BookableResource.Type).Value != (int)BookableResource.Type_OptionSet.Staff) return;
                tracingService.Trace("BookableResource: {0}", bookableResourceRef.Id);
                // ── Step 2: Get Date from Attendence ──
                if (!target.Contains(Attendence.Date) || target[Attendence.Date] == null)
                {
                    tracingService.Trace("Date is not set on Attendence. Exiting.");
                    return;
                }

                DateTime attendenceDate = target.GetAttributeValue<DateTime>(Attendence.Date);
                tracingService.Trace("Attendence Date: {0}", attendenceDate.ToString("dd/MM/yyyy"));

                // ── Step 3: Parse InTime and OutTime ──
                string inTimeStr = target.Contains(Attendence.InTime) && target[Attendence.InTime] != null
                    ? target.GetAttributeValue<string>(Attendence.InTime)
                    : null;

                string outTimeStr = target.Contains(Attendence.OutTime) && target[Attendence.OutTime] != null
                    ? target.GetAttributeValue<string>(Attendence.OutTime)
                    : null;

                tracingService.Trace("InTime: {0}, OutTime: {1}", inTimeStr ?? "N/A", outTimeStr ?? "N/A");

                DateTime startDateTime = attendenceDate.Date;
                DateTime endDateTime = attendenceDate.Date;

                if (!string.IsNullOrEmpty(inTimeStr))
                {
                    TimeSpan inTime;
                    if (TimeSpan.TryParse(inTimeStr, out inTime))
                    {
                        startDateTime = attendenceDate.Date.Add(inTime);
                    }
                    else
                    {
                        tracingService.Trace("Could not parse InTime: {0}. Using date only.", inTimeStr);
                    }
                }

                if (!string.IsNullOrEmpty(outTimeStr))
                {
                    TimeSpan outTime;
                    if (TimeSpan.TryParse(outTimeStr, out outTime))
                    {
                        endDateTime = attendenceDate.Date.Add(outTime);
                    }
                    else
                    {
                        tracingService.Trace("Could not parse OutTime: {0}. Using date only.", outTimeStr);
                    }
                }

                tracingService.Trace("Start DateTime: {0}, End DateTime: {1}", startDateTime, endDateTime);

                // ── Step 4: Parse TotalWorkingHours — cap at 8 hours if >= 8, else use actual ──
                int durationMinutes = 480;
                string totalWorkingHoursStr = target.Contains(Attendence.TotalWorkingHours) && target[Attendence.TotalWorkingHours] != null
                    ? target.GetAttributeValue<string>(Attendence.TotalWorkingHours)
                    : null;

                if (!string.IsNullOrEmpty(totalWorkingHoursStr))
                {
                    decimal workedHours = 0m;
                    decimal parsedVal;
                    if (decimal.TryParse(totalWorkingHoursStr, out parsedVal))
                    {
                        workedHours = parsedVal;
                    }
                    else
                    {
                        TimeSpan workedSpan;
                        if (TimeSpan.TryParse(totalWorkingHoursStr, out workedSpan))
                        {
                            workedHours = (decimal)workedSpan.TotalHours;
                        }
                    }

                    tracingService.Trace("Total Working Hours parsed: {0}", workedHours);

                    if (workedHours >= 8m)
                    {
                        durationMinutes = 480;
                    }
                    else
                    {
                        durationMinutes = (int)(workedHours * 60);
                    }
                }

                tracingService.Trace("Duration (minutes): {0}", durationMinutes);

                // ── Step 5: Parse Overtime from Attendence ──
                string overtimeStr = target.Contains(Attendence.Overtime) && target[Attendence.Overtime] != null
                    ? target.GetAttributeValue<string>(Attendence.Overtime)
                    : null;

                decimal overtimeHours = 0m;
                if (!string.IsNullOrEmpty(overtimeStr))
                {
                    decimal parsedHours;
                    if (decimal.TryParse(overtimeStr, out parsedHours))
                    {
                        overtimeHours = parsedHours;
                    }
                    else
                    {
                        TimeSpan overtimeSpan;
                        if (TimeSpan.TryParse(overtimeStr, out overtimeSpan))
                        {
                            overtimeHours = (decimal)overtimeSpan.TotalHours;
                        }
                    }
                }

                tracingService.Trace("Overtime hours: {0}", overtimeHours);

                // ── Step 6: Get Project (MachineNameIN) if available ──
                EntityReference projectRef = null;
                if (target.Contains(Attendence.MachineNameIN) && target[Attendence.MachineNameIN] != null)
                {
                    projectRef = target.GetAttributeValue<EntityReference>(Attendence.MachineNameIN);
                    tracingService.Trace("Project from MachineNameIN: {0}", projectRef.Id);
                }

                // ── Step 7: Get "Present" Task from msdyn_servicetasktype ──
                EntityReference taskRef = null;
                var taskQuery = new QueryExpression("msdyn_servicetasktype");
                taskQuery.ColumnSet = new ColumnSet("msdyn_servicetasktypeid");
                taskQuery.Criteria.AddCondition("msdyn_name", ConditionOperator.Equal, "Present");
                taskQuery.TopCount = 1;

                var taskResults = service.RetrieveMultiple(taskQuery);
                if (taskResults.Entities.Count > 0)
                {
                    taskRef = new EntityReference("msdyn_servicetasktype", taskResults.Entities[0].Id);
                    tracingService.Trace("Task 'Present' found: {0}", taskRef.Id);
                }
                else
                {
                    tracingService.Trace("Task 'Present' not found in msdyn_servicetasktype.");
                }

                // ── Step 8: Create Time Entry record ──
                var timeEntry = new Entity(TimeEntry.EntityName);
                timeEntry[TimeEntry.BookableResource] = new EntityReference(BookableResource.EntityName, bookableResourceRef.Id);
                timeEntry[TimeEntry.Date] = attendenceDate;
                timeEntry[TimeEntry.Start] = startDateTime;
                timeEntry[TimeEntry.End] = endDateTime;
                timeEntry[TimeEntry.Duration] = durationMinutes;
                timeEntry[TimeEntry.EntryType] = new OptionSetValue((int)TimeEntry.EntryType_OptionSet.Present);
                timeEntry[TimeEntry.Type] = new OptionSetValue((int)TimeEntry.Type_OptionSet.Work);
                timeEntry[TimeEntry.Biometric] = new EntityReference(Attendence.EntityName, target.Id);
                timeEntry[TimeEntry.Total_Hours] = !string.IsNullOrEmpty(totalWorkingHoursStr) ? Convert.ToDecimal(totalWorkingHoursStr) : decimal.Zero;
                if (overtimeHours > 0m)
                {
                    timeEntry[TimeEntry.Overtime1] = overtimeHours;
                    tracingService.Trace("Overtime1 set to: {0}", overtimeHours);
                }
                if (projectRef != null)
                {
                    timeEntry[TimeEntry.Project] = projectRef;
                }

                if (taskRef != null)
                {
                    timeEntry[TimeEntry.Task1] = taskRef;
                }

                Guid timeEntryId = service.Create(timeEntry);
                tracingService.Trace("Time Entry created successfully. ID: {0}", timeEntryId);

                tracingService.Trace("CreateTimeEntryForAttendence: Plugin execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("CreateTimeEntryForAttendence: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in CreateTimeEntryForAttendence plugin: " + ex.Message, ex);
            }
        }
    }
}
