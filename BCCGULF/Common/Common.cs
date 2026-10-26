using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using BCCGULF.Constants;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;


namespace BCCGULF.Common
{
    public static class Common
    {
        public static Guid CreateProjectedLeavesRecord(
            IOrganizationService service,
            EntityReference bookableResource,
            int month,
            int year,
            decimal days,
            ITracingService tracingService)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            if (bookableResource == null)
                throw new ArgumentNullException(nameof(bookableResource));
            if (tracingService == null)
                throw new ArgumentNullException(nameof(tracingService));
            if (bookableResource.LogicalName != BCCGULF.Constants.BookableResource.EntityName
                || bookableResource.Id == Guid.Empty)
                throw new ArgumentException("A valid Bookable Resource reference is required.", nameof(bookableResource));
            if (month < 1 || month > 12)
                throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");
            if (year < 2026 || year > 9999)
                throw new ArgumentOutOfRangeException(nameof(year), "Year must be between 2026 and 9999.");

            string monthName = ((ProjectedLeaves.Month_OptionSet)month).ToString();
            string resourceName = string.IsNullOrWhiteSpace(bookableResource.Name)
                ? bookableResource.Id.ToString()
                : bookableResource.Name;

            var projectedLeaves = new Entity(ProjectedLeaves.EntityName);
            projectedLeaves[ProjectedLeaves.PrimaryName] = string.Format("{0} - {1} {2}", resourceName, monthName, year);
            projectedLeaves[ProjectedLeaves.BookableResource] = bookableResource;
            projectedLeaves[ProjectedLeaves.Month] = month;
            projectedLeaves[ProjectedLeaves.MonthOfYear] = new OptionSetValue(month);
            projectedLeaves[ProjectedLeaves.Year] = year;
            projectedLeaves[ProjectedLeaves.Days] = days;

            try
            {
                Guid recordId = service.Create(projectedLeaves);
                tracingService.Trace("Projected Leaves record created: {0}", recordId);
                return recordId;
            }
            catch (Exception ex)
            {
                tracingService.Trace("CreateProjectedLeavesRecord failed: {0}", ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Monthly Annual Leave accrual for a Bookable Resource Type: Staff = 1.83 days, Worker = 2.5 days.
        /// </summary>
        public static decimal GetMonthlyAnnualLeaveDays(int resourceTypeValue)
        {
            return resourceTypeValue == (int)BookableResource.Type_OptionSet.Staff ? 1.83m : 2.5m;
        }

        /// <summary>
        /// First projected month for a joining date.
        /// Joined on or before the 15th: the joining month. Joined after the 15th: the following month.
        /// </summary>
        public static DateTime GetProjectionStartMonth(DateTime joiningDate)
        {
            var startMonth = new DateTime(joiningDate.Year, joiningDate.Month, 1);
            return joiningDate.Day > 15 ? startMonth.AddMonths(1) : startMonth;
        }

        /// <summary>
        /// Creates 12 monthly Projected Leaves records starting from startMonth.
        /// Months before 2026 are skipped (bcc_year minimum). Returns the number of records created.
        /// </summary>
        public static int CreateProjectedLeaves(
            IOrganizationService service,
            EntityReference bookableResource,
            DateTime startMonth,
            decimal monthlyDays,
            ITracingService tracingService)
        {
            int created = 0;

            for (int i = 0; i < 12; i++)
            {
                var period = startMonth.AddMonths(i);
                if (period.Year < 2026)
                {
                    tracingService.Trace("Skipping Projected Leaves for {0}: year is before 2026.", period.ToString("yyyy-MM"));
                    continue;
                }

                CreateProjectedLeavesRecord(service, bookableResource, period.Month, period.Year, monthlyDays, tracingService);
                created++;
            }

            tracingService.Trace("Projected Leaves created: {0} month(s) of {1} day(s) from {2}",
                created, monthlyDays, startMonth.ToString("yyyy-MM"));
            return created;
        }

        /// <summary>
        /// Leave type name from the lookup, retrieving the Leave Type record when the name isn't populated.
        /// </summary>
        public static string GetLeaveTypeName(IOrganizationService service, EntityReference leaveTypeRef)
        {
            if (leaveTypeRef == null)
                return string.Empty;

            if (!string.IsNullOrEmpty(leaveTypeRef.Name))
                return leaveTypeRef.Name;

            var leaveType = service.Retrieve(LeaveType.EntityName, leaveTypeRef.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet(LeaveType.PrimaryName));
            return leaveType.GetAttributeValue<string>(LeaveType.PrimaryName) ?? string.Empty;
        }

        public static bool IsCompensatoryLeave(string leaveTypeName)
        {
            return !string.IsNullOrEmpty(leaveTypeName)
                && leaveTypeName.IndexOf("Compensatory Leave", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Counts the attendance records in a Leave Request's bcc_attendance JSON, e.g.
        /// [{"id":"...","name":"...","etn":"new_biometric"}, ...]. Duplicate ids are counted once.
        /// Returns 0 for empty or null JSON.
        /// </summary>
        public static int CountAttendanceRecords(string attendanceJson)
        {
            if (string.IsNullOrWhiteSpace(attendanceJson))
                return 0;

            AttendanceReference[] records;
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(attendanceJson)))
            {
                records = (AttendanceReference[])new DataContractJsonSerializer(typeof(AttendanceReference[])).ReadObject(stream);
            }

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var record in records ?? new AttendanceReference[0])
            {
                if (record != null && !string.IsNullOrWhiteSpace(record.Id))
                    ids.Add(record.Id.Trim('{', '}', ' '));
            }

            return ids.Count;
        }

        public static void UploadFile(
    IOrganizationService service,
    string entityName,
    Guid recordId,
    string fileAttributeName,
    string fileName,
    string mimeType,
    byte[] fileContent,
    ITracingService tracingService)
        {
            tracingService.Trace("UploadFile called for {0} ({1})", fileName, fileAttributeName);
            try
            {
                // 1. Init
                var initRequest = new InitializeFileBlocksUploadRequest
                {
                    Target = new EntityReference(entityName, recordId),
                    FileAttributeName = fileAttributeName,
                    FileName = fileName
                };

                var initResponse = (InitializeFileBlocksUploadResponse)service.Execute(initRequest);

                string token = initResponse.FileContinuationToken;
                tracingService.Trace("File upload initialized. Token: {0}", token);

                // 2. Upload block
                string blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes("block-0001"));

                var uploadRequest = new UploadBlockRequest
                {
                    BlockId = blockId,
                    BlockData = fileContent,
                    FileContinuationToken = token
                };

                service.Execute(uploadRequest);
                tracingService.Trace("File block uploaded. BlockId: {0}", blockId);

                // 3. Commit
                var commitRequest = new CommitFileBlocksUploadRequest
                {
                    FileContinuationToken = token,
                    BlockList = new[] { blockId },
                    FileName = fileName,
                    MimeType = mimeType
                };

                service.Execute(commitRequest);
                tracingService.Trace("File upload committed.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("UploadFile failed: {0}", ex.ToString());
                throw;
            }
        }

    }
    [DataContract]
    public class FileModel
    {
        [DataMember] public string Base64Content { get; set; }
        [DataMember] public string FileName { get; set; }
        [DataMember] public string FileType { get; set; }
    }

    /// <summary>One entry of the Leave Request bcc_attendance JSON.</summary>
    [DataContract]
    public class AttendanceReference
    {
        [DataMember(Name = "id")] public string Id { get; set; }
        [DataMember(Name = "name")] public string Name { get; set; }
        [DataMember(Name = "etn")] public string EntityName { get; set; }
    }
}
