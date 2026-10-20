using BCCGULF.Constants;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;

namespace BCCGULF.BatchJobs.Jobs
{
    /// <summary>
    /// Creates Projected Leaves for existing Bookable Resources that were created before the
    /// GenerateLeaveBalance plugin started generating them.
    ///
    /// Eligible resources: active, Type and Joining Date set, an active Annual Leave Balance,
    /// and no Projected Leaves yet (so the job can be re-run safely).
    ///
    /// Projection window: the current leave year — the 12-month cycle based on the joining
    /// anniversary that contains the current month. The cycle start follows the plugin rule:
    /// joined on or before the 15th → joining month, otherwise the following month.
    /// </summary>
    public class ProjectedLeavesBackfillJob
    {
        private readonly IOrganizationService _service;
        private readonly ITracingService _tracingService;
        private readonly bool _commit;

        public ProjectedLeavesBackfillJob(IOrganizationService service, ITracingService tracingService, bool commit)
        {
            _service = service;
            _tracingService = tracingService;
            _commit = commit;
        }

        public void Run()
        {
            var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var resourcesWithAnnualLeave = GetResourcesWithAnnualLeaveBalance();
            var resourcesWithProjections = GetResourcesWithProjectedLeaves();
            var resources = GetActiveBookableResources();

            Console.WriteLine("Active Bookable Resources: {0}", resources.Count);
            Console.WriteLine("Resources with an Annual Leave Balance: {0}", resourcesWithAnnualLeave.Count);
            Console.WriteLine("Resources that already have Projected Leaves: {0}", resourcesWithProjections.Count);
            Console.WriteLine();

            int processed = 0, skipped = 0, failed = 0, recordsCreated = 0;

            foreach (var resource in resources)
            {
                string name = resource.GetAttributeValue<string>(BookableResource.PrimaryName) ?? resource.Id.ToString();

                if (!resourcesWithAnnualLeave.Contains(resource.Id))
                {
                    _tracingService.Trace("Skip {0}: no active Annual Leave Balance.", name);
                    skipped++;
                    continue;
                }

                if (resourcesWithProjections.Contains(resource.Id))
                {
                    _tracingService.Trace("Skip {0}: Projected Leaves already exist.", name);
                    skipped++;
                    continue;
                }

                var type = resource.GetAttributeValue<OptionSetValue>(BookableResource.Type);
                DateTime? joiningDate = GetJoiningDate(resource);

                if (type == null || !joiningDate.HasValue)
                {
                    Console.WriteLine("SKIP  {0}: {1} is not set.", name, type == null ? "Type" : "Joining Date");
                    skipped++;
                    continue;
                }

                decimal monthlyDays = Common.Common.GetMonthlyAnnualLeaveDays(type.Value);
                DateTime startMonth = GetCurrentLeaveYearStart(joiningDate.Value, thisMonth);

                Console.WriteLine("{0}  {1}: joined {2:yyyy-MM-dd}, {3} day(s)/month, {4:yyyy-MM} to {5:yyyy-MM}",
                    _commit ? "CREATE" : "PLAN  ", name, joiningDate.Value, monthlyDays, startMonth, startMonth.AddMonths(11));

                if (!_commit)
                {
                    processed++;
                    continue;
                }

                try
                {
                    var resourceRef = new EntityReference(BookableResource.EntityName, resource.Id) { Name = name };
                    recordsCreated += Common.Common.CreateProjectedLeaves(_service, resourceRef, startMonth, monthlyDays, _tracingService);
                    processed++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("ERROR {0}: {1}", name, ex.Message);
                    failed++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("{0}: {1} resource(s), skipped: {2}, failed: {3}{4}",
                _commit ? "Processed" : "Would process", processed, skipped, failed,
                _commit ? ", Projected Leaves records created: " + recordsCreated : string.Empty);

            if (!_commit)
                Console.WriteLine("Dry run only — re-run with --commit to create the records.");
        }

        /// <summary>
        /// Start of the 12-month leave cycle (anchored on the joining date) that contains thisMonth.
        /// Resources still before their first cycle get their first cycle.
        /// </summary>
        private static DateTime GetCurrentLeaveYearStart(DateTime joiningDate, DateTime thisMonth)
        {
            var startMonth = Common.Common.GetProjectionStartMonth(joiningDate);

            while (startMonth.AddMonths(12) <= thisMonth)
                startMonth = startMonth.AddMonths(12);

            return startMonth;
        }

        private static DateTime? GetJoiningDate(Entity resource)
        {
            // Same fields and timezone adjustment as GenerateLeaveBalance
            if (resource.Contains(BookableResource.JoiningDate) && resource[BookableResource.JoiningDate] != null)
                return resource.GetAttributeValue<DateTime>(BookableResource.JoiningDate).AddHours(6);

            if (resource.Contains(BookableResource.DateofJoining) && resource[BookableResource.DateofJoining] != null)
                return resource.GetAttributeValue<DateTime>(BookableResource.DateofJoining).AddHours(6);

            return null;
        }

        private List<Entity> GetActiveBookableResources()
        {
            var query = new QueryExpression(BookableResource.EntityName)
            {
                ColumnSet = new ColumnSet(BookableResource.PrimaryName, BookableResource.Type,
                    BookableResource.JoiningDate, BookableResource.DateofJoining)
            };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);
            query.AddOrder(BookableResource.PrimaryName, OrderType.Ascending);

            return RetrieveAll(query);
        }

        private HashSet<Guid> GetResourcesWithAnnualLeaveBalance()
        {
            var query = new QueryExpression(LeaveBalance.EntityName)
            {
                ColumnSet = new ColumnSet(LeaveBalance.BookableResource)
            };
            query.Criteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            var leaveType = query.AddLink(LeaveType.EntityName, LeaveBalance.LeaveType, LeaveType.PrimaryKey);
            leaveType.LinkCriteria.AddCondition(LeaveType.PrimaryName, ConditionOperator.Like, "%Annual Leave%");
            leaveType.LinkCriteria.AddCondition("statecode", ConditionOperator.Equal, 0);

            return GetResourceIds(RetrieveAll(query), LeaveBalance.BookableResource);
        }

        private HashSet<Guid> GetResourcesWithProjectedLeaves()
        {
            var query = new QueryExpression(ProjectedLeaves.EntityName)
            {
                ColumnSet = new ColumnSet(ProjectedLeaves.BookableResource)
            };

            return GetResourceIds(RetrieveAll(query), ProjectedLeaves.BookableResource);
        }

        private static HashSet<Guid> GetResourceIds(IEnumerable<Entity> records, string lookupField)
        {
            var ids = new HashSet<Guid>();
            foreach (var record in records)
            {
                var resourceRef = record.GetAttributeValue<EntityReference>(lookupField);
                if (resourceRef != null)
                    ids.Add(resourceRef.Id);
            }
            return ids;
        }

        private List<Entity> RetrieveAll(QueryExpression query)
        {
            var results = new List<Entity>();
            query.PageInfo = new PagingInfo { Count = 5000, PageNumber = 1 };

            while (true)
            {
                var page = _service.RetrieveMultiple(query);
                results.AddRange(page.Entities);

                if (!page.MoreRecords)
                    break;

                query.PageInfo.PageNumber++;
                query.PageInfo.PagingCookie = page.PagingCookie;
            }

            return results;
        }
    }
}
