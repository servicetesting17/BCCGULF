using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Crm.Sdk.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BCCGULF.Plugins
{
    public class OnUpdateEvaluationItem : IPlugin
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

            var target = context.PostEntityImages["PostImage"];

            if (target.LogicalName != EvaluationItem.EntityName)
                return;

            try
            {
                tracingService.Trace("OnUpdateEvaluationItem: Plugin execution started.");

                // ── Step 1: Get Evaluation ID from EvaluationItem record ──
                if (!target.Contains(EvaluationItem.Evaluation) || target[EvaluationItem.Evaluation] == null)
                {
                    tracingService.Trace("Evaluation is not set on EvaluationItem. Exiting.");
                    return;
                }



                var evaluationRef = target.GetAttributeValue<EntityReference>(EvaluationItem.Evaluation);
                tracingService.Trace("Evaluation: {0}", evaluationRef.Id);

                // Recalculate rollup fields on the parent Evaluation so rollup values stay current
                try
                {
                    var rollupFields = new[]
                    {
                        Evaluation.TotalEmployeeInput,
                        Evaluation.TotalMangerInput,
                        Evaluation.TotalWeitage
                    };

                    foreach (var rollupField in rollupFields)
                    {
                        tracingService.Trace("Recalculating rollup field {0} for Evaluation {1}", rollupField, evaluationRef.Id);
                        var recalcReq = new CalculateRollupFieldRequest
                        {
                            Target = new EntityReference(Evaluation.EntityName, evaluationRef.Id),
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

                // ── Step 2: Fetch all EvaluationItems for this Evaluation ──
                string fetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                          <condition attribute='{3}' operator='eq' value='{4}' />
                        </filter>
                      </entity>
                    </fetch>",
                    EvaluationItem.EntityName,     // {0} bcc_evaluationitem
                    EvaluationItem.ManagerInput,   // {1} bcc_managerinput
                    EvaluationItem.PrimaryKey,     // {2} bcc_evaluationitemid
                    EvaluationItem.Evaluation,     // {3} bcc_evaluationid
                    evaluationRef.Id);             // {4} evaluation id

                var evaluationItems = service.RetrieveMultiple(new FetchExpression(fetchXml));
                tracingService.Trace("Evaluation Items found: {0}", evaluationItems.Entities.Count);

                if (evaluationItems.Entities.Count == 0)
                {
                    tracingService.Trace("No Evaluation Items found for this Evaluation. Exiting.");
                    return;
                }

                // ── Step 3: Check if all ManagerInput fields are filled ──
                List<int?> managerInputs = new List<int?>();
                foreach (var item in evaluationItems.Entities)
                {
                    int? managerInput = item.Contains(EvaluationItem.ManagerInput) && item[EvaluationItem.ManagerInput] != null
                        ? item.GetAttributeValue<int>(EvaluationItem.ManagerInput)
                        : (int?)null;
                    managerInputs.Add(managerInput);
                    tracingService.Trace("EvaluationItem {0}: ManagerInput = {1}", item.Id, managerInput ?? -1);
                }

                // Check if any ManagerInput is null
                if (managerInputs.Any(mi => mi == null))
                {
                    tracingService.Trace("Not all Manager Input fields are filled. Exiting without updating Evaluation rating.");
                    return;
                }

                // ── Step 4: Calculate average of all ManagerInput values ──
                int averageManagerInput = (int)managerInputs.Sum(mi => mi.Value);
                tracingService.Trace("Average Manager Input: {0}", averageManagerInput);

                // ── Step 5: Fetch all active Grade records ──
                string gradeFetchXml = string.Format(@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                      <entity name='{0}'>
                        <attribute name='{1}' />
                        <attribute name='{2}' />
                        <attribute name='{3}' />
                        <filter type='and'>
                          <condition attribute='statecode' operator='eq' value='0' />
                        </filter>
                      </entity>
                    </fetch>",
                    Grade.EntityName,       // {0} bcc_grade
                    Grade.PrimaryName,      // {1} bcc_name
                    Grade.Percentage,       // {2} bcc_percentage
                    Grade.Grade_);          // {3} bcc_grade

                var grades = service.RetrieveMultiple(new FetchExpression(gradeFetchXml));
                tracingService.Trace("Grade records found: {0}", grades.Entities.Count);

                if (grades.Entities.Count == 0)
                {
                    tracingService.Trace("No Grade records found. Cannot determine grade for Evaluation.");
                    return;
                }

                // ── Step 6: Match average to grade based on percentage ranges ──
                int? matchedGradeOptionValue = null;
                foreach (var grade in grades.Entities)
                {
                    string percentageRange = grade.Contains(Grade.Percentage)
                        ? grade.GetAttributeValue<string>(Grade.Percentage)
                        : "";

                    tracingService.Trace("Grade: {0}, Percentage Range: {1}", 
                        grade.GetAttributeValue<string>(Grade.PrimaryName), percentageRange);

                    if (IsAverageInRange(averageManagerInput, percentageRange))
                    {
                        int gradeOptionValue = grade.Contains(Grade.Grade_)
                            ? grade.GetAttributeValue<OptionSetValue>(Grade.Grade_).Value
                            : 0;
                        matchedGradeOptionValue = gradeOptionValue;
                        tracingService.Trace("Matched Grade: {0} (Option Value: {1})", 
                            grade.GetAttributeValue<string>(Grade.PrimaryName), gradeOptionValue);
                        break;
                    }
                }

                if (matchedGradeOptionValue == null)
                {
                    tracingService.Trace("Could not match average to any grade range.");
                    return;
                }

                // ── Step 7: Update Evaluation record with matched grade ──
                var updateEvaluation = new Entity(Evaluation.EntityName, evaluationRef.Id);
                updateEvaluation[Evaluation.Rating] = new OptionSetValue(matchedGradeOptionValue.Value);
                service.Update(updateEvaluation);

                tracingService.Trace("Evaluation record updated with Grade Rating. Average: {0}, Matched Grade Option Value: {1}",
                    averageManagerInput, matchedGradeOptionValue);
                tracingService.Trace("OnUpdateEvaluationItem: Plugin execution completed successfully.");
            }
            catch (Exception ex)
            {
                tracingService.Trace("OnUpdateEvaluationItem: Error - {0}", ex.ToString());
                throw new InvalidPluginExecutionException(
                    "An error occurred in OnUpdateEvaluationItem plugin: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Determines if the average percentage falls within a grade's percentage range.
        /// Supports formats like: "100-85%", "85-70%", "69-50%", "< 50%", "<50%"
        /// Boundary logic: Higher boundary is inclusive, lower boundary is exclusive.
        /// Example: For "85-70%", average >= 85 is in this range, average < 85 goes to next lower range.
        /// </summary>
        private static bool IsAverageInRange(int average, string percentageRange)
        {
            if (string.IsNullOrWhiteSpace(percentageRange))
                return false;

            percentageRange = percentageRange.Trim().Replace("%", "").Trim();

            // Normalize common less-than characters (some records use a unicode '˂' instead of ASCII '<')
            percentageRange = percentageRange.Replace("˂", "<").Replace("＜", "<").Replace("‹", "<");

            // Handle "< 50" or "<50" format (less than)
            if (percentageRange.Contains("<"))
            {
                int idx = percentageRange.IndexOf('<');
                string numberStr = new string(percentageRange.Substring(idx + 1)
                    .Where(c => char.IsDigit(c)).ToArray());
                if (int.TryParse(numberStr, out int threshold))
                {
                    return average < threshold;
                }
            }

            // Handle "100-85" format (range)
            if (percentageRange.Contains("-"))
            {
                string[] parts = percentageRange.Split('-');
                if (parts.Length == 2)
                {
                    string highStr = new string(parts[0].Where(c => char.IsDigit(c)).ToArray());
                    string lowStr = new string(parts[1].Where(c => char.IsDigit(c)).ToArray());

                    if (int.TryParse(highStr, out int high) && int.TryParse(lowStr, out int low))
                    {
                        // Ensure high is the larger number
                        if (high < low)
                        {
                            int temp = high;
                            high = low;
                            low = temp;
                        }

                        // Boundary logic: high is inclusive, low is exclusive
                        return average <= high && average > low;
                    }
                }
            }

            return false;
        }
    }
}
