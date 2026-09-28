using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCCGULF.Constants
{

    public static class JobApplication
    {
        public const string EntityName = "bcc_jobapplication";
        public const string EntityCollectionName = "bcc_jobapplications";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_jobapplicationid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: DateOnly</summary>
        public const string ExpiryDate = "bcc_ppexpirydate";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string ApplicationFrom = "bcc_applicationfrom";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_jobposting</summary>
        public const string JobPosting = "bcc_jobpostingid";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bcc_manpowerrequest</summary>
        public const string ManPowerRequest = "bcc_manpowerrequest";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bcc_designation</summary>
        public const string Position = "bcc_designation";
        /// <summary>Type: Money, RequiredLevel: ApplicationRequired, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 4</summary>
        public const string Basic = "bcc_basic";
         /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_workforceplanning</summary>
        public const string WorkforcePlanning = "bcc_workforceplanningid";
          /// <summary>Type: Money, RequiredLevel: ApplicationRequired, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 4</summary>
        public const string Allowance = "bcc_allowance";









        #endregion Attributes

        #region OptionSets

        public enum Status_OptionSet
        {
            Active = 0,
            Inactive = 1
        }
        public enum StatusReason_OptionSet
        {
            Active = 1,
            Inactive = 2
        }


        public enum ApplicatioFrom_OptionSet
        {
            InsideCountry = 755700000,
            OutsideCountry = 755700001
        }

        #endregion OptionSets
    }
}
