// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\ManpowerRequest.cs
// Created    : 2026-07-05 10:55:54
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Manpower Request, OwnershipType: UserOwned, IntroducedVersion: 1.0.0.0</summary>
    public static class ManpowerRequest
    {
        public const string EntityName = "bcc_manpowerrequest";
        public const string EntityCollectionName = "bcc_manpowerrequests";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_manpowerrequestid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: account</summary>
        public const string Agency = "bcc_agency";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string CountofManpower = "bcc_countofmanpower";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: transactioncurrency</summary>
        public const string Currency = "transactioncurrencyid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string Date = "bcc_date";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bcc_designation</summary>
        public const string Designation = "bcc_designation";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: 0.0000000001, MaxValue: 100000000000, Precision: 12</summary>
        public const string ExchangeRate = "exchangerate";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string MonthlyAllowance = "bcc_monthlyallowance";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string MonthlyBasic = "bcc_monthlybasic";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string PositionFilled = "bcc_positionfilled";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_project</summary>
        public const string Project = "bcc_project";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string RejectionRemark = "bcc_rejectionremark";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string RequestNumber = "bcc_requestnumber";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Status, OptionSetType: Picklist, DefaultFormValue: 755700000</summary>
        public const string Status = "bcc_status";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status1 = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string TotalSalary = "bcc_totalsalary";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_workforceplanning</summary>
        public const string WorkforcePlanning = "bcc_workforceplanningid";

        #endregion Attributes

        #region OptionSets

        public enum Status_OptionSet
        {
            Draft = 755700000,
            Submitted = 755700001,
            PendingApproval = 755700002,
            Approve = 755700003,
            Reject = 755700004
        }
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

        #endregion OptionSets
    }
}
