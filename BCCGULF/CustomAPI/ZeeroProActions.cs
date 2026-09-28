using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BCCGULF.CustomAPI
{
    public sealed class ZeeroProAction : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            if (!context.InputParameters.Contains("Message") || context.InputParameters["Message"] == null)
                throw new InvalidPluginExecutionException("Message is required.");

            var message = context.InputParameters["Message"].ToString().Trim();
            switch (message.ToLowerInvariant())
            {
                case "createrole":
                    new ZeeroProCreateRoleAction().Execute(serviceProvider);
                    break;
                case "apply":
                    new ZeeroProApplyAction().Execute(serviceProvider);
                    break;
                case "shortlist":
                    new ZeeroProShortlistAction().Execute(serviceProvider);
                    break;
                case "invite":
                    new ZeeroProInviteAction().Execute(serviceProvider);
                    break;
                default:
                    throw new InvalidPluginExecutionException("Unsupported Message: " + message + ".");
            }
        }
    }

    internal abstract class ZeeroProActionBase
    {
        protected abstract string Operation { get; }

        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);
            var tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                var message = GetRequiredString(context, "Message");
                if (!string.Equals(message, Operation, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidPluginExecutionException("Message must be '" + Operation + "'.");
                if (context.PrimaryEntityId == Guid.Empty || string.IsNullOrWhiteSpace(context.PrimaryEntityName))
                    throw new InvalidPluginExecutionException("This action must be bound to a saved Dataverse record.");

                tracing.Trace("ZeeroPro {0} action started for {1} {2}.", Operation, context.PrimaryEntityName, context.PrimaryEntityId);
                var response = ExecuteOperation(context, service, tracing) ?? string.Empty;
                tracing.Trace("ZeeroPro {0} output Response: {1}", Operation, response);
                context.OutputParameters["Response"] = response;
            }
            catch (InvalidPluginExecutionException)
            {
                throw;
            }
            catch (Exception ex)
            {
                tracing.Trace("ZeeroPro {0} action failed: {1}", Operation, ex);
                throw new InvalidPluginExecutionException("ZeeroPro " + Operation + " failed: " + ex.Message, ex);
            }
        }

        protected abstract string ExecuteOperation(IPluginExecutionContext context, IOrganizationService service, ITracingService tracing);

        protected static Entity RetrieveCurrent(IPluginExecutionContext context, IOrganizationService service)
        {
            return service.Retrieve(context.PrimaryEntityName, context.PrimaryEntityId, new ColumnSet(true));
        }

        protected static string StringValue(Entity entity, params string[] names)
        {
            foreach (var name in names)
            {
                if (entity.Contains(name) && entity[name] != null)
                    return entity[name] is EntityReference reference ? reference.Name : entity[name].ToString();
            }
            return null;
        }

        protected static EntityReference LookupValue(Entity entity, params string[] names)
        {
            foreach (var name in names)
            {
                var value = entity.GetAttributeValue<EntityReference>(name);
                if (value != null)
                    return value;
            }
            return null;
        }

        protected static void SetAndSave(Entity entity, string field, object value, IOrganizationService service)
        {
            if (value == null)
                return;
            var update = new Entity(entity.LogicalName, entity.Id);
            update[field] = value;
            service.Update(update);
        }

        private static string GetRequiredString(IPluginExecutionContext context, string name)
        {
            if (!context.InputParameters.Contains(name) || context.InputParameters[name] == null)
                throw new InvalidPluginExecutionException(name + " is required.");
            var value = context.InputParameters[name].ToString();
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidPluginExecutionException(name + " cannot be empty.");
            return value.Trim();
        }
    }

    internal sealed class ZeeroProCreateRoleAction : ZeeroProActionBase
    {
        protected override string Operation { get { return "CreateRole"; } }

        protected override string ExecuteOperation(IPluginExecutionContext context, IOrganizationService service, ITracingService tracing)
        {
            var record = RetrieveCurrent(context, service);
            var position = LookupValue(record, "bcc_position");
            var location = StringValue(record, "bcc_location");
            var payload = new Dictionary<string, object>
            {
                { "roleName", position == null ? null : position.Name },
                { "description", StringValue(record, "bcc_description") },
                { "businessUnit", "Ellington" },
                { "country", "United Arab Emirates" },
                { "state", location },
                { "city", location },
                { "workModel", "Onsite" },
                { "statusOfRole", "Draft" }
            };
            var result = ZeeroProHttp.PostJson("/client/service/api/v1/jobs", payload, service, tracing);
            SetAndSave(record, "bcc_integrationid", ZeeroProHttp.JsonString(result, "jobId"), service);
            return result;
        }
    }

    internal sealed class ZeeroProApplyAction : ZeeroProActionBase
    {
        protected override string Operation { get { return "Apply"; } }

        protected override string ExecuteOperation(IPluginExecutionContext context, IOrganizationService service, ITracingService tracing)
        {
            var record = RetrieveCurrent(context, service);
            var posting = LookupValue(record, "bcc_jobpostingid");
            if (posting == null)
                throw new InvalidPluginExecutionException("A Job Posting is required before applying.");

            var postingRecord = service.Retrieve(posting.LogicalName, posting.Id, new ColumnSet("bcc_location","bcc_name", "bcc_description"));
            var file = ZeeroProHttp.DownloadFile(service, record, "bcc_resume");
            var candidate = new Dictionary<string, object>
            {
                { "firstName", StringValue(record, "bcc_firstname") },
                { "lastName", StringValue(record, "bcc_lastname") },
                { "email", StringValue(record, "emailaddress", "emailaddress1") },
                { "phone", StringValue(record, "bcc_mobilephone", "mobilephone") },
                { "location", StringValue(postingRecord, "bcc_location") },
                { "roleName", record.GetAttributeValue<EntityReference>("bcc_designation").Name },
                { "screeningAnswers", new Dictionary<string, object>() }
            };
              var config = ZeeroProHttp.GetConfig(service, tracing);
            tracing.Trace("ZeeroPro Apply candidate data: {0} {1}", JsonSerializer.Serialize(candidate), config.ClientId);
          
            var result = ZeeroProHttp.PostMultipart("/client/service/api/v1/process/career/apply", service, tracing,
                new Dictionary<string, string>
                {
                    { "candidateData", JsonSerializer.Serialize(candidate) },
                    { "clientId", config.ClientId },
                    { "templateId", "null" },
                    { "job_description", StringValue(postingRecord, "bcc_description") }
                }, file, "resume");

            
            SetAndSave(record, "bcc_integrationid", ZeeroProHttp.JsonString(result, "id"), service);
            SetAndSave(record, "bcc_resumeurl", ZeeroProHttp.JsonString(result, "resumeUrl"), service);
            return result;
        }
    }

    internal sealed class ZeeroProShortlistAction : ZeeroProActionBase
    {
        protected override string Operation { get { return "Shortlist"; } }

        protected override string ExecuteOperation(IPluginExecutionContext context, IOrganizationService service, ITracingService tracing)
        {
            var record = RetrieveCurrent(context, service);
            var config = ZeeroProHttp.GetConfig(service, tracing);
            var payload = new Dictionary<string, object>
            {
                { "clientId", config.ClientId },
                { "roleName",record.GetAttributeValue<EntityReference>("bcc_designation").Name },
                { "candidateEmails", new[] { StringValue(record, "emailaddress") } }
            };
            tracing.Trace("ZeeroPro Shortlist payload: {0}", JsonSerializer.Serialize(payload));
            return ZeeroProHttp.PostJson("/client/service/api/v1/process/career/shortlist/recruiter", payload, service, tracing);
        }
    }

    internal sealed class ZeeroProInviteAction : ZeeroProActionBase
    {
        protected override string Operation { get { return "Invite"; } }

        protected override string ExecuteOperation(IPluginExecutionContext context, IOrganizationService service, ITracingService tracing)
        {
            var record = RetrieveCurrent(context, service);
            var config = ZeeroProHttp.GetConfig(service, tracing);
            var invite = new Dictionary<string, object>
            {
                { "name", StringValue(record, "bcc_name") },
                { "email", StringValue(record,  "emailaddress") },
                { "mobile", StringValue(record, "bcc_mobilephone") }
            };
            var jsonData = new Dictionary<string, object>
            {
                { "roleId", record.GetAttributeValue<EntityReference>("bcc_designation").Name },
                { "clientId", config.ClientId },
                { "tenantClientId", config.TenantId },
                { "stages", new[]
                    {
                        new Dictionary<string, object>
                        {
                            { "stageType", "aiinterview" },
                            { "stageName", "ai assessment" }
                        }
                    }
                },
                { "isEmail", true },
                { "isWhatsapp", false }
            };
            tracing.Trace("ZeeroPro Invite payload: {0}", JsonSerializer.Serialize(jsonData));
            tracing.Trace("ZeeroPro Invite invitees: {0}", JsonSerializer.Serialize(new Dictionary<string, object> { { "invites", new[] { invite } } }));   
            return ZeeroProHttp.PostMultipart("/client/service/api/v1/process/save", service, tracing,
                new Dictionary<string, string>
                {
                    { "jsonData", JsonSerializer.Serialize(jsonData) },
                    { "invitees", JsonSerializer.Serialize(new Dictionary<string, object> { { "invites", new[] { invite } } }) }
                }, null, null);
        }
    }

    internal static class ZeeroProHttp
    {
        internal sealed class Config
        {
            public string BaseUrl;
            public string ApiKey;
            public string TenantId;
            public string ClientId;
        }

        internal sealed class FileData
        {
            public byte[] Content;
            public string FileName;
            public string ContentType;
        }

        internal static Config GetConfig(IOrganizationService service, ITracingService tracing)
        {
            return new Config
            {
                BaseUrl = GetEnvironmentValue(service, "bcc_baseUrl"),
                ApiKey = GetEnvironmentValue(service, "bcc_apiKey"),
                TenantId = GetEnvironmentValue(service, "bcc_tenantId"),
                ClientId = GetEnvironmentValue(service, "bcc_clientId")
            };
        }

        internal static string PostJson(string path, object payload, IOrganizationService service, ITracingService tracing)
        {
            var config = GetConfig(service, tracing);
            using (var client = CreateClient(config, tracing))
            using (var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"))
                return Send(client.PostAsync(config.BaseUrl.TrimEnd('/') + path, content), tracing);
        }

        internal static string PostMultipart(string path, IOrganizationService service, ITracingService tracing,
            IDictionary<string, string> fields, FileData file, string fileField)
        {
            var config = GetConfig(service, tracing);
            using (var client = CreateClient(config, tracing))
            using (var content = new MultipartFormDataContent())
            {
                foreach (var field in fields)
                    content.Add(new StringContent(field.Value ?? string.Empty), field.Key);
                if (file != null)
                {
                    var fileContent = new ByteArrayContent(file.Content);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                    content.Add(fileContent, fileField, file.FileName);
                }
                return Send(client.PostAsync(config.BaseUrl.TrimEnd('/') + path, content), tracing);
            }
        }

        internal static FileData DownloadFile(IOrganizationService service, Entity record, string column)
        {
            var response = (InitializeFileBlocksDownloadResponse)service.Execute(new InitializeFileBlocksDownloadRequest
            {
                Target = new EntityReference(record.LogicalName, record.Id),
                FileAttributeName = column
            });
            var bytes = new List<byte>();
            long offset = 0;
            while (offset < response.FileSizeInBytes)
            {
                var length = (int)Math.Min(4 * 1024 * 1024, response.FileSizeInBytes - offset);
                var block = (DownloadBlockResponse)service.Execute(new DownloadBlockRequest
                {
                    FileContinuationToken = response.FileContinuationToken,
                    Offset = offset,
                    BlockLength = length
                });
                bytes.AddRange(block.Data);
                offset += block.Data.Length;
            }
            var fileName = record.GetAttributeValue<string>(column + "_name") ?? column;
            return new FileData
            {
                Content = bytes.ToArray(),
                FileName = fileName,
                ContentType = GetResumeContentType(fileName)
            };
        }

        private static string GetResumeContentType(string fileName)
        {
            switch (Path.GetExtension(fileName).ToLowerInvariant())
            {
                case ".pdf":
                    return "application/pdf";
                case ".doc":
                    return "application/msword";
                case ".docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                default:
                    throw new InvalidPluginExecutionException(
                        "Invalid resume file type. Allowed types: PDF, DOC, DOCX.");
            }
        }

        private static HttpClient CreateClient(Config config, ITracingService tracing)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("X-Api-Key", config.ApiKey);
            tracing.Trace("ZeeroPro authenticating with API Key: {0} - {1}", config.ApiKey, config.BaseUrl.TrimEnd('/') + "/authentication/api/v1/auth/service/token");
            var tokenResponse = client.PostAsync(
                config.BaseUrl.TrimEnd('/') + "/authentication/api/v1/auth/service/token",
                new StringContent(string.Empty, Encoding.UTF8, "application/json"))
                .GetAwaiter().GetResult();
            var tokenBody = tokenResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            tokenResponse.Dispose();
            if (!tokenResponse.IsSuccessStatusCode)
                throw new InvalidPluginExecutionException("ZeeroPro authentication returned HTTP " + (int)tokenResponse.StatusCode + ": " + tokenBody);
            var accessToken = JsonString(tokenBody, "accessToken");
            tracing.Trace("ZeeroPro authentication returned accessToken: {0}", accessToken);
            if (string.IsNullOrWhiteSpace(accessToken))
                throw new InvalidPluginExecutionException("ZeeroPro authentication returned no accessToken.");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

        private static string Send(Task<HttpResponseMessage> task, ITracingService tracing)
        {
            using (var response = task.GetAwaiter().GetResult())
            {
                var body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                tracing.Trace("ZeeroPro response HTTP {0}: {1}", (int)response.StatusCode, body);
                if (!response.IsSuccessStatusCode)
                    throw new InvalidPluginExecutionException("ZeeroPro API returned HTTP " + (int)response.StatusCode + ": " + body);
                return body;
            }
        }

        private static string GetEnvironmentValue(IOrganizationService service, string schemaName)
        {
            var query = new QueryExpression("environmentvariabledefinition") { ColumnSet = new ColumnSet("defaultvalue") };
            query.Criteria.AddCondition("schemaname", ConditionOperator.Equal, schemaName);
            query.LinkEntities.Add(new LinkEntity("environmentvariabledefinition", "environmentvariablevalue", "environmentvariabledefinitionid", "environmentvariabledefinitionid", JoinOperator.LeftOuter)
            {
                Columns = new ColumnSet("value"),
                EntityAlias = "current"
            });
            var definitions = service.RetrieveMultiple(query).Entities;
            if (definitions.Count == 0)
                throw new InvalidPluginExecutionException("Environment Variable '" + schemaName + "' was not found.");
            var definition = definitions[0];
            var current = definition.GetAttributeValue<AliasedValue>("current.value");
            var value = current == null ? definition.GetAttributeValue<string>("defaultvalue") : current.Value as string;
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidPluginExecutionException("Environment Variable '" + schemaName + "' has no value.");
            return value;
        }

       

        internal static string JsonString(string json, string property)
        {
            using (var document = JsonDocument.Parse(json))
            {
                JsonElement value;
                return document.RootElement.TryGetProperty(property, out value) ? value.ToString() : null;
            }
        }
    }
}