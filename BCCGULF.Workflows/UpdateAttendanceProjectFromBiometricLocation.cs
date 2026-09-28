using BCCGULF.Workflows.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace BCCGULF.Workflows
{
    public class UpdateAttendanceProjectFromBiometricLocation : CodeActivity
    {
        [RequiredArgument]
        [Input("Attendance")]
        [ReferenceTarget(Attendence.EntityName)]
        public InArgument<EntityReference> Attendance { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var workflowContext = context.GetExtension<IWorkflowContext>();
            var tracingService = context.GetExtension<ITracingService>();
            var serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            var service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            var attendanceRef = Attendance.Get(context);
            if (attendanceRef == null || attendanceRef.Id == Guid.Empty)
            {
                throw new InvalidOperationException("Attendance is required.");
            }

            tracingService?.Trace("Starting attendance project update for {0}", attendanceRef.Id);

            var attendance = service.Retrieve(
                Attendence.EntityName,
                attendanceRef.Id,
                new ColumnSet(Attendence.Mid1, Attendence.Mid1Location, Attendence.Project));

            if (attendance == null)
            {
                tracingService?.Trace("Attendance record not found: {0}", attendanceRef.Id);
                return;
            }

            string mid1 = attendance.GetAttributeValue<string>(Attendence.Mid1);
            string mid1Location = attendance.GetAttributeValue<string>(Attendence.Mid1Location);
            var fetchXml = $@"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
  <entity name='{BioMarticLocations.EntityName}'>
    <attribute name='{BioMarticLocations.PrimaryKey}' />
    <attribute name='{BioMarticLocations.Project}' />
    <filter type='and'>
      <condition attribute='statecode' operator='eq' value='0' />
      <condition attribute='{BioMarticLocations.Mid1}' operator='eq' value='{mid1.Trim()}' />
      <condition attribute='{BioMarticLocations.Project}' operator='not-null' />
    </filter>
  </entity>
</fetch>";

            var results = service.RetrieveMultiple(new FetchExpression(fetchXml));
            if (results.Entities.Count == 0)
            {
                tracingService?.Trace("No biometric location found for Mid1 '{0}' and Mid1Location '{1}'.", mid1, mid1Location);
                return;
            }

            var biometricLocation = results.Entities.First();
            if (!biometricLocation.Contains(BioMarticLocations.Project) || biometricLocation[BioMarticLocations.Project] == null)
            {
                tracingService?.Trace("Biometric location {0} does not have a project assigned.", biometricLocation.Id);
                return;
            }
            var projectRef = biometricLocation.GetAttributeValue<EntityReference>(BioMarticLocations.Project);
            var update = new Entity(Attendence.EntityName, attendance.Id);
            update[Attendence.Project] = new EntityReference(projectRef.LogicalName, projectRef.Id);
            service.Update(update);
            tracingService?.Trace("Updated attendance project to {0} for attendance {1}", projectRef.Id, attendance.Id);
        }
    }
}
