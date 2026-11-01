using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;
using System.Text;

namespace BCCGULF.Plugins
{
    /// <summary>
    /// Plugin Step Registration:
    ///   Entity  : new_biometric
    ///   Message : Create, Update
    ///   Stage   : Post-Operation (40)
    ///   Mode    : Asynchronous
    ///
    /// Sends an email alert to the BookableResource's linked Contact when:
    ///   - Check-in  (bcc_intime)  is more than 30 minutes AFTER  shift start (bcc_shifttimefrom)
    ///   - Check-out (bcc_outtime) is more than 30 minutes BEFORE shift end   (bcc_shifttimeto)
    ///
    /// Only evaluates the field(s) present in the target entity (i.e. what changed this operation).
    /// </summary>
    public class AttendanceShiftAlertPlugin : IPlugin
    {
        private const int    ThresholdMinutes   = 15;

        // Shift lookup on bookableresource — not yet in BookableResource.cs constants; update if field name differs
        private const string ResourceShiftField = "new_shift";

        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)             serviceProvider.GetService(typeof(ITracingService));
            var context        = (IPluginExecutionContext)      serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory) serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service        = serviceFactory.CreateOrganizationService(context.UserId);

            tracingService.Trace("AttendanceShiftAlertPlugin started. Message={0}", context.MessageName);

            if (context.InputParameters == null
                || !context.InputParameters.Contains("Target")
                || !(context.InputParameters["Target"] is Entity target))
                return;

            if (target.LogicalName != Attendence.EntityName)
                return;

            try
            {
                // Only run when InTime or OutTime is included in this operation
                bool inTimeChanged  = target.Contains(Attendence.InTime);
                bool outTimeChanged = target.Contains(Attendence.OutTime);

                if (!inTimeChanged && !outTimeChanged)
                {
                    tracingService.Trace("Neither InTime nor OutTime in target — skipping.");
                    return;
                }

                tracingService.Trace("InTime changed={0} | OutTime changed={1}", inTimeChanged, outTimeChanged);

                // ── Step 1: Retrieve full attendance record (post-op) ─────────────
                var attendance = context.PostEntityImages.Contains("PostImage") && context.PostEntityImages["PostImage"] is Entity postImage
                    ? postImage
                    : service.Retrieve(Attendence.EntityName, target.Id, new ColumnSet(true));

                if (!attendance.Contains(Attendence.Employee_ID) || attendance[Attendence.Employee_ID] == null)
                {
                    tracingService.Trace("Attendance has no Employee — skipping.");
                    return;
                }

                var employeeRef = attendance.GetAttributeValue<EntityReference>(Attendence.Employee_ID);
                tracingService.Trace("Employee (BookableResource) ID: {0}", employeeRef.Id);

                // ── Step 2: Retrieve BookableResource ─────────────────────────────
                var resource = service.Retrieve(
                    BookableResource.EntityName,
                    employeeRef.Id,
                    new ColumnSet(ResourceShiftField, BookableResource.Contact, BookableResource.PrimaryName,
                                  BookableResource.Manager, BookableResource.EmployeeID, BookableResource.DesignationID)
                );

                string employeeName   = resource.GetAttributeValue<string>(BookableResource.PrimaryName) ?? "Employee";
                var    managerRef     = resource.GetAttributeValue<EntityReference>(BookableResource.Manager);
                string employeeId     = resource.GetAttributeValue<string>(BookableResource.EmployeeID) ?? "-";
                string designationName = resource.GetAttributeValue<EntityReference>(BookableResource.DesignationID)?.Name ?? "-";

                if (!resource.Contains(ResourceShiftField) || resource[ResourceShiftField] == null)
                {
                    tracingService.Trace("BookableResource '{0}' has no shift assigned — skipping.", employeeName);
                    return;
                }

                var shiftRef = resource.GetAttributeValue<EntityReference>(ResourceShiftField);
                tracingService.Trace("Shift ID: {0}", shiftRef.Id);

                // ── Step 3: Retrieve Shift ────────────────────────────────────────
                var shift = service.Retrieve(
                    Shift.EntityName,
                    shiftRef.Id,
                    new ColumnSet(Shift.Shifttimefrom, Shift.Shifttimeto, Shift.PrimaryName)
                );

                tracingService.Trace("Shift name: {0}", shift.GetAttributeValue<string>(Shift.PrimaryName));

                // ── Step 4: Retrieve Contact linked to BookableResource ───────────
                var contactRef = resource.GetAttributeValue<EntityReference>(BookableResource.Contact);
                if (contactRef == null)
                {
                    tracingService.Trace("BookableResource '{0}' has no Contact linked — skipping.", employeeName);
                    return;
                }

                var contact = service.Retrieve(
                    "contact",
                    contactRef.Id,
                    new ColumnSet("emailaddress1", "fullname")
                );

                string contactEmail = contact.GetAttributeValue<string>("emailaddress1");
                if (string.IsNullOrWhiteSpace(contactEmail))
                {
                    tracingService.Trace("Contact has no email address — skipping.");
                    return;
                }

                tracingService.Trace("Contact email: {0}", contactEmail);

                // Attendance date for email subject
                string attendanceDateStr = "-";
                if (attendance.Contains(Attendence.Date) && attendance[Attendence.Date] is DateTime attendanceDate)
                    attendanceDateStr = attendanceDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

                string inTimeStr  = attendance.GetAttributeValue<string>(Attendence.InTime);
                string outTimeStr = attendance.GetAttributeValue<string>(Attendence.OutTime);

                // Shift DateTime fields come back from D365 in UTC; InTime/OutTime strings are local (GST = UTC+4).
                // Convert shift times to local before comparing.
                var gulfTz = TimeZoneInfo.FindSystemTimeZoneById("Arabian Standard Time");

                bool isLateFlag = false;
                bool isEarlyCheckoutFlag = false;

                // ── Step 5: Late Check-In Check ───────────────────────────────────
                if (!string.IsNullOrWhiteSpace(inTimeStr)
                    && shift.Contains(Shift.Shifttimefrom) && shift[Shift.Shifttimefrom] != null)
                {
                    TimeSpan shiftStart = TimeZoneInfo.ConvertTimeFromUtc((DateTime)shift[Shift.Shifttimefrom], gulfTz).TimeOfDay;

                    if (TimeSpan.TryParse(inTimeStr, out TimeSpan actualCheckIn))
                    {
                        tracingService.Trace("Check-in: {0} | Shift start: {1}", actualCheckIn, shiftStart);
                        isLateFlag = actualCheckIn > shiftStart.Add(TimeSpan.FromMinutes(ThresholdMinutes));

                        if (inTimeChanged && isLateFlag)
                        {
                            tracingService.Trace("Late check-in detected — sending alert.");
                            SendAlertEmail(
                                service, tracingService, context.UserId,
                                contactRef, contactEmail, employeeName, attendanceDateStr,
                                isLateCheckIn:       true,
                                actualTime:          actualCheckIn,
                                shiftTime:           shiftStart,
                                bookableResourceRef: employeeRef,
                                attendanceRef:       new EntityReference(Attendence.EntityName, target.Id),
                                managerRef:          managerRef,
                                employeeId:          employeeId,
                                designationName:     designationName
                            );
                        }
                        else if (inTimeChanged)
                            tracingService.Trace("Check-in within threshold — no alert needed.");
                    }
                    else
                        tracingService.Trace("Could not parse InTime '{0}' — skipping check-in check.", inTimeStr);
                }

                // ── Step 6: Early Check-Out Check ─────────────────────────────────
                if (!string.IsNullOrWhiteSpace(outTimeStr)
                    && shift.Contains(Shift.Shifttimeto) && shift[Shift.Shifttimeto] != null)
                {
                    TimeSpan shiftEnd = TimeZoneInfo.ConvertTimeFromUtc((DateTime)shift[Shift.Shifttimeto], gulfTz).TimeOfDay;

                    if (TimeSpan.TryParse(outTimeStr, out TimeSpan actualCheckOut))
                    {
                        tracingService.Trace("Check-out: {0} | Shift end: {1}", actualCheckOut, shiftEnd);
                        isEarlyCheckoutFlag = actualCheckOut < shiftEnd.Subtract(TimeSpan.FromMinutes(ThresholdMinutes));

                        if (outTimeChanged && isEarlyCheckoutFlag)
                        {
                            tracingService.Trace("Early check-out detected — sending alert.");
                            SendAlertEmail(
                                service, tracingService, context.UserId,
                                contactRef, contactEmail, employeeName, attendanceDateStr,
                                isLateCheckIn:       false,
                                actualTime:          actualCheckOut,
                                shiftTime:           shiftEnd,
                                bookableResourceRef: employeeRef,
                                attendanceRef:       new EntityReference(Attendence.EntityName, target.Id),
                                managerRef:          managerRef,
                                employeeId:          employeeId,
                                designationName:     designationName
                            );
                        }
                        else if (outTimeChanged)
                            tracingService.Trace("Check-out within threshold — no alert needed.");
                    }
                    else
                        tracingService.Trace("Could not parse OutTime '{0}' — skipping check-out check.", outTimeStr);
                }

                UpdateAttendanceComplianceFlags(service, tracingService, attendance, isLateFlag, isEarlyCheckoutFlag);

                tracingService.Trace("AttendanceShiftAlertPlugin completed.");
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracingService.Trace("AttendanceShiftAlertPlugin error: {0}", ex.ToString());
                // Non-fatal — attendance record save must not be blocked by notification failure
            }
        }

        private static void UpdateAttendanceComplianceFlags(
            IOrganizationService service, ITracingService tracingService,
            Entity attendance, bool isLate, bool isEarlyCheckout)
        {
            if (attendance == null || attendance.Id == Guid.Empty)
                return;

            bool needsUpdate = false;
            var updateAttendance = new Entity(Attendence.EntityName, attendance.Id);

            var expectedLateValue = new OptionSetValue(isLate
                ? (int)Attendence.CheckInStatus_OptionSet.Yes
                : (int)Attendence.CheckInStatus_OptionSet.NO);
            if (!HasOptionSetValue(attendance, Attendence.UserIsLate, expectedLateValue.Value))
            {
                updateAttendance[Attendence.UserIsLate] = expectedLateValue;
                needsUpdate = true;
            }

            var expectedEarlyCheckoutValue = new OptionSetValue(isEarlyCheckout
                ? (int)Attendence.CheckInStatus_OptionSet.Yes
                : (int)Attendence.CheckInStatus_OptionSet.NO);
            if (!HasOptionSetValue(attendance, Attendence.UserEarlyCheckout, expectedEarlyCheckoutValue.Value))
            {
                updateAttendance[Attendence.UserEarlyCheckout] = expectedEarlyCheckoutValue;
                needsUpdate = true;
            }

            if (needsUpdate)
            {
                service.Update(updateAttendance);
                tracingService.Trace("Attendance compliance flags updated: Late={0}, CheckedOutEarly={1}", isLate, isEarlyCheckout);
            }
        }

        private static bool HasOptionSetValue(Entity entity, string attributeName, int expectedValue)
        {
            if (entity == null || string.IsNullOrWhiteSpace(attributeName))
                return false;

            if (!entity.Contains(attributeName) || entity[attributeName] == null)
                return false;

            return entity.GetAttributeValue<OptionSetValue>(attributeName)?.Value == expectedValue;
        }

        // ─── Email ────────────────────────────────────────────────────────────────

        private static void SendAlertEmail(
            IOrganizationService service, ITracingService tracingService,
            Guid senderId, EntityReference contactRef, string contactEmail,
            string employeeName, string attendanceDate,
            bool isLateCheckIn, TimeSpan actualTime, TimeSpan shiftTime,
            EntityReference bookableResourceRef, EntityReference attendanceRef, EntityReference managerRef,
            string employeeId, string designationName)
        {
            string subject = isLateCheckIn
                ? string.Format("🚫 [No Reply] Late Check-In Alert — {0} — {1}", employeeName, attendanceDate)
                : string.Format("🚫 [No Reply] Early Check-Out Alert — {0} — {1}", employeeName, attendanceDate);

            var email = new Entity("email");
            email["subject"]           = subject;
            email["description"]       = BuildEmailBody(employeeName, attendanceDate, isLateCheckIn, actualTime, shiftTime, employeeId, designationName);
            email["regardingobjectid"] = attendanceRef;

            var fromParty = new Entity("activityparty");
            fromParty["partyid"] = new EntityReference("systemuser", senderId);
            email["from"] = new EntityCollection(new[] { fromParty });

            var toParty = new Entity("activityparty");
            toParty["partyid"] = contactRef;
            email["to"] = new EntityCollection(new[] { toParty });

            if (managerRef != null)
            {
                var ccParty = new Entity("activityparty");
                ccParty["partyid"] = managerRef;
                email["cc"] = new EntityCollection(new[] { ccParty });
                tracingService.Trace("Manager CC set: {0}", managerRef.Id);
            }

            Guid emailId = service.Create(email);
            tracingService.Trace("Email activity created: {0} → {1}", emailId, contactEmail);

            var sendRequest = new OrganizationRequest("SendEmail");
            sendRequest["EmailId"]       = emailId;
            sendRequest["TrackingToken"] = string.Empty;
            sendRequest["IssueSend"]     = true;
            service.Execute(sendRequest);

            tracingService.Trace("Email sent successfully.");
        }

        // ─── HTML Builder ─────────────────────────────────────────────────────────

        private static string BuildEmailBody(
            string employeeName, string date, bool isLateCheckIn,
            TimeSpan actualTime, TimeSpan shiftTime,
            string employeeId, string designationName)
        {
            string alertTitle  = isLateCheckIn ? "Late Check-In Alert"  : "Early Check-Out Alert";
            string headerColor = isLateCheckIn ? "#a4262c"              : "#835b00";
            string actualLabel = isLateCheckIn ? "Actual Check-In"      : "Actual Check-Out";
            string shiftLabel  = isLateCheckIn ? "Shift Start Time"     : "Shift End Time";
            int    deviation   = isLateCheckIn
                ? (int)(actualTime - shiftTime).TotalMinutes
                : (int)(shiftTime  - actualTime).TotalMinutes;
            string deviationStr = isLateCheckIn
                ? string.Format("{0} minutes after shift start",  deviation)
                : string.Format("{0} minutes before shift end",   deviation);

            var sb = new StringBuilder();
            sb.Append("<html><body style=\"font-family:Segoe UI,Arial,sans-serif;font-size:13px;color:#222;\">");

            // Alert header banner
            sb.AppendFormat(
                "<div style=\"background-color:{0};padding:12px 16px;border-radius:4px;margin-bottom:16px;\">"
              + "<span style=\"color:#ffffff;font-size:15px;font-weight:700;\">{1}</span></div>",
                headerColor, alertTitle);

            sb.AppendFormat("<p>Dear <strong>{0}</strong>,</p>", Encode(employeeName));

            string detail = isLateCheckIn
                ? "Your attendance record shows a check-in time that is more than 15 minutes after your scheduled shift start."
                : "Your attendance record shows a check-out time that is more than 15 minutes before your scheduled shift end.";
            sb.AppendFormat("<p>{0}</p>", detail);

            // Details table
            sb.Append("<table border=\"1\" cellpadding=\"7\" cellspacing=\"0\" "
                    + "style=\"border-collapse:collapse;width:100%;max-width:500px;\">");
            sb.Append("<thead><tr style=\"background-color:#1F4E79;color:#ffffff;text-align:left;\">"
                    + "<th style=\"padding:8px;\">Field</th><th style=\"padding:8px;\">Value</th>"
                    + "</tr></thead><tbody>");

            AppendRow(sb, "Employee",    Encode(employeeName),       "#ffffff");
            AppendRow(sb, "Employee ID", Encode(employeeId),          "#EBF3FB");
            AppendRow(sb, "Designation", Encode(designationName),     "#ffffff");
            AppendRow(sb, "Date",        Encode(date),                "#EBF3FB");
            AppendRow(sb, shiftLabel,    FormatTimeSpan(shiftTime),   "#ffffff");
            AppendRow(sb, actualLabel,   FormatTimeSpan(actualTime),  "#EBF3FB");
            AppendRow(sb, "Deviation",   Encode(deviationStr),        "#fff4ce");

            sb.Append("</tbody></table><br/>");
            sb.Append("<p style=\"color:#888;font-size:11px;\">&#x1F6AB; This is an automated notification. Please do not reply to this email.</p>");
            sb.Append("</body></html>");

            return sb.ToString();
        }

        private static void AppendRow(StringBuilder sb, string label, string value, string bgColor)
        {
            sb.AppendFormat(
                "<tr style=\"background-color:{0};\"><td style=\"padding:7px;font-weight:600;\">{1}</td><td style=\"padding:7px;\">{2}</td></tr>",
                bgColor, label, value);
        }

        // ─── Helpers ──────────────────────────────────────────────────────────────

        private static string FormatTimeSpan(TimeSpan t)
        {
            int h    = (int)t.TotalHours % 24;
            int h12  = h % 12 == 0 ? 12 : h % 12;
            string ampm = h >= 12 ? "PM" : "AM";
            return string.Format("{0}:{1:D2} {2}", h12, t.Minutes, ampm);
        }

        private static string Encode(string value)
        {
            if (string.IsNullOrEmpty(value)) return "-";
            return value
                .Replace("&",  "&amp;")
                .Replace("<",  "&lt;")
                .Replace(">",  "&gt;")
                .Replace("\"", "&quot;");
        }
    }
}
