using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using BCCGULF.Constants;
using System;
using System.Runtime.Serialization;
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
}
