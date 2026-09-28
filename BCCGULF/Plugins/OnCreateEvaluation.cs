using BCCGULF.Constants;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace BCCGULF.Plugins
{
    public class OnCreateEvaluation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = serviceFactory.CreateOrganizationService(context.UserId);

            if (context.InputParameters == null
                || !context.InputParameters.Contains("Target")
                || !(context.InputParameters["Target"] is Entity))
            {
                return;
            }

            var target = (Entity)context.InputParameters["Target"];

            if (target.LogicalName != Evaluation.EntityName)
                return;

            try
            {
                tracingService.Trace("OnCreateEvaluation: Plugin execution started.");

                // ── Step 1: Get BookableResource from Evaluation record ──
                if (!target.Contains(Evaluation.BookableResource) || target[Evaluation.BookableResource] == null)
                {
                    tracingService.Trace("BookableResource is not set on Evaluation. Exiting.");
                    return;
                }

                var bookableResourceRef = target.GetAttributeValue<EntityReference>(Evaluation.BookableResource);
                tracingService.Trace("BookableResource: {0}", bookableResourceRef.Id);

                // ── Step 2: Retrieve Bookable Resource with Type, Department, and Division ──
                var bookableResource = service.Retrieve(
                    BookableResource.EntityName,
                    bookableResourceRef.Id,
                    new ColumnSet(BookableResource.Type, BookableResource.Department, BookableResource.Division, BookableResource.PrimaryName));
                string bookableResourceName = bookableResource.Contains(BookableResource.PrimaryName)
                   ? bookableResource.GetAttributeValue<string>(BookableResource.PrimaryName)
                   : "";
                tracingService.Trace("Bookable Resource retrieved: {0}", bookableResource.Id);

                // ── Step 2a: Get Month and Year from Evaluation record ──
                string monthValue = "";                
                if (target.Contains(Evaluation.Month) && target[Evaluation.Month] != null)
                {
                    int monthOptionValue = target.GetAttributeValue<OptionSetValue>(Evaluation.Month).Value;
                    monthValue = GetMonthName(monthOptionValue);
                    tracingService.Trace("Month: {0} (Value: {1})", monthValue, monthOptionValue);
                }               

                // ── Step 2b: Check for duplicate Evaluation records (same Bookable Resource, Month, Year) ──
                // Ignore current record since we're running on Post Image
                string duplicateFetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false' top='1'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <filter type='and'>
                          <condition attribute='{2}' operator='eq' value='{3}' />
                          <condition attribute='{4}' operator='eq' value='{5}' />
                          <condition attribute='{6}' operator='eq' value='{7}' />
                          <condition attribute='{8}' operator='ne' value='{9}' />
                        </filter>
                      </entity>
                    </fetch>",
                    Evaluation.EntityName,              // {0} bcc_evaluation
                    Evaluation.PrimaryKey,             // {1} bcc_evaluationid
                    Evaluation.BookableResource,       // {2} bcc_bookableresourceid
                    bookableResourceRef.Id,            // {3} bookable resource id
                    Evaluation.Month,                  // {4} bcc_month
                    target.Contains(Evaluation.Month) && target[Evaluation.Month] != null 
                        ? target.GetAttributeValue<OptionSetValue>(Evaluation.Month).Value.ToString()
                        : "null",                      // {5} month option value
                    Evaluation.Year,                   // {6} bcc_year
                    DateTime.Now.Year.ToString(),      // {7} current year
                    Evaluation.PrimaryKey,             // {8} bcc_evaluationid (to exclude current)
                    target.Id);                        // {9} current record id

                var duplicateRecords = service.RetrieveMultiple(new FetchExpression(duplicateFetchXml));

                if (duplicateRecords.Entities.Count > 0)
                {
                    string errorMessage = string.Format(
                        "An Evaluation record already exists for this Bookable Resource ({0}) with Month: {1} and Year: {2}. Duplicate records are not allowed.",
                        bookableResourceName, monthValue, DateTime.Now.Year);
                    tracingService.Trace("ERROR: {0}", errorMessage);
                    throw new InvalidPluginExecutionException(errorMessage);
                }

                tracingService.Trace("No duplicate Evaluation record found. Proceeding with creation.");


                // ── Step 3: Validate that Type, Department, and Division are set ──
                if (!bookableResource.Contains(BookableResource.Type) || bookableResource[BookableResource.Type] == null)
                {
                    tracingService.Trace("Type is not set on the Bookable Resource. Exiting.");
                    return;
                }

                if (!bookableResource.Contains(BookableResource.Department) || bookableResource[BookableResource.Department] == null)
                {
                    tracingService.Trace("Department is not set on the Bookable Resource. Exiting.");
                    return;
                }

                if (!bookableResource.Contains(BookableResource.Division) || bookableResource[BookableResource.Division] == null)
                {
                    tracingService.Trace("Division is not set on the Bookable Resource. Exiting.");
                    return;
                }

                // ── Step 4: Extract Type, Department, and Division values ──
                int typeValue = bookableResource.GetAttributeValue<OptionSetValue>(BookableResource.Type).Value;
                int departmentValue = bookableResource.GetAttributeValue<OptionSetValue>(BookableResource.Department).Value;
                int divisionValue = bookableResource.GetAttributeValue<OptionSetValue>(BookableResource.Division).Value;

                tracingService.Trace("Type: {0}, Department: {1}, Division: {2}", typeValue, departmentValue, divisionValue);

                // ── Step 5: Fetch Evaluation Matrix items matching Type, Department, and Division ──
                string fetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <attribute name='{3}' />
                        <attribute name='{4}' />
                        <attribute name='{5}' />
  <attribute name='{6}' />
<attribute name='{7}' />

                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='{2}' operator='eq' value='{8}' />
                          <condition attribute='{3}' operator='eq' value='{9}' />
                          <condition attribute='{4}' operator='eq' value='{10}' />
                        </filter>
                      </entity>
                    </fetch>",
                    EvaluationMatrix.EntityName,       // {0} bcc_evaluationmatrix
                    EvaluationMatrix.PrimaryKey,       // {1} bcc_evaluationmatrixid
                    EvaluationMatrix.ApplicableTo,     // {2} bcc_applicableto
                    EvaluationMatrix.Department,       // {3} bcc_department
                    EvaluationMatrix.Division,         // {4} new_division
                    EvaluationMatrix.Weightage,        // {5} bcc_weightage
                    EvaluationMatrix.Remarks,          // {6} bcc_remarks
                    EvaluationMatrix.PrimaryName,     // {7} bcc_name
                    typeValue,                         // {8} type option value
                    departmentValue,                   // {9} department option value
                    divisionValue);                    // {10} division option value

                var evaluationMatrices = service.RetrieveMultiple(new FetchExpression(fetchXml));
                tracingService.Trace("Evaluation Matrix items found: {0}", evaluationMatrices.Entities.Count);

                if (evaluationMatrices.Entities.Count == 0)
                {
                    tracingService.Trace("No matching Evaluation Matrix items found. Exiting.");
                    return;
                }

                // ── Step 6: Create Evaluation Item records for each matching Evaluation Matrix item ──
                foreach (var matrixItem in evaluationMatrices.Entities)
                {
                    string itemName = matrixItem.GetAttributeValue<string>(EvaluationMatrix.PrimaryName) ?? "Evaluation Item";
                    int weightage = matrixItem.Contains(EvaluationMatrix.Weightage)
                        ? matrixItem.GetAttributeValue<int>(EvaluationMatrix.Weightage)
                        : 0;
                    string remarks = matrixItem.Contains(EvaluationMatrix.Remarks)
                        ? matrixItem.GetAttributeValue<string>(EvaluationMatrix.Remarks)
                        : "";

                    tracingService.Trace("Creating Evaluation Item - Name: {0}, Weightage: {1}", itemName, weightage);

                    var evaluationItem = new Entity(EvaluationItem.EntityName);
                    evaluationItem[EvaluationItem.PrimaryName] = itemName;
                    evaluationItem[EvaluationItem.Evaluation] = new EntityReference(Evaluation.EntityName, target.Id);
                    evaluationItem[EvaluationItem.Weightage] = weightage;
                    evaluationItem[EvaluationItem.Remarks] = remarks;
                    evaluationItem[EvaluationItem.BookableResource] = bookableResourceRef;
                    service.Create(evaluationItem);
                    tracingService.Trace("Evaluation Item created: {0}", itemName);
                }

                // ── Step 7: Update Evaluation record with name format: BookableResourceName - Month - Year ──
               

                string evaluationName = bookableResourceName;
                if (!string.IsNullOrEmpty(monthValue))
                {
                    evaluationName += " - " + monthValue;
                }    
                
                evaluationName += " - " + DateTime.Now.Year;

                tracingService.Trace("Updating Evaluation record name to: {0}", evaluationName);

                var updateEvaluation = new Entity(Evaluation.EntityName, target.Id);
                updateEvaluation[Evaluation.PrimaryName] = evaluationName;
                service.Update(updateEvaluation);
                try
                {
                    var rollupFields = new[]
                    {
                        Evaluation.TotalWeitage
                    };

                    foreach (var rollupField in rollupFields)
                    {
                        tracingService.Trace("Recalculating rollup field {0} for Evaluation {1}", rollupField, target.Id);
                        var recalcReq = new CalculateRollupFieldRequest
                        {
                            Target = new EntityReference(Evaluation.EntityName, target.Id),
                            FieldName = rollupField
                        };
                        service.Execute(recalcReq);
                    }
                }
                catch (Exception rex)
                {
                    tracingService.Trace("Failed to recalculate rollup fields: {0}", rex.ToString());
                    // Do not abort plugin execution for rollup failures; continue with normal processing
                }

                tracingService.Trace("OnCreateEvaluation: Plugin execution completed successfully. Created {0} Evaluation Items.", evaluationMatrices.Entities.Count);
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnCreateEvaluation: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in OnCreateEvaluation plugin: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Converts month option set value to month name
        /// </summary>
        private static string GetMonthName(int monthOptionValue)
        {
            switch (monthOptionValue)
            {
                case 755700000: return "January";
                case 755700001: return "February";
                case 755700002: return "March";
                case 755700003: return "April";
                case 755700004: return "May";
                case 755700005: return "June";
                case 755700006: return "July";
                case 755700007: return "August";
                case 755700008: return "September";
                case 755700009: return "October";
                case 755700010: return "November";
                case 755700011: return "December";
                default: return "";
            }
        }
    }
}
