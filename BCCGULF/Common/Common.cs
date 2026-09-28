using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Runtime.Serialization;
using System.Text;


namespace BCCGULF.Common
{
    public static class Common
    {
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
