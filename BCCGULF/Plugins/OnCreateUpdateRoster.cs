using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class OnCreateUpdateRoster : IPlugin
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

            if (target.LogicalName != Roster.EntityName)
                return;

            try
            {
                tracingService.Trace("OnCreateUpdateRoster: Plugin execution started. Message: {0}", context.MessageName);

                bool isCreate = context.MessageName.Equals("Create", StringComparison.OrdinalIgnoreCase);
                bool isUpdate = context.MessageName.Equals("Update", StringComparison.OrdinalIgnoreCase);

                if (!isCreate && !isUpdate)
                    return;

                // ── Merge target with full record for reliable field access ──
                Entity fullRecord;
                if (isCreate)
                {
                    fullRecord = target;
                }
                else
                {
                    // On Update, target only has changed fields; use Post Image for full data
                    if (!context.PostEntityImages.Contains("PostImage"))
                    {
                        tracingService.Trace("PostImage is not registered. Exiting.");
                        return;
                    }
                    fullRecord = context.PostEntityImages["PostImage"];
                }

                // ── Step 1: Get Date from Roster ──
                if (!fullRecord.Contains(Roster.Date) || fullRecord[Roster.Date] == null)
                {
                    tracingService.Trace("Date is not set on Roster. Exiting.");
                    return;
                }

                DateTime rosterDate = fullRecord.GetAttributeValue<DateTime>(Roster.Date);
                tracingService.Trace("Roster Date: {0}", rosterDate.ToString("yyyy-MM-dd"));

                // ── Step 2: Validate Roster date is in the current month ──
                DateTime now = DateTime.UtcNow;
                if (rosterDate.Year != now.Year || rosterDate.Month != now.Month)
                {
                    throw new InvalidPluginExecutionException(
                        "Roster can only be created or updated for the current month (" +
                        now.ToString("MMMM yyyy") + ").");
                }

                // ── Step 2b: Validate Roster date is not a Saturday ──
                if (rosterDate.DayOfWeek != DayOfWeek.Saturday)
                {
                    throw new InvalidPluginExecutionException(
                        "Roster can be created or updated for a Saturday (" +
                        rosterDate.ToString("dd/MM/yyyy") + " is " + rosterDate.DayOfWeek+ ").");
                }

                // ── Step 3: Get Bookable Resource ──
                if (!fullRecord.Contains(Roster.BookableResource) || fullRecord[Roster.BookableResource] == null)
                {
                    tracingService.Trace("BookableResource is not set on Roster. Exiting.");
                    return;
                }

                var bookableResourceRef = fullRecord.GetAttributeValue<EntityReference>(Roster.BookableResource);
                tracingService.Trace("BookableResource: {0}", bookableResourceRef.Id);

                // ── Step 4: Get Status (Present / Absent) ──
                if (!fullRecord.Contains(Roster.Status) || fullRecord[Roster.Status] == null)
                {
                    tracingService.Trace("Status is not set on Roster. Exiting.");
                    return;
                }

                int statusValue = fullRecord.GetAttributeValue<OptionSetValue>(Roster.Status).Value;
                tracingService.Trace("Roster Status: {0}", statusValue);

                bool isPresent = statusValue == (int)Roster.RosterStatus_OptionSet.Present;
                bool isAbsent = statusValue == (int)Roster.RosterStatus_OptionSet.Absent;

                // ── Step 5: On Update, check if status actually changed ──
                if (isUpdate)
                {
                    // Only act if bcc_status was actually changed in this update
                    if (!target.Contains(Roster.Status))
                    {
                        tracingService.Trace("Status was not changed in this update. Exiting.");
                        return;
                    }

                    // Get Pre Image to compare old status
                    if (context.PreEntityImages.Contains("PreImage"))
                    {
                        var preImage = context.PreEntityImages["PreImage"];
                        if (preImage.Contains(Roster.Status) && preImage[Roster.Status] != null)
                        {
                            int oldStatus = preImage.GetAttributeValue<OptionSetValue>(Roster.Status).Value;
                            tracingService.Trace("Old Status: {0}, New Status: {1}", oldStatus, statusValue);

                            if (oldStatus == statusValue)
                            {
                                tracingService.Trace("Status did not change. Exiting.");
                                return;
                            }
                        }
                    }
                }

                // ── Step 6: Execute logic based on status ──
                if (isPresent)
                {
                    tracingService.Trace("Status is Present. Creating Time Entry...");
                    CreateTimeEntry(service, tracingService, target, bookableResourceRef, rosterDate);
                }
                else if (isAbsent)
                {
                    tracingService.Trace("Status is Absent. Deleting Time Entry...");
                    DeleteTimeEntry(service, tracingService, fullRecord, bookableResourceRef, rosterDate);
                }
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnCreateUpdateRoster: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred in OnCreateUpdateRoster: " + ex.Message, ex);
            }
        }

        private void CreateTimeEntry(IOrganizationService service, ITracingService tracingService,
            Entity rosterTarget, EntityReference bookableResourceRef, DateTime rosterDate)
        {
            // ── Build Start and End for 8-hour day ──
            DateTime startDateTime = new DateTime(rosterDate.Year, rosterDate.Month, rosterDate.Day, 0, 0, 0, DateTimeKind.Utc);
            DateTime endDateTime = startDateTime.AddHours(8);
            int durationMinutes = 480; // 8 hours

            tracingService.Trace("Start: {0}, End: {1}, Duration: {2} min", startDateTime, endDateTime, durationMinutes);

            // ── Lookup "Present" task from msdyn_servicetasktype ──
            EntityReference taskRef = null;
            string taskFetch = string.Format(@"
                <fetch top='1' no-lock='true'>
                    <entity name='msdyn_servicetasktype'>
                        <attribute name='msdyn_servicetasktypeid' />
                        <filter>
                            <condition attribute='msdyn_name' operator='eq' value='Present' />
                        </filter>
                    </entity>
                </fetch>");

            var taskResults = service.RetrieveMultiple(new FetchExpression(taskFetch));
            if (taskResults.Entities.Count > 0)
            {
                taskRef = new EntityReference("msdyn_servicetasktype", taskResults.Entities[0].Id);
                tracingService.Trace("Task 'Present' found: {0}", taskRef.Id);
            }
            else
            {
                tracingService.Trace("Task 'Present' not found in msdyn_servicetasktype.");
            }

            // ── Build Time Entry Name: "ResourceName - 24-Mar-2026 - 8 Hours" ──
            string resourceName = bookableResourceRef.Name;
            if (string.IsNullOrEmpty(resourceName))
            {
                var resource = service.Retrieve(BookableResource.EntityName, bookableResourceRef.Id,
                    new ColumnSet(BookableResource.PrimaryName));
                resourceName = resource.GetAttributeValue<string>(BookableResource.PrimaryName) ?? string.Empty;
            }
            int durationHours = durationMinutes / 60;
            string timeEntryName = string.Format("{0} - {1} - {2} Hours",
                resourceName, rosterDate.ToString("dd-MMM-yyyy"), durationHours);
            tracingService.Trace("Time Entry Name: {0}", timeEntryName);

            // ── Create Time Entry ──
            var timeEntry = new Entity(TimeEntry.EntityName);
            timeEntry[TimeEntry.PrimaryName] = timeEntryName;
            timeEntry[TimeEntry.BookableResource] = new EntityReference(BookableResource.EntityName, bookableResourceRef.Id);
            timeEntry[TimeEntry.Date] = rosterDate;
            timeEntry[TimeEntry.Start] = startDateTime;
            timeEntry[TimeEntry.End] = endDateTime;
            timeEntry[TimeEntry.Duration] = durationMinutes;
            timeEntry[TimeEntry.EntryType] = new OptionSetValue((int)TimeEntry.EntryType_OptionSet.Present);
            timeEntry[TimeEntry.Type] = new OptionSetValue((int)TimeEntry.Type_OptionSet.Work);
            timeEntry[TimeEntry.Roster] = new EntityReference(Roster.EntityName, rosterTarget.Id);

            if (taskRef != null)
            {
                timeEntry[TimeEntry.Task1] = taskRef;
            }

            Guid timeEntryId = service.Create(timeEntry);
            tracingService.Trace("Time Entry created: {0}", timeEntryId);

            // ── Update Roster with the Time Entry reference ──
            var rosterUpdate = new Entity(Roster.EntityName, rosterTarget.Id);
            rosterUpdate[Roster.TimeEntry] = new EntityReference(TimeEntry.EntityName, timeEntryId);
            service.Update(rosterUpdate);
            tracingService.Trace("Roster updated with Time Entry reference.");
        }

        private void DeleteTimeEntry(IOrganizationService service, ITracingService tracingService,
            Entity fullRecord, EntityReference bookableResourceRef, DateTime rosterDate)
        {
            // ── First try to use the Roster's TimeEntry lookup ──
            if (fullRecord.Contains(Roster.TimeEntry) && fullRecord[Roster.TimeEntry] != null)
            {
                var existingTimeEntryRef = fullRecord.GetAttributeValue<EntityReference>(Roster.TimeEntry);
                tracingService.Trace("Deleting Time Entry from Roster lookup: {0}", existingTimeEntryRef.Id);

                try
                {
                    service.Delete(TimeEntry.EntityName, existingTimeEntryRef.Id);
                    tracingService.Trace("Time Entry deleted successfully.");
                }
                catch (Exception ex)
                {
                    tracingService.Trace("Failed to delete Time Entry by lookup: {0}", ex.Message);
                }

                // Clear the Roster's TimeEntry lookup
                var rosterUpdate = new Entity(Roster.EntityName, fullRecord.Id);
                rosterUpdate[Roster.TimeEntry] = null;
                service.Update(rosterUpdate);
                tracingService.Trace("Roster TimeEntry lookup cleared.");
                return;
            }

            // ── Fallback: Query Time Entry by BookableResource + Date ──
            tracingService.Trace("No TimeEntry lookup on Roster. Querying by BookableResource and Date...");

            string fetchXml = string.Format(@"
                <fetch top='1' no-lock='true'>
                    <entity name='{0}'>
                        <attribute name='{1}' />
                        <filter type='and'>
                            <condition attribute='{2}' operator='eq' value='{3}' />
                            <condition attribute='{4}' operator='on' value='{5}' />
                        </filter>
                    </entity>
                </fetch>",
                TimeEntry.EntityName,
                TimeEntry.PrimaryKey,
                TimeEntry.BookableResource,
                bookableResourceRef.Id.ToString(),
                TimeEntry.Date,
                rosterDate.ToString("yyyy-MM-dd"));

            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Count > 0)
            {
                Guid timeEntryId = results.Entities[0].Id;
                tracingService.Trace("Found Time Entry to delete: {0}", timeEntryId);

                service.Delete(TimeEntry.EntityName, timeEntryId);
                tracingService.Trace("Time Entry deleted successfully.");

                // Clear the Roster's TimeEntry lookup
                var rosterUpdate = new Entity(Roster.EntityName, fullRecord.Id);
                rosterUpdate[Roster.TimeEntry] = null;
                service.Update(rosterUpdate);
                tracingService.Trace("Roster TimeEntry lookup cleared.");
            }
            else
            {
                tracingService.Trace("No Time Entry found for BookableResource {0} on Date {1}.",
                    bookableResourceRef.Id, rosterDate.ToString("yyyy-MM-dd"));
            }
        }
    }
}
