using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace BCCGULF.Workflows
{
    public sealed class UpdateJobApplicationBPFStage : CodeActivity
    {
        [RequiredArgument]
        [Input("Job Application")]
        [ReferenceTarget("bcc_jobapplication")]
        public InArgument<EntityReference> JobApplication { get; set; }

        [RequiredArgument]
        [Input("Active Stage")]
        [ReferenceTarget("processstage")]
        public InArgument<EntityReference> ActiveStage { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
             IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            var tracingService = context.GetExtension<ITracingService>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            var jobApplication = JobApplication.Get(context);
            var activeStage = ActiveStage.Get(context);

            if (jobApplication == null || jobApplication.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Job Application is required.");
            }

            if (activeStage == null || activeStage.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Active Stage is required.");
            }

            tracingService?.Trace("Updating BPF stage for Job Application {0} to stage {1}", jobApplication.Id, activeStage.Id);

            var fetchXml = string.Format(
                @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='bcc_jobapplicationapproval'>
    <attribute name='businessprocessflowinstanceid' />
    <attribute name='activestageid' />
    <order attribute='bpf_name' descending='false' />
    <filter type='and'>
      <condition attribute='bpf_bcc_jobapplicationid' operator='eq' uiname='Idrees Ahmad' uitype='bcc_jobapplication' value='{0}' />
    </filter>
  </entity>
</fetch>",
                jobApplication.Id);

            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (results.Entities.Count == 0)
            {
                tracingService?.Trace("No BPF record found for Job Application {0}.", jobApplication.Id);
                return;
            }

            var bpfRecord = results.Entities[0];
            bpfRecord["activestageid"] = new EntityReference("processstage", activeStage.Id);
            service.Update(bpfRecord);

            tracingService?.Trace("BPF stage updated successfully for Job Application {0}.", jobApplication.Id);
        }
    }
}
