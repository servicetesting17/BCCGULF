using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Globalization;
using System.Linq;

namespace BCCGULF.CustomAPI
{
    /// <summary>
    /// Custom API: bcc_ProcessPayroll
    ///
    /// Input parameters:
    ///   resourceId (String) — BookableResource GUID
    ///   fromDate   (String) — period start, YYYY-MM-DD
    ///   toDate     (String) — period end,   YYYY-MM-DD
    ///   month      (String) — display label, e.g. "June 2026"
    ///
    /// Output parameters:
    ///   result (String) — success/error summary
    ///
    /// Financial fields on the payslip come directly from time entry cost columns:
    ///   bcc_basictotalcost, bcc_overtime1totalcost, bcc_overtime2totalcost,
    ///   bcc_overtime3totalcost, bcc_totalpayrollcost — pre-populated per entry.
    /// </summary>
    public class ProcessPayroll : IPlugin
    {
        //private const int HoursPerAbsentDay = 8;

        public void Execute(IServiceProvider serviceProvider)
        {
            var trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var service = factory.CreateOrganizationService(context.UserId);

            trace.Trace("ProcessPayroll started.");

            string resourceIdRaw = GetRequiredString(context, "resourceId");
            string fromDateStr = GetRequiredString(context, "fromDate");
            string toDateStr = GetRequiredString(context, "toDate");
            string month = GetRequiredString(context, "month");

            string cleanId = resourceIdRaw.Trim().Replace("{", "").Replace("}", "");
            if (!Guid.TryParse(cleanId, out Guid resourceGuid))
                throw new InvalidPluginExecutionException("resourceId is not a valid GUID.");

            // ── Guard: only process Worker resources ──────────────────────────────
            var res = service.Retrieve(
                        BookableResource.EntityName, resourceGuid,
                        new ColumnSet(
                            BookableResource.PrimaryName, BookableResource.EmployeeID,
                            BookableResource.JoiningDate,
                            BookableResource.MonthlyBasic, BookableResource.MonthlyAllowance, BookableResource.ConveyanceAllowance, BookableResource.Type, BookableResource.DesignationID
                        )
                    );
            var resourceTypeValue = res.GetAttributeValue<OptionSetValue>(BookableResource.Type)?.Value;
            if (resourceTypeValue == (int)BookableResource.Type_OptionSet.Worker)
            {
                DateTime fromDate = DateTime.ParseExact(fromDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
                DateTime toDate = DateTime.ParseExact(toDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;

                if (toDate < fromDate)
                    throw new InvalidPluginExecutionException("toDate cannot be earlier than fromDate.");

                // ── Step 1: FetchXML — time entries joined to BookableResource ────────
                //   Link-entity brings resource metadata (name, IDs, salary reference)
                //   in a single round-trip.
                string fetchXml = string.Format(
                    @"<fetch>
                    <entity name='{0}'>
                      <attribute name='{1}' />
                      <attribute name='{2}' />
                      <attribute name='{3}' />
                      <attribute name='{4}' />
                      <attribute name='{5}' />
                      <attribute name='{6}' />
                      <attribute name='{7}' />
                      <attribute name='{8}' />
                      <attribute name='{9}' />
                      <attribute name='{10}' />
                      <attribute name='{11}' />
                      <attribute name='{12}' />
                      <filter>
                        <condition attribute='{13}' operator='eq'          value='{14}' />
                        <condition attribute='{15}' operator='on-or-after'  value='{16}' />
                        <condition attribute='{15}' operator='on-or-before' value='{17}' />
                      </filter>
                      <link-entity name='{18}' from='{19}' to='{13}' link-type='inner' alias='res'>
                        <attribute name='{20}' />
                        <attribute name='{21}' />
                        <attribute name='{22}' />
                        <attribute name='{23}' />
                        <attribute name='{24}' />
                        <link-entity name='bcc_designation' from='bcc_designationid' to='bcc_designation_id' link-type='outer' alias='des'>
                            <attribute name='bcc_designationid1' />
                        </link-entity>
                      </link-entity>
                    </entity>
                  </fetch>",
                    TimeEntry.EntityName,                          // {0}
                    TimeEntry.EntryType,                           // {1}  new_entrytype
                    TimeEntry.AbsentReason,                        // {2}  bcc_absentreason
                    TimeEntry.Duration,                            // {3}  msdyn_duration
                    TimeEntry.Overtime1,                           // {4}  new_overtime1
                    TimeEntry.Overtime2,                           // {5}  new_overtime2
                    TimeEntry.Overtime3OnlyonSundaysPublicHolidays,// {6}  new_overtime3
                    TimeEntry.Total_Hours,                         // {7}  bcc_total_hours
                    TimeEntry.BasicTotalCost,                      // {8}  bcc_basictotalcost
                    TimeEntry.Overtime1TotalCost,                  // {9}  bcc_overtime1totalcost
                    TimeEntry.Overtime2TotalCost,                  // {10} bcc_overtime2totalcost
                    TimeEntry.Overtime3TotalCost,                  // {11} bcc_overtime3totalcost
                    TimeEntry.TotalPayrollCost,                    // {12} bcc_totalpayrollcost
                    TimeEntry.BookableResource,                    // {13} msdyn_bookableresource (filter + link)
                    cleanId,                                       // {14} resource GUID value
                    TimeEntry.Date,                                // {15} new_date
                    fromDateStr,                                   // {16}
                    toDateStr,                                     // {17}
                    BookableResource.EntityName,                   // {18} bookableresource
                    BookableResource.PrimaryKey,                   // {19} bookableresourceid
                    BookableResource.PrimaryName,                  // {20} name         → res.name
                    BookableResource.EmployeeID,                   // {21} bcc_userid   → res.bcc_userid
                    BookableResource.JoiningDate,                  // {22} bcc_joiningdate → res.bcc_joiningdate
                    BookableResource.MonthlyBasic,                 // {23} bcc_monthlybasic → res.bcc_monthlybasic
                    BookableResource.MonthlyAllowance             // {24} bcc_monthlyallowance → res.bcc_monthlyallowance,

                );

                trace.Trace("Executing FetchXML for resource {0}, period {1} → {2}.", cleanId, fromDateStr, toDateStr);
                var entries = service.RetrieveMultiple(new FetchExpression(fetchXml));
                trace.Trace("FetchXML returned {0} rows.", entries.Entities.Count);

                // ── Step 2: Extract resource metadata from the first row ──────────────
                //   If there are no time entries at all, fall back to a direct retrieve.
                string resourceName;
                string employeeId;
                DateTime? joiningDate;
                decimal basicSalaryRef;
                decimal allowanceRef;
                string Department = string.Empty;

                if (entries.Entities.Count > 0)
                {
                    var first = entries.Entities[0];
                    resourceName = GetAliasedString(first, "res." + BookableResource.PrimaryName);
                    employeeId = GetAliasedString(first, "res." + BookableResource.EmployeeID);
                    joiningDate = GetAliasedDateTime(first, "res." + BookableResource.JoiningDate);
                    basicSalaryRef = GetAliasedMoney(first, "res." + BookableResource.MonthlyBasic);
                    allowanceRef = GetAliasedMoney(first, "res." + BookableResource.MonthlyAllowance);
                    Department = first.Contains("des.bcc_designationid1")
                        ? first.GetAttributeValue<AliasedValue>("des.bcc_designationid1")?.Value.ToString()
                        : string.Empty;
                }
                else
                {
                    // No time entries — still retrieve the resource so we can create a zero payslip

                    resourceName = res.GetAttributeValue<string>(BookableResource.PrimaryName) ?? "Unknown";
                    employeeId = res.GetAttributeValue<string>(BookableResource.EmployeeID) ?? "";
                    joiningDate = res.Contains(BookableResource.JoiningDate)
                        ? (DateTime?)res.GetAttributeValue<DateTime>(BookableResource.JoiningDate) : null;
                    basicSalaryRef = res.GetAttributeValue<Money>(BookableResource.MonthlyBasic)?.Value ?? 0m;
                    allowanceRef = res.GetAttributeValue<Money>(BookableResource.MonthlyAllowance)?.Value ?? 0m;
                }

                trace.Trace("Resource: {0} | EmployeeId: {1}", resourceName, employeeId);

                // ── Step 3: Duplicate payslip check ──────────────────────────────────
                var dupQuery = new QueryExpression(Payslip.EntityName)
                {
                    ColumnSet = new ColumnSet(Payslip.PrimaryKey),
                    TopCount = 1
                };
                dupQuery.Criteria.AddCondition(Payslip.BookableResource, ConditionOperator.Equal, resourceGuid);
                dupQuery.Criteria.AddCondition(Payslip.PayPeriod, ConditionOperator.Equal, month);

                if (service.RetrieveMultiple(dupQuery).Entities.Count > 0)
                {
                    trace.Trace(string.Format("A payslip for '{0}' already exists for {1}.", resourceName, month));
                    return;
                }

                // ── Step 4: Aggregate attendance counts + sum cost fields ─────────────
                int presentDays = 0;
                int sickLeaveDays = 0;
                int annualLeaveDays = 0;
                int emergencyLeaveDays = 0;
                int unpaidAbsentDays = 0;
                decimal ot1Hours = 0m;
                decimal ot2Hours = 0m;
                decimal ot3Hours = 0m;
                decimal totalHoursSum = 0m;

                // Cost field sums — straight from pre-calculated time entry columns
                decimal sumBasicCost = 0m;
                decimal sumOT1Cost = 0m;
                decimal sumOT2Cost = 0m;
                decimal sumOT3Cost = 0m;
                decimal sumPayroll = 0m;

                presentDays = entries.Entities.Count(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Present);
                ot1Hours = entries.Entities.Where(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Present).Sum(te => te.GetAttributeValue<decimal>(TimeEntry.Overtime1));
                ot2Hours = entries.Entities.Where(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Present).Sum(te => te.GetAttributeValue<decimal>(TimeEntry.Overtime2));
                ot3Hours = entries.Entities.Where(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Present).Sum(te => te.GetAttributeValue<decimal>(TimeEntry.Overtime3OnlyonSundaysPublicHolidays));
                totalHoursSum = entries.Entities.Where(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Present).Sum(te => te.GetAttributeValue<decimal>(TimeEntry.Total_Hours));
                sickLeaveDays = entries.Entities.Count(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Absent
                    && te.GetAttributeValue<OptionSetValue>(TimeEntry.AbsentReason)?.Value == (int)TimeEntry.AbsentReason_OptionSet.SickLeave);
                annualLeaveDays = entries.Entities.Count(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Absent
                    && te.GetAttributeValue<OptionSetValue>(TimeEntry.AbsentReason)?.Value == (int)TimeEntry.AbsentReason_OptionSet.Vacationleave);
                emergencyLeaveDays = entries.Entities.Count(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Absent
                    && te.GetAttributeValue<OptionSetValue>(TimeEntry.AbsentReason)?.Value == (int)TimeEntry.AbsentReason_OptionSet.FieldEmergencyLeave);

                unpaidAbsentDays = entries.Entities.Count(te => te.GetAttributeValue<OptionSetValue>(TimeEntry.EntryType)?.Value == (int)TimeEntry.EntryType_OptionSet.Absent
                    && (te.GetAttributeValue<OptionSetValue>(TimeEntry.AbsentReason)?.Value == (int)TimeEntry.AbsentReason_OptionSet.Unpaid));

                sumBasicCost = entries.Entities.Sum(te => te.GetAttributeValue<Money>(TimeEntry.BasicTotalCost)?.Value ?? 0m);
                sumOT1Cost = entries.Entities.Sum(te => te.GetAttributeValue<Money>(TimeEntry.Overtime1TotalCost)?.Value ?? 0m);
                sumOT2Cost = entries.Entities.Sum(te => te.GetAttributeValue<Money>(TimeEntry.Overtime2TotalCost)?.Value ?? 0m);
                sumOT3Cost = entries.Entities.Sum(te => te.GetAttributeValue<Money>(TimeEntry.Overtime3TotalCost)?.Value ?? 0m);
                decimal grossSalaryRef = basicSalaryRef + allowanceRef;

                trace.Trace("Present={0}h | Sick={1} | Annual={2} | Emergency={3} | Unpaid={4}",
                    presentDays, sickLeaveDays, annualLeaveDays, emergencyLeaveDays, unpaidAbsentDays);
                trace.Trace("Costs — Basic={0} | OT1={1} | OT2={2} | OT3={3} | Total={4}",
                    sumBasicCost, sumOT1Cost, sumOT2Cost, sumOT3Cost, sumPayroll);
                // ── Step 4.1: Sum Deduction records for this resource & period ───────
                var deductionQuery = new QueryExpression(Deduction.EntityName)
                {
                    ColumnSet = new ColumnSet(Deduction.AdvanceDeduction, Deduction.HSEDeduction, Deduction.OtherDeduction)
                };
                deductionQuery.Criteria.AddCondition(Deduction.BookableResource, ConditionOperator.Equal, resourceGuid);
                deductionQuery.Criteria.AddCondition(Deduction.Date, ConditionOperator.OnOrAfter, fromDate);
                deductionQuery.Criteria.AddCondition(Deduction.Date, ConditionOperator.OnOrBefore, toDate);
                var deductionEntities = service.RetrieveMultiple(deductionQuery).Entities;
                decimal sumAdvanceDeduction = deductionEntities.Sum(d => d.GetAttributeValue<decimal?>(Deduction.AdvanceDeduction) ?? 0m);
                decimal sumHSEDeduction = deductionEntities.Sum(d => d.GetAttributeValue<decimal?>(Deduction.HSEDeduction) ?? 0m);
                decimal sumOtherDeduction = deductionEntities.Sum(d => d.GetAttributeValue<decimal?>(Deduction.OtherDeduction) ?? 0m);
                trace.Trace("Deductions — Advance={0} | HSE={1} | Other={2}", sumAdvanceDeduction, sumHSEDeduction, sumOtherDeduction);
                // ── Step 5: Create payslip ────────────────────────────────────────────
                string payslipName = string.Format("{0} - {1}", resourceName, month);
                var payslip = new Entity(Payslip.EntityName);
                payslip[Payslip.Trade] = Department;
                payslip[Payslip.PrimaryName] = payslipName;
                payslip[Payslip.PayPeriod] = month;
                payslip[Payslip.BookableResource] = new EntityReference(BookableResource.EntityName, resourceGuid);
                payslip[Payslip.IDNo] = employeeId;
                if (joiningDate.HasValue)
                    payslip[Payslip.DOJ] = joiningDate.Value;
                payslip[Payslip.Present] = (decimal)presentDays;
                payslip[Payslip.Absent] = (decimal)unpaidAbsentDays;
                payslip[Payslip.SickLeave] = (decimal)sickLeaveDays;
                payslip[Payslip.AnnualLeave] = (decimal)annualLeaveDays;
                payslip[Payslip.EmergencyLeave] = (decimal)emergencyLeaveDays;
                payslip[Payslip.OT1] = ot1Hours;
                payslip[Payslip.OT2] = ot2Hours;
                payslip[Payslip.OT3] = ot3Hours;
                payslip[Payslip.BasicSalary] = new Money(basicSalaryRef);
                payslip[Payslip.OtherAllowance] = new Money(allowanceRef);
                payslip[Payslip.GrossSalary] = new Money(grossSalaryRef);
                payslip[Payslip.EarnedSalary] = new Money(sumBasicCost);
                payslip[Payslip.EarnedOT1] = new Money(sumOT1Cost);
                payslip[Payslip.EarnedOT2] = new Money(sumOT2Cost);
                payslip[Payslip.EarnedOT3] = new Money(sumOT3Cost);
                payslip[Payslip.AdvanceDeduction] = sumAdvanceDeduction > 0 ? sumAdvanceDeduction * -1 : 0m;
                payslip[Payslip.HSEDeduction] = sumHSEDeduction > 0 ? sumHSEDeduction * -1 : 0m;
                payslip[Payslip.OtherDeduction] = sumOtherDeduction > 0 ? sumOtherDeduction * -1 : 0m;
                payslip.Id = service.Create(payslip);
                trace.Trace("Payslip created: {0}", payslip.Id);
                foreach (var te in entries.Entities)
                {
                    var update = new Entity(TimeEntry.EntityName, te.Id);
                    update[TimeEntry.Payslip] = payslip.ToEntityReference();
                    service.Update(update);
                    trace.Trace("Linked TimeEntry {0} to Payslip.", te.Id);
                }
            }
            else
            {
                // ── Non-Worker (Staff): create payslip from BookableResource details only ──
                string resourceName = res.GetAttributeValue<string>(BookableResource.PrimaryName) ?? "Unknown";
                string employeeId = res.GetAttributeValue<string>(BookableResource.EmployeeID) ?? "";
                DateTime? joiningDate = res.Contains(BookableResource.JoiningDate)
                    ? (DateTime?)res.GetAttributeValue<DateTime>(BookableResource.JoiningDate) : null;
                decimal basicSalaryRef = res.GetAttributeValue<Money>(BookableResource.MonthlyBasic)?.Value ?? 0m;
                decimal allowanceRef = res.GetAttributeValue<Money>(BookableResource.MonthlyAllowance)?.Value ?? 0m;
                decimal grossSalary = basicSalaryRef + allowanceRef;
                decimal conveyanceAllowanceRef = res.GetAttributeValue<Money>(BookableResource.ConveyanceAllowance)?.Value ?? 0m;
                decimal HRAAllowanceRef = res.GetAttributeValue<Money>(BookableResource.HRAAllowance)?.Value ?? 0m;
                string Department = res.Contains(BookableResource.DesignationID)
                    ? res.GetAttributeValue<EntityReference>(BookableResource.DesignationID)?.Name
                    : string.Empty;

                trace.Trace("Non-Worker resource: {0} | EmployeeId: {1}", resourceName, employeeId);

                // Duplicate check
                var dupQuery = new QueryExpression(Payslip.EntityName)
                {
                    ColumnSet = new ColumnSet(Payslip.PrimaryKey),
                    TopCount = 1
                };
                dupQuery.Criteria.AddCondition(Payslip.BookableResource, ConditionOperator.Equal, resourceGuid);
                dupQuery.Criteria.AddCondition(Payslip.PayPeriod, ConditionOperator.Equal, month);

                if (service.RetrieveMultiple(dupQuery).Entities.Count > 0)
                {
                    trace.Trace("A payslip for '{0}' already exists for {1} — skipping.", resourceName, month);
                    return;
                }

                DateTime fromDate         = DateTime.ParseExact(fromDateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
                DateTime toDate           = DateTime.ParseExact(toDateStr,   "yyyy-MM-dd", CultureInfo.InvariantCulture).Date;
                decimal  totalDaysInMonth = DateTime.DaysInMonth(fromDate.Year, fromDate.Month);

                // ── Query: Unpaid absent time entries in the date range ────────────
                var unpaidQuery = new QueryExpression(TimeEntry.EntityName)
                {
                    ColumnSet = new ColumnSet(TimeEntry.PrimaryKey)
                };
                unpaidQuery.Criteria.AddCondition(TimeEntry.BookableResource, ConditionOperator.Equal,       resourceGuid);
                unpaidQuery.Criteria.AddCondition(TimeEntry.EntryType,        ConditionOperator.Equal,       (int)TimeEntry.EntryType_OptionSet.Absent);
                unpaidQuery.Criteria.AddCondition(TimeEntry.AbsentReason,     ConditionOperator.Equal,       (int)TimeEntry.AbsentReason_OptionSet.Unpaid);
                unpaidQuery.Criteria.AddCondition(TimeEntry.Date,             ConditionOperator.OnOrAfter,   fromDate);
                unpaidQuery.Criteria.AddCondition(TimeEntry.Date,             ConditionOperator.OnOrBefore,  toDate);

                int unpaidAbsentDays = service.RetrieveMultiple(unpaidQuery).Entities.Count;
                trace.Trace("Staff unpaid absent days in period: {0}", unpaidAbsentDays);

                // ── Step 4.1: Sum Deduction records for this resource & period (staff) ─
                var deductionQuery = new QueryExpression(Deduction.EntityName)
                {
                    ColumnSet = new ColumnSet(Deduction.AdvanceDeduction, Deduction.HSEDeduction, Deduction.OtherDeduction)
                };
                deductionQuery.Criteria.AddCondition(Deduction.BookableResource, ConditionOperator.Equal, resourceGuid);
                deductionQuery.Criteria.AddCondition(Deduction.Date, ConditionOperator.OnOrAfter, fromDate);
                deductionQuery.Criteria.AddCondition(Deduction.Date, ConditionOperator.OnOrBefore, toDate);
                var deductionEntities = service.RetrieveMultiple(deductionQuery).Entities;
                decimal sumAdvanceDeduction = deductionEntities.Sum(d => d.GetAttributeValue<decimal?>(Deduction.AdvanceDeduction) ?? 0m);
                decimal sumHSEDeduction = deductionEntities.Sum(d => d.GetAttributeValue<decimal?>(Deduction.HSEDeduction) ?? 0m);
                decimal sumOtherDeduction = deductionEntities.Sum(d => d.GetAttributeValue<decimal?>(Deduction.OtherDeduction) ?? 0m);
                trace.Trace("Deductions (staff) — Advance={0} | HSE={1} | Other={2}", sumAdvanceDeduction, sumHSEDeduction, sumOtherDeduction);

                var payslip = new Entity(Payslip.EntityName);
                payslip[Payslip.PrimaryName]      = string.Format("{0} - {1}", resourceName, month);
                payslip[Payslip.PayPeriod]        = month;
                payslip[Payslip.BookableResource] = new EntityReference(BookableResource.EntityName, resourceGuid);
                payslip[Payslip.IDNo]             = employeeId;
                if (joiningDate.HasValue)
                    payslip[Payslip.DOJ]          = joiningDate.Value;
                payslip[Payslip.Present]          = totalDaysInMonth - unpaidAbsentDays;
                payslip[Payslip.DaysWorked]       = totalDaysInMonth - unpaidAbsentDays;
                payslip[Payslip.Absent]           = (decimal)unpaidAbsentDays;
                payslip[Payslip.BasicSalary]      = new Money(basicSalaryRef);
                payslip[Payslip.EarnedSalary]     = new Money(basicSalaryRef);
                payslip[Payslip.OtherAllowance]   = new Money(allowanceRef);
                payslip[Payslip.HRAAccommodation]   = HRAAllowanceRef;
                payslip[Payslip.ConveyanceAllowance] = conveyanceAllowanceRef;
                payslip[Payslip.GrossSalary]      = new Money(grossSalary);
                payslip[Payslip.Trade]            = Department;
                 payslip[Payslip.LeaveDeduction]            = unpaidAbsentDays > 0 ? ((basicSalaryRef + allowanceRef) / totalDaysInMonth * unpaidAbsentDays) * -1 : 0;
                payslip[Payslip.AdvanceDeduction] = sumAdvanceDeduction > 0 ? sumAdvanceDeduction * -1 : 0m;
                payslip[Payslip.HSEDeduction] = sumHSEDeduction > 0 ? sumHSEDeduction * -1 : 0m;
                payslip[Payslip.OtherDeduction] = sumOtherDeduction > 0 ? sumOtherDeduction * -1 : 0m;
                payslip.Id = service.Create(payslip);
                trace.Trace("Non-Worker payslip created: {0}", payslip.Id);
            }
        }

        // ── AliasedValue helpers ──────────────────────────────────────────────────

        private static string GetAliasedString(Entity e, string alias)
        {
            var av = e.GetAttributeValue<AliasedValue>(alias);
            return av?.Value as string ?? "";
        }

        private static DateTime? GetAliasedDateTime(Entity e, string alias)
        {
            var av = e.GetAttributeValue<AliasedValue>(alias);
            if (av?.Value is DateTime dt) return dt;
            return null;
        }

        private static decimal GetAliasedMoney(Entity e, string alias)
        {
            var av = e.GetAttributeValue<AliasedValue>(alias);
            return (av?.Value as Money)?.Value ?? 0m;
        }

        // ── Parameter helpers ─────────────────────────────────────────────────────

        private static string GetRequiredString(IPluginExecutionContext context, string key)
        {
            if (!context.InputParameters.Contains(key) || !(context.InputParameters[key] is string val))
                throw new InvalidPluginExecutionException(string.Format("Required parameter '{0}' is missing.", key));
            if (string.IsNullOrWhiteSpace(val))
                throw new InvalidPluginExecutionException(string.Format("Required parameter '{0}' is empty.", key));
            return val;
        }
    }
}
