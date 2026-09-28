using BCCGULF.Constants;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace BCCGULF.Plugins
{
    /// <summary>
    /// Plugin to upload Company Job Offer file to external API
    /// Triggered on Update of JobOffer entity (Post-operation, Asynchronous)
    /// 
    /// Logic:
    /// 1. Get PreImage and PostImage
    /// 2. Check if file column is NOT present in PreImage but IS present in PostImage
    /// 3. If file was newly uploaded, extract it and call the API
    /// 4. Matches curl: passport_no, file_base64, filename
    /// 
    /// Register on: Update of bcc_joboffer
    /// Message: Update
    /// Stage: Post-operation
    /// Mode: Asynchronous (recommended for file uploads)
    /// Pre-Image: All attributes including the file column
    /// Post-Image: All attributes including the file column
    /// </summary>
    public class JobOfferUploads : IPlugin
    {
        // Configuration - Update these with your actual values
        private const string BASE_API_URL = "https://eportal.yovant.ae/api/bcc/webhook/";
        private const string API_KEY = "cc782227-a72c-40e0-97e4-6e7771f2d0c1";

        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                tracingService.Trace("CompanyJobOfferUpload Plugin: Execution started.");
                tracingService.Trace("Message: {0}, Stage: {1}, Mode: {2}", context.MessageName, context.Stage, context.Mode);

                // Validate entity type
                if (context.PrimaryEntityName != JobOffer.EntityName)
                {
                    tracingService.Trace("Plugin triggered on wrong entity: {0}. Expected: {1}", context.PrimaryEntityName, JobOffer.EntityName);
                    return;
                }

                // Validate message is Update
                if (context.MessageName.ToLower() != "update")
                {
                    tracingService.Trace("Plugin triggered on wrong message: {0}. Expected: update", context.MessageName);
                    return;
                }

                // Get Pre-Image and Post-Image
                Entity preImage = null;
                Entity postImage = null;

                if (context.PreEntityImages.Contains("Image"))
                {
                    preImage = context.PreEntityImages["Image"];
                    tracingService.Trace("PreImage retrieved");
                }
                else
                {
                    tracingService.Trace("PreImage not found. Ensure plugin is registered with PreImage.");
                    return;
                }

                if (context.PostEntityImages.Contains("Image"))
                {
                    postImage = context.PostEntityImages["Image"];
                    tracingService.Trace("PostImage retrieved");
                }
                else
                {
                    tracingService.Trace("PostImage not found. Ensure plugin is registered with PostImage.");
                    return;
                }
                string[] ColumnsNames = new string[] { JobOffer.JobOffer_, JobOffer.EmployeeContract, JobOffer.VisaFee, JobOffer.MohreLetter };
                foreach (var FILE_COLUMN_NAME in ColumnsNames)
                {
                    tracingService.Trace("Processing file column: {0}", FILE_COLUMN_NAME);
                    string FILE_NAME_COLUMN = FILE_COLUMN_NAME + "_name";  // Automatically derived
                    // Check if file column is NOT in PreImage but IS in PostImage (newly uploaded)
                    bool fileInPreImage = preImage.Contains(FILE_COLUMN_NAME) && preImage[FILE_COLUMN_NAME] != null;
                    bool fileInPostImage = postImage.Contains(FILE_COLUMN_NAME) && postImage[FILE_COLUMN_NAME] != null;
                    string API_URL = string.Empty;
                    switch (FILE_COLUMN_NAME)
                    {
                        case JobOffer.JobOffer_:
                            tracingService.Trace("Checking JobOffer file column: {0}", FILE_COLUMN_NAME);
                            API_URL = BASE_API_URL + "mohre-offer";
                            break;
                        case JobOffer.EmployeeContract:
                            tracingService.Trace("Checking EmployeeContract file column: {0}", FILE_COLUMN_NAME);
                            API_URL = BASE_API_URL + "employment-contract";
                            break;
                        case JobOffer.VisaFee:
                            tracingService.Trace("Checking VisaFee file column: {0}", FILE_COLUMN_NAME);
                            API_URL = BASE_API_URL + "entry-permit";
                            break;
                        case JobOffer.MohreLetter:
                            tracingService.Trace("Checking MohreLetter file column: {0}", FILE_COLUMN_NAME);
                            API_URL = BASE_API_URL + "mohre-offer";
                            break;
                        default:
                            break;
                    }

                    tracingService.Trace("File in PreImage: {0}, File in PostImage: {1}", fileInPreImage, fileInPostImage);

                    // Only proceed if file was newly uploaded (not in PreImage but in PostImage)
                    if (!fileInPreImage && fileInPostImage)
                    {


                        tracingService.Trace("File was newly uploaded. Proceeding with API call.");
                        var initializeRequest = new InitializeFileBlocksDownloadRequest
                        {
                            Target = new EntityReference(postImage.LogicalName, postImage.Id),
                            FileAttributeName = FILE_COLUMN_NAME
                        };

                        var initializeResponse = (InitializeFileBlocksDownloadResponse)service.Execute(initializeRequest);
                        string token = initializeResponse.FileContinuationToken;
                        long fileSize = initializeResponse.FileSizeInBytes;

                        tracingService.Trace("File size: {0} bytes", fileSize);

                        // Download file content (handles files larger than 4MB by downloading in blocks)
                        byte[] fileContent = DownloadFileContent(service, token, fileSize, tracingService);

                        if (fileContent == null || fileContent.Length == 0)
                        {
                            tracingService.Trace("File content is empty. Skipping API call.");
                            return;
                        }

                        tracingService.Trace("File content retrieved: {0} bytes", fileContent.Length);

                        // Get PassportNo from PostImage
                        string passportNo = postImage.Contains(JobOffer.PassportNo)
                            ? postImage.GetAttributeValue<string>(JobOffer.PassportNo)
                            : "";

                        if (string.IsNullOrWhiteSpace(passportNo))
                        {
                            tracingService.Trace("PassportNo is empty in PostImage. Skipping API call.");
                            return;
                        }

                        tracingService.Trace("PassportNo retrieved: {0}", passportNo);

                        // Get filename from the file name attribute
                        string filename = "";
                        if (postImage.Contains(FILE_NAME_COLUMN))
                        {
                            filename = postImage.GetAttributeValue<string>(FILE_NAME_COLUMN);
                            tracingService.Trace("Filename retrieved: {0}", filename);
                        }

                        // Convert file to Base64
                        string fileBase64 = Convert.ToBase64String(fileContent);
                        tracingService.Trace("File converted to Base64. Base64 length: {0}", fileBase64.Length);

                        // Call the external API
                        string apiResponse = CallExternalApi(passportNo, fileBase64, filename, tracingService, API_URL, postImage, FILE_COLUMN_NAME, service);
                        tracingService.Trace("API Response: {0}", apiResponse);

                        tracingService.Trace("CompanyJobOfferUpload Plugin: Execution completed successfully.");
                        return;
                    }
                    else
                    {
                        tracingService.Trace("File was already present in PreImage. This is not a new upload. Skipping API call or File is not present in PostImage. Skipping API call.");
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("CompanyJobOfferUpload Plugin: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in CompanyJobOfferUpload plugin: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Downloads file content in blocks (handles files larger than 4MB)
        /// Uses InitializeFileBlocksDownloadRequest and DownloadBlockRequest
        /// </summary>
        private byte[] DownloadFileContent(IOrganizationService service, string token, long fileSize, ITracingService tracingService)
        {
            const int BLOCK_SIZE = 4 * 1024 * 1024; // 4MB blocks
            var fileData = new MemoryStream();

            try
            {
                long offset = 0;

                while (offset < fileSize)
                {
                    // Calculate block length (last block might be smaller)
                    int blockLength = (int)Math.Min(BLOCK_SIZE, fileSize - offset);

                    tracingService.Trace("Downloading block: offset={0}, length={1}", offset, blockLength);

                    // Download this block
                    var downloadBlockRequest = new DownloadBlockRequest
                    {
                        FileContinuationToken = token,
                        BlockLength = blockLength,
                        Offset = offset
                    };

                    var downloadResponse = (DownloadBlockResponse)service.Execute(downloadBlockRequest);
                    byte[] blockContent = downloadResponse.Data;

                    if (blockContent == null || blockContent.Length == 0)
                    {
                        tracingService.Trace("Downloaded block is empty at offset {0}", offset);
                        break;
                    }

                    tracingService.Trace("Block downloaded: {0} bytes", blockContent.Length);
                    fileData.Write(blockContent, 0, blockContent.Length);

                    offset += blockLength;
                }

                return fileData.ToArray();
            }
            catch (Exception ex)
            {
                tracingService.Trace("DownloadFileContent error: {0}", ex.ToString());
                throw;
            }
            finally
            {
                fileData.Dispose();
            }
        }

        /// <summary>
        /// Calls the external API to upload the company offer
        /// Dynamically builds payload based on file type
        /// </summary>
        private string CallExternalApi(string passportNo, string fileBase64, string filename, ITracingService tracingService, string apiUrl, Entity postImage, string fileColumnName, IOrganizationService service)
        {
            tracingService.Trace("CallExternalApi called for PassportNo: {0}, Filename: {1}, FileType: {2}", passportNo, filename, fileColumnName);

            // Dataverse plugin hosts may not select TLS 1.2 by default on .NET Framework.
            // Keep certificate validation enabled; the API certificate must still be valid.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Add("X-API-Key", API_KEY);
                    client.Timeout = TimeSpan.FromSeconds(30);

                    tracingService.Trace("API URL: {0}", apiUrl);

                    // Build dynamic request JSON based on file type
                    string requestJson = BuildDynamicPayload(passportNo, fileBase64, filename, fileColumnName, postImage, tracingService, service);

                    tracingService.Trace("Request body prepared. Size: {0} bytes", requestJson.Length);

                    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                    // Make HTTP POST request
                    tracingService.Trace("Sending POST request to API...");
                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                    // Read response
                    string responseBody = response.Content.ReadAsStringAsync().Result;
                    tracingService.Trace("API Response Status: {0}", response.StatusCode);
                    tracingService.Trace("API Response Body: {0}", responseBody);

                    if (!response.IsSuccessStatusCode)
                    {
                        tracingService.Trace("API returned non-success status: {0}", response.StatusCode);
                        throw new InvalidOperationException(
                            $"API call failed with status {response.StatusCode}: {responseBody}");
                    }

                    return $"Success: {responseBody}";
                }
                catch (Exception ex)
                {
                    tracingService.Trace("CallExternalApi failed: {0}", ex.ToString());
                    if (ex.GetBaseException() is System.Security.Authentication.AuthenticationException)
                    {
                        tracingService.Trace("TLS certificate validation failed. Verify that the API certificate is valid, unexpired, matches the hostname, and includes the complete certificate chain.");
                    }
                    throw;
                }
            }
        }

        /// <summary>
        /// Builds dynamic JSON payload based on file type
        /// Different file types have different required fields
        /// </summary>
        private string BuildDynamicPayload(string passportNo, string fileBase64, string filename, string fileColumnName, Entity postImage, ITracingService tracingService, IOrganizationService service)
        {
            tracingService.Trace("BuildDynamicPayload for file type: {0}", fileColumnName);

            switch (fileColumnName)
            {
                case JobOffer.MohreLetter:
                    return BuildMohreLettterPayload(passportNo, fileBase64, filename, postImage, tracingService, service);

                case JobOffer.VisaFee:
                    return BuildPreApprovalWorkPermitPayload(passportNo, fileBase64, filename, postImage, tracingService, service);

                case JobOffer.EmployeeContract:
                    return BuildEmployeeContractPayload(passportNo, fileBase64, filename, postImage, tracingService, service);

                case JobOffer.JobOffer_:
                default:
                    return BuildBasicPayload(passportNo, fileBase64, filename, tracingService);
            }
        }

        /// <summary>
        /// Payload for MohreLetter:
        /// {
        ///     "passport_no": "Y1850639",
        ///     "file_base64": "...",
        ///     "filename": "mohre_offer.pdf",
        ///     "employment_contract_base64": "...",
        ///     "st_transaction_number": "ST123456",
        ///     "mb_transaction_number": "MB789012"
        /// }
        /// </summary>
        private string BuildMohreLettterPayload(string passportNo, string fileBase64, string filename, Entity postImage, ITracingService tracingService, IOrganizationService service)
        {
            tracingService.Trace("Building MohreLetter payload");

            var payload = new MohreLettterApiRequest
            {
                passport_no = passportNo,
                file_base64 = fileBase64,
                filename = filename,
                employment_contract_base64 = GetEmploymentContractBase64(postImage, tracingService, service),
                st_transaction_number = "", // Adjust attribute name as needed
                mb_transaction_number = ""     // Adjust attribute name as needed
            };

            return SerializeToJson(payload);
        }

        /// <summary>
        /// Payload for PreApprovalWorkPermit:
        /// {
        ///     "passport_no": "Y1850639",
        ///     "file_base64": "...",
        ///     "filename": "entry_permit.pdf",
        ///     "entry_permit_number": "201/2026/12345",
        ///     "date_of_issue": "2026-05-11",
        ///     "uid_no": "U1234567"
        /// }
        /// </summary>
        private string BuildPreApprovalWorkPermitPayload(string passportNo, string fileBase64, string filename, Entity postImage, ITracingService tracingService, IOrganizationService service)
        {
            tracingService.Trace("Building PreApprovalWorkPermit payload");

            var payload = new PreApprovalWorkPermitApiRequest
            {
                passport_no = passportNo,
                file_base64 = fileBase64,
                filename = filename,
                entry_permit_number = "",        // Adjust attribute name as needed
                date_of_issue = "", // Adjust attribute name as needed
                uid_no = ""                       // Adjust attribute name as needed
            };

            return SerializeToJson(payload);
        }

        /// <summary>
        /// Payload for EmployeeContract (basic format)
        /// </summary>
        private string BuildEmployeeContractPayload(string passportNo, string fileBase64, string filename, Entity postImage, ITracingService tracingService, IOrganizationService service)
        {
            tracingService.Trace("Building EmployeeContract payload");

            var payload = new BasicApiRequest
            {
                passport_no = passportNo,
                file_base64 = fileBase64,
                filename = filename
            };

            return SerializeToJson(payload);
        }

        /// <summary>
        /// Basic payload for Company Offer and other files
        /// </summary>
        private string BuildBasicPayload(string passportNo, string fileBase64, string filename, ITracingService tracingService)
        {
            tracingService.Trace("Building basic payload");

            var payload = new BasicApiRequest
            {
                passport_no = passportNo,
                file_base64 = fileBase64,
                filename = filename
            };

            return SerializeToJson(payload);
        }

        /// <summary>
        /// Get employment contract as Base64 if it exists
        /// Uses InitializeFileBlocksDownloadRequest to properly download the file
        /// </summary>
        private string GetEmploymentContractBase64(Entity postImage, ITracingService tracingService, IOrganizationService service)
        {
            try
            {
                if (postImage.Contains(JobOffer.EmployeeContract) && postImage[JobOffer.EmployeeContract] != null)
                {
                    tracingService.Trace("Employment contract field is present in PostImage");

                    // Use the proper file download API
                    var initializeRequest = new InitializeFileBlocksDownloadRequest
                    {
                        Target = new EntityReference(postImage.LogicalName, postImage.Id),
                        FileAttributeName = JobOffer.EmployeeContract
                    };

                    var initializeResponse = (InitializeFileBlocksDownloadResponse)service.Execute(initializeRequest);
                    string token = initializeResponse.FileContinuationToken;
                    long fileSize = initializeResponse.FileSizeInBytes;

                    tracingService.Trace("Employment contract file size: {0} bytes", fileSize);

                    // Download file content using block download
                    byte[] contractContent = DownloadFileContent(service, token, fileSize, tracingService);

                    if (contractContent != null && contractContent.Length > 0)
                    {
                        tracingService.Trace("Employment contract downloaded: {0} bytes", contractContent.Length);
                        return Convert.ToBase64String(contractContent);
                    }
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("Error retrieving employment contract: {0}", ex.ToString());
            }

            tracingService.Trace("Employment contract not found or is empty");
            return "";
        }

        /// <summary>
        /// Safely get string attribute value
        /// </summary>
        private string GetAttributeValue(Entity entity, string attributeName, ITracingService tracingService)
        {
            try
            {
                if (entity.Contains(attributeName) && entity[attributeName] != null)
                {
                    string value = entity.GetAttributeValue<string>(attributeName);
                    tracingService.Trace("Attribute {0}: {1}", attributeName, value ?? "NULL");
                    return value ?? "";
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("Error getting attribute {0}: {1}", attributeName, ex.ToString());
            }

            return "";
        }

        /// <summary>
        /// Safely get date attribute value and format as yyyy-MM-dd
        /// </summary>
        private string GetDateAttributeValue(Entity entity, string attributeName, ITracingService tracingService)
        {
            try
            {
                if (entity.Contains(attributeName) && entity[attributeName] != null)
                {
                    DateTime dateValue = entity.GetAttributeValue<DateTime>(attributeName);
                    string formattedDate = dateValue.ToString("yyyy-MM-dd");
                    tracingService.Trace("Date Attribute {0}: {1}", attributeName, formattedDate);
                    return formattedDate;
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("Error getting date attribute {0}: {1}", attributeName, ex.ToString());
            }

            return "";
        }

        /// <summary>
        /// Serialize object to JSON string using DataContractJsonSerializer
        /// </summary>
        private string SerializeToJson(object obj)
        {
            var serializer = new DataContractJsonSerializer(obj.GetType());
            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, obj);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }

        [DataContract]
        public class BasicApiRequest
        {
            [DataMember]
            public string passport_no { get; set; }

            [DataMember]
            public string file_base64 { get; set; }

            [DataMember]
            public string filename { get; set; }
        }

        [DataContract]
        public class MohreLettterApiRequest
        {
            [DataMember]
            public string passport_no { get; set; }

            [DataMember]
            public string file_base64 { get; set; }

            [DataMember]
            public string filename { get; set; }

            [DataMember]
            public string employment_contract_base64 { get; set; }

            [DataMember]
            public string st_transaction_number { get; set; }

            [DataMember]
            public string mb_transaction_number { get; set; }
        }

        [DataContract]
        public class PreApprovalWorkPermitApiRequest
        {
            [DataMember]
            public string passport_no { get; set; }

            [DataMember]
            public string file_base64 { get; set; }

            [DataMember]
            public string filename { get; set; }

            [DataMember]
            public string entry_permit_number { get; set; }

            [DataMember]
            public string date_of_issue { get; set; }

            [DataMember]
            public string uid_no { get; set; }
        }

        [DataContract]
        public class ApiRequest
        {
            [DataMember]
            public string passport_no { get; set; }

            [DataMember]
            public string file_base64 { get; set; }

            [DataMember]
            public string filename { get; set; }
        }
    }
}
