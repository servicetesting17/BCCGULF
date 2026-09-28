using BCCGULF.Common;
using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace BCCGULF.CustomAPI
{
    /// <summary>
    /// Custom Action to upload SignContract and/or SignedJobOffer files to JobOffer record
    /// Input Parameters: JsonInput (required containing PassportNo, SignContract, SignedJobOffer objects)
    /// </summary>
    public class JobOfferUpdates : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            tracingService.Trace("JobOfferUpdates plugin started.");
            try
            {
                tracingService.Trace("Checking for JsonInput parameter...");
                if (!context.InputParameters.Contains("JsonInput") || context.InputParameters["JsonInput"] == null)
                    throw new InvalidPluginExecutionException("JsonInput is required");

                string json = context.InputParameters["JsonInput"].ToString();
                tracingService.Trace("JsonInput received: {0}", json);

                if (string.IsNullOrWhiteSpace(json))
                    throw new InvalidPluginExecutionException("JsonInput is empty");

                tracingService.Trace("Deserializing JSON...");
                JobOfferUpdateModel model = DeserializeJson<JobOfferUpdateModel>(json, tracingService);

                if (model == null)
                    throw new InvalidPluginExecutionException("Invalid JSON");

                // Validate PassportNo
                if (string.IsNullOrWhiteSpace(model.PassportNo))
                    throw new InvalidPluginExecutionException("PassportNo is required");

                // Validate at least one file is provided
                if ((model.SignContract == null || string.IsNullOrWhiteSpace(model.SignContract.Base64Content)) &&
                    (model.SignedJobOffer == null || string.IsNullOrWhiteSpace(model.SignedJobOffer.Base64Content)))
                {
                    throw new InvalidPluginExecutionException("At least one of SignContract or SignedJobOffer must be provided");
                }

                tracingService.Trace("Fetching JobOffer by PassportNo: {0}", model.PassportNo);
                Guid jobOfferId = FetchJobOfferByPassportNo(service, model.PassportNo, tracingService);

                tracingService.Trace("Processing files for JobOfferId: {0}", jobOfferId);
                ProcessFiles(service, jobOfferId, model, tracingService);

                tracingService.Trace("JobOfferUpdates: Execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("JobOfferUpdates: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException($"Error in JobOfferUpdates plugin: {ex.Message}", ex);
            }
        }

        private T DeserializeJson<T>(string json, ITracingService tracingService)
        {
            tracingService.Trace("DeserializeJson<{0}> called.", typeof(T).Name);
            var serializer = new DataContractJsonSerializer(typeof(T));
            try
            {
                using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                {
                    var obj = (T)serializer.ReadObject(ms);
                    tracingService.Trace("Deserialization successful.");
                    return obj;
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace("Deserialization failed: {0}", ex.ToString());
                throw;
            }
        }

        private Guid FetchJobOfferByPassportNo(IOrganizationService service, string passportNo, ITracingService tracingService)
        {
            string fetchXml = string.Format(@"
                <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>
                  <entity name='{0}'>
                    <attribute name='{1}' />
                    <filter type='and'>
                      <condition attribute='{2}' operator='eq' value='{3}' />
                      <condition attribute='statecode' operator='eq' value='0' />
                    </filter>
                  </entity>
                </fetch>",
                JobOffer.EntityName,      // {0} bcc_joboffer
                JobOffer.PrimaryKey,      // {1} bcc_jobofferid
                JobOffer.PassportNo,      // {2} bcc_passportno
                passportNo.Replace("'", "''"));  // {3} escaped passport no

            var jobOffers = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (jobOffers.Entities.Count == 0)
            {
                tracingService.Trace("No JobOffer found with PassportNo: {0}", passportNo);
                throw new InvalidPluginExecutionException(string.Format("No JobOffer record found with Passport Number: {0}", passportNo));
            }

            return jobOffers.Entities[0].Id;
        }

        private static void ProcessFiles(IOrganizationService service, Guid jobOfferId, JobOfferUpdateModel model, ITracingService tracingService)
        {
            tracingService.Trace("ProcessFiles called.");

            // Upload SignContract file (if provided)
            if (model.SignContract != null &&
                !string.IsNullOrWhiteSpace(model.SignContract.Base64Content) &&
                !string.IsNullOrWhiteSpace(model.SignContract.FileName))
            {
                tracingService.Trace("Uploading SignContract file: {0}", model.SignContract.FileName);
                byte[] fileBytes = Convert.FromBase64String(model.SignContract.Base64Content);

                Common.Common.UploadFile(
                    service,
                    JobOffer.EntityName,
                    jobOfferId,
                    JobOffer.SignContract,
                    model.SignContract.FileName,
                    model.SignContract.FileType,
                    fileBytes,
                    tracingService
                );
            }

            // Upload SignedJobOffer file (if provided)
            if (model.SignedJobOffer != null &&
                !string.IsNullOrWhiteSpace(model.SignedJobOffer.Base64Content) &&
                !string.IsNullOrWhiteSpace(model.SignedJobOffer.FileName))
            {
                tracingService.Trace("Uploading SignedJobOffer file: {0}", model.SignedJobOffer.FileName);
                byte[] fileBytes = Convert.FromBase64String(model.SignedJobOffer.Base64Content);

                Common.Common.UploadFile(
                    service,
                    JobOffer.EntityName,
                    jobOfferId,
                    JobOffer.SignedJobOffer,
                    model.SignedJobOffer.FileName,
                    model.SignedJobOffer.FileType,
                    fileBytes,
                    tracingService
                );
            }
            tracingService.Trace("ProcessFiles completed.");
        }

        [DataContract]
        public class JobOfferUpdateModel
        {
            [DataMember] public string PassportNo { get; set; }
            [DataMember] public FileModel SignContract { get; set; }
            [DataMember] public FileModel SignedJobOffer { get; set; }
        
        }
    }
}
