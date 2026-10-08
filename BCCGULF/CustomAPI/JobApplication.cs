using BCCGULF.Common;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using static BCCGULF.Common.Common;

namespace BCCGULF.CustomAPI
{
    public class JobApplication : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("JobApplication plugin started.");
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
                JobApplicationModel model = DeserializeJson<JobApplicationModel>(json, tracingService);

                if (model == null)
                    throw new InvalidPluginExecutionException("Invalid JSON");

                tracingService.Trace("Building entity from model...");
                Entity entity = BuildEntityFromModel(model, service, tracingService);

                tracingService.Trace("Creating entity record...");
                Guid recordId = service.Create(entity);

                tracingService.Trace("Processing files...");
                ProcessFiles(service, recordId, model, tracingService);

                tracingService.Trace("Setting output parameter RecordId: {0}", recordId);
                context.OutputParameters["RecordId"] = recordId;
            }
            catch (Exception ex)
            {
                tracingService.Trace("Exception: {0}", ex.ToString());
                throw new InvalidPluginExecutionException($"Error in JobApplication plugin: {ex.Message}", ex);
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

        public static Entity BuildEntityFromModel(JobApplicationModel model, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("BuildEntityFromModel called.");
            var entity = new Entity("bcc_jobapplication");

            // 🔹 TEXT FIELDS
            SetIfNotEmpty(entity, "bcc_address", model.bcc_address, tracingService);
            SetIfNotEmpty(entity, "bcc_degreetype", model.bcc_degreetype, tracingService);
            SetIfNotEmpty(entity, "bcc_emiratesid", model.bcc_emiratesid, tracingService);
            SetIfNotEmpty(entity, "bcc_faculty", model.bcc_faculty, tracingService);
            SetIfNotEmpty(entity, "bcc_jobapplicationnumber", model.bcc_jobapplicationnumber, tracingService);
            SetIfNotEmpty(entity, "bcc_labourcardno", model.bcc_labourcardno, tracingService);
            SetIfNotEmpty(entity, "bcc_lastname", model.bcc_lastname, tracingService);
            SetIfNotEmpty(entity, "bcc_middlename", model.bcc_middlename, tracingService);
            SetIfNotEmpty(entity, "bcc_mohrerefnumber", model.bcc_mohrerefnumber, tracingService);
            SetIfNotEmpty(entity, "bcc_molnumber", model.bcc_molnumber, tracingService);
            SetIfNotEmpty(entity, "bcc_name", model.bcc_name, tracingService);
            SetIfNotEmpty(entity, "bcc_nationality_", model.bcc_nationality_, tracingService);
            SetIfNotEmpty(entity, "bcc_passportno", model.bcc_passportno, tracingService);
            SetIfNotEmpty(entity, "bcc_placeofbirth", model.bcc_placeofbirth, tracingService);
            SetIfNotEmpty(entity, "bcc_placeofissue", model.bcc_placeofissue, tracingService);
            SetIfNotEmpty(entity, "bcc_rejectremark", model.bcc_rejectremark, tracingService);
            SetIfNotEmpty(entity, "bcc_rejectresponse", model.bcc_rejectresponse, tracingService);
            SetIfNotEmpty(entity, "bcc_studymajors", model.bcc_studymajors, tracingService);
            SetIfNotEmpty(entity, "bcc_uidno", model.bcc_uidno, tracingService);
            SetIfNotEmpty(entity, "bcc_universityname", model.bcc_universityname, tracingService);
            SetIfNotEmpty(entity, "bcc_visano", model.bcc_visano, tracingService);
            SetIfNotEmpty(entity, "new_address", model.new_address, tracingService);
            SetIfNotEmpty(entity, "new_contactid", model.new_contactid, tracingService);
            SetIfNotEmpty(entity, "new_givennames", model.bcc_firstname, tracingService);
            SetIfNotEmpty(entity, "bcc_firstname", model.bcc_firstname, tracingService);

            // 🔹 NUMBERS / MONEY
            SetMoney(entity, "bcc_allowance", model.bcc_allowance, tracingService);
            SetMoney(entity, "bcc_basic", model.bcc_basic, tracingService);
            SetMoney(entity, "bcc_hra", model.bcc_hra, tracingService);
            SetMoney(entity, "bcc_totalsalary", model.bcc_totalsalary, tracingService);

            SetDecimal(entity, "bcc_labourfeeamount", model.bcc_labourfeeamount, tracingService);
            SetDecimal(entity, "bcc_labourfeecharged", model.bcc_labourfeecharged, tracingService);

            SetInt(entity, "bcc_degreeenddate", model.bcc_degreeenddate, tracingService);
            SetInt(entity, "bcc_degreestartdate", model.bcc_degreestartdate, tracingService);
            SetInt(entity, "bcc_graduationyear", model.bcc_graduationyear, tracingService);

            // 🔹 OPTIONS (CHOICE)
            SetOption(entity, "bcc_applicationfrom", model.bcc_applicationfrom, tracingService);
            SetOption(entity, "bcc_applicationfrom_", model.bcc_applicationfrom_, tracingService);
            SetOption(entity, "bcc_approvejobapplication", model.bcc_approvejobapplication, tracingService);
            SetOption(entity, "bcc_department", model.bcc_department, tracingService);
            SetOption(entity, "bcc_employerverification", model.bcc_employerverification, tracingService);
            SetOption(entity, "bcc_gender", model.bcc_gender, tracingService);
            SetOption(entity, "bcc_jobapplicationstatus", model.bcc_jobapplicationstatus, tracingService);
            SetOption(entity, "bcc_medicalinsurance", model.bcc_medicalinsurance, tracingService);
            SetOption(entity, "bcc_noticeperiod", model.bcc_noticeperiod, tracingService);
            SetOption(entity, "bcc_probationperiod", model.bcc_probationperiod, tracingService);
            SetOption(entity, "bcc_statuss", model.bcc_statuss, tracingService);
            SetOption(entity, "bcc_workmanscomp", model.bcc_workmanscomp, tracingService);

            // 🔹 DATES
            SetDate(entity, "bcc_dateofbirth", model.bcc_dateofbirth, tracingService);
            SetDate(entity, "bcc_joiningdate", model.bcc_joiningdate, tracingService);
            SetDate(entity, "bcc_labourcardexpiry", model.bcc_labourcardexpiry, tracingService);
            SetDate(entity, "bcc_ppexpirydate", model.bcc_ppexpirydate, tracingService);
            SetDate(entity, "bcc_probationcompletiondate", model.bcc_probationcompletiondate, tracingService);
            SetDate(entity, "bcc_visaexpirydate", model.bcc_visaexpirydate, tracingService);
            SetDate(entity, "new_moldate", model.new_moldate, tracingService);
            SetDate(entity, "new_molreceiveddate", model.new_molreceiveddate, tracingService);

            // 🔹 LOOKUPS (resolve by name from CRM)
            ResolveLookup(entity, "bcc_agency", "account", "name", model.bcc_agency, service, tracingService);
            ResolveLookup(entity, "bcc_contact", "contact", "fullname", model.bcc_contact, service, tracingService);
            ResolveLookup(entity, "bcc_designation", "bcc_designation", "bcc_designationid1", model.bcc_designation, service, tracingService);
            ResolveLookup(entity, "bcc_designationaspervisa", "bcc_designation", "bcc_designationid1", model.bcc_designationaspervisa, service, tracingService);
            ResolveLookup(entity, "bcc_designationincompany", "bcc_designation", "bcc_designationid1", model.bcc_designationincompany, service, tracingService);
            ResolveLookup(entity, "bcc_jobofferlookup", "bcc_joboffer", "bcc_givennames", model.bcc_jobofferlookup, service, tracingService);
            ResolveLookup(entity, "bcc_manpowerquota", "bcc_manpowerquota", "bcc_designationasperquota", model.bcc_manpowerquota, service, tracingService);
            ResolveLookup(entity, "bcc_manpowerrequest", "bcc_manpowerrequest", "bcc_name", model.bcc_manpowerrequest, service, tracingService);
            //ResolveLookup(entity, "bcc_nationality", "bcc_country", "bcc_name", model.bcc_nationality, service, tracingService);

            tracingService.Trace("BuildEntityFromModel completed.");
            return entity;
        }

        private static void SetIfNotEmpty(Entity e, string field, string value, ITracingService tracingService)
        {
            tracingService.Trace("SetIfNotEmpty: {0} = {1}", field, value);
            if (!string.IsNullOrWhiteSpace(value))
                e[field] = value;
        }

        private static void SetMoney(Entity e, string field, decimal? value, ITracingService tracingService)
        {
            tracingService.Trace("SetMoney: {0} = {1}", field, value);
            if (value.HasValue)
                e[field] = new Money(value.Value);
        }

        private static void SetDecimal(Entity e, string field, decimal? value, ITracingService tracingService)
        {
            tracingService.Trace("SetDecimal: {0} = {1}", field, value);
            if (value.HasValue)
                e[field] = value.Value;
        }

        private static void SetInt(Entity e, string field, int? value, ITracingService tracingService)
        {
            tracingService.Trace("SetInt: {0} = {1}", field, value);
            if (value.HasValue)
                e[field] = value.Value;
        }

        private static void SetOption(Entity e, string field, int? value, ITracingService tracingService)
        {
            tracingService.Trace("SetOption: {0} = {1}", field, value);
            if (value.HasValue)
                e[field] = new OptionSetValue(value.Value);
        }

        private static void SetDate(Entity e, string field, string value, ITracingService tracingService)
        {
            tracingService.Trace("SetDate: {0} = {1}", field, value);
            if (!string.IsNullOrEmpty(value) && !string.IsNullOrWhiteSpace(value))
            {
                try
                {
                    e[field] = Convert.ToDateTime(value);
                }
                catch (Exception ex)
                {
                    tracingService.Trace("SetDate failed for field {0} with value '{1}': {2}", field, value, ex.ToString());
                    throw;
                }
            }
        }

        private static void ResolveLookup(Entity e, string field, string targetEntity, string nameAttribute, string nameValue, IOrganizationService service, ITracingService tracingService)
        {
            tracingService.Trace("ResolveLookup: {0} = '{1}' (entity: {2}, nameAttr: {3})", field, nameValue, targetEntity, nameAttribute);

            if (string.IsNullOrEmpty(nameValue) || string.IsNullOrWhiteSpace(nameValue) || nameValue == null)
            {
                tracingService.Trace("ResolveLookup: Value is null or empty for {0}. Skipping.", field);
                return;
            }

            string fetchXml = string.Format(@"
                <fetch top='1' no-lock='true'>
                    <entity name='{0}'>
                        <attribute name='{1}' />
                        <filter>
                            <condition attribute='{2}' operator='eq' value='{3}' />
                        </filter>
                    </entity>
                </fetch>",
                targetEntity,
                targetEntity + "id",
                nameAttribute,
                nameValue.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;").Replace("\"", "&quot;").Replace("'", "&apos;"));

            tracingService.Trace("ResolveLookup FetchXML: {0}", fetchXml);
            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));

            if (results.Entities.Count > 0)
            {
                Guid recordId = results.Entities[0].Id;
                e[field] = new EntityReference(targetEntity, recordId);
                tracingService.Trace("ResolveLookup: Resolved {0} to {1}", field, recordId);
            }
            else
            {
                tracingService.Trace("ResolveLookup: No record found for {0} with {1} = '{2}'. Skipping.", targetEntity, nameAttribute, nameValue);
            }
        }

        private static void ProcessFiles(IOrganizationService service, Guid recordId, JobApplicationModel model, ITracingService tracingService)
        {
            tracingService.Trace("ProcessFiles called.");
            var properties = typeof(JobApplicationModel).GetProperties();

            foreach (var prop in properties)
            {
                if (prop.PropertyType == typeof(FileModel))
                {
                    var file = (FileModel)prop.GetValue(model);

                    if (file != null &&
                        !string.IsNullOrWhiteSpace(file.Base64Content) &&
                        !string.IsNullOrWhiteSpace(file.FileName))
                    {
                        tracingService.Trace("Uploading file: {0} ({1})", file.FileName, prop.Name);
                        byte[] fileBytes = Convert.FromBase64String(file.Base64Content);

                        // 🔥 Using UploadFile method
                    Common.Common.UploadFile(
                            service,
                            "bcc_jobapplication",
                            recordId,
                            prop.Name,
                            file.FileName,
                            file.FileType,
                            fileBytes,
                            tracingService
                        );
                    }
                }
            }
            tracingService.Trace("ProcessFiles completed.");
        }

       

        [DataContract]
        public class JobApplicationModel
        {
            // 🔹 TEXT / SIMPLE FIELDS

            [DataMember] public string bcc_address { get; set; }
            [DataMember] public string bcc_agency { get; set; }
            [DataMember] public decimal? bcc_allowance { get; set; }
            [DataMember] public int? bcc_applicationfrom { get; set; }
            [DataMember] public int? bcc_applicationfrom_ { get; set; }
            [DataMember] public int? bcc_approvejobapplication { get; set; }
            [DataMember] public decimal? bcc_basic { get; set; }
            [DataMember] public string bcc_cancellationformjauploaddate { get; set; }
            [DataMember] public string bcc_contact { get; set; }
            [DataMember] public string bcc_dateofbirth { get; set; }
            [DataMember] public int? bcc_degreeenddate { get; set; }
            [DataMember] public int? bcc_degreestartdate { get; set; }
            [DataMember] public string bcc_degreetype { get; set; }
            [DataMember] public int? bcc_department { get; set; }
            [DataMember] public string bcc_designation { get; set; }
            [DataMember] public string bcc_designationaspervisa { get; set; }
            [DataMember] public string bcc_designationincompany { get; set; }
            [DataMember] public string bcc_emiratesid { get; set; }
            [DataMember] public int? bcc_employerverification { get; set; }
            [DataMember] public string bcc_faculty { get; set; }
            [DataMember] public int? bcc_gender { get; set; }
            [DataMember] public int? bcc_graduationyear { get; set; }
            [DataMember] public decimal? bcc_hra { get; set; }
            [DataMember] public string bcc_jobapplicationid { get; set; }
            [DataMember] public string bcc_jobapplicationnumber { get; set; }
            [DataMember] public int? bcc_jobapplicationstatus { get; set; }
            [DataMember] public string bcc_jobofferlookup { get; set; }
            [DataMember] public string bcc_joiningdate { get; set; }
            [DataMember] public string bcc_labourcardexpiry { get; set; }
            [DataMember] public string bcc_labourcardno { get; set; }
            [DataMember] public decimal? bcc_labourfeeamount { get; set; }
            [DataMember] public decimal? bcc_labourfeecharged { get; set; }
            [DataMember] public string bcc_lastname { get; set; }
            [DataMember] public string bcc_firstname { get; set; }
            [DataMember] public string bcc_manpowerquota { get; set; }
            [DataMember] public string bcc_manpowerrequest { get; set; }
            [DataMember] public int? bcc_medicalinsurance { get; set; }
            [DataMember] public string bcc_middlename { get; set; }
            [DataMember] public string bcc_mohreletteruploaddate { get; set; }
            [DataMember] public string bcc_mohrerefnumber { get; set; }
            [DataMember] public string bcc_molnumber { get; set; }
            [DataMember] public string bcc_name { get; set; }
            [DataMember] public string bcc_nationality { get; set; }
            [DataMember] public string bcc_nationality_ { get; set; }
            [DataMember] public int? bcc_noticeperiod { get; set; }
            [DataMember] public string bcc_otherdocumentjauploaddate { get; set; }
            [DataMember] public string bcc_passport2jauploaddate { get; set; }
            [DataMember] public string bcc_passport3jauploaddate { get; set; }
            [DataMember] public string bcc_passportjauploaddste { get; set; }
            [DataMember] public string bcc_passportno { get; set; }
            [DataMember] public string bcc_photojauploaddate { get; set; }
            [DataMember] public string bcc_placeofbirth { get; set; }
            [DataMember] public string bcc_placeofissue { get; set; }
            [DataMember] public string bcc_ppexpirydate { get; set; }
            [DataMember] public string bcc_probationcompletiondate { get; set; }
            [DataMember] public int? bcc_probationperiod { get; set; }
            [DataMember] public string bcc_rejectremark { get; set; }
            [DataMember] public string bcc_rejectresponse { get; set; }
            [DataMember] public int? bcc_statuss { get; set; }
            [DataMember] public string bcc_studymajors { get; set; }
            [DataMember] public decimal? bcc_totalsalary { get; set; }
            [DataMember] public string bcc_uidno { get; set; }
            [DataMember] public string bcc_universityname { get; set; }
            [DataMember] public string bcc_visaexpirydate { get; set; }
            [DataMember] public string bcc_visano { get; set; }
            [DataMember] public string bcc_weeklyrest { get; set; }
            [DataMember] public string bcc_workerformjauploaddate { get; set; }
            [DataMember] public int? bcc_workmanscomp { get; set; }
            [DataMember] public string new_address { get; set; }
            [DataMember] public string new_contactid { get; set; }
            [DataMember] public string new_givennames { get; set; }
            [DataMember] public string new_moldate { get; set; }
            [DataMember] public string new_molreceiveddate { get; set; }

            // 🔥 FILE FIELDS (STANDARDIZED OBJECT)

            [DataMember] public FileModel bcc_cancellationpaper { get; set; }
            [DataMember] public FileModel bcc_degreecertificate { get; set; }
            [DataMember] public FileModel bcc_degreecertificate2 { get; set; }
            [DataMember] public FileModel bcc_jafile { get; set; }
            [DataMember] public FileModel bcc_mohreletter { get; set; }
            [DataMember] public FileModel bcc_otherdocument { get; set; }
            [DataMember] public FileModel bcc_passport { get; set; }
            [DataMember] public FileModel bcc_passport2 { get; set; }
            [DataMember] public FileModel bcc_passport3 { get; set; }
            [DataMember] public FileModel bcc_workerform { get; set; }
            [DataMember] public FileModel bcc_image { get; set; }
        }


       
    }
}


