// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccgulf.crm15.dynamics.com/
// Filename   : C:\Users\DELL\Downloads\WebResources\Deduction.cs
// Created    : 2026-08-10 20:22:11
// *********************************************************************
namespace BCCGULF.Constants 
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class Deduction
    {
        public const string EntityName = "bcc_deduction";
        public const string EntityCollectionName = "bcc_deductions";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_deductionid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string AdvanceDeduction = "bcc_advancededuction";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string Days = "bcc_days";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1000, Format: Text</summary>
        public const string AdvanceDeductionRemark = "bcc_advancedeductionremark";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string Date = "bcc_date";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string HSEDeduction = "bcc_hsededuction";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1000, Format: Text</summary>
        public const string HSEDeductionRemark = "bcc_hsedeductionremark";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string OtherDeduction = "bcc_otherdeduction";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1000, Format: Text</summary>
        public const string OtherDeductionRemark = "bcc_otherdeductionremark";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Deduction Type, OptionSetType: Status</summary>
        public const string DeductionType = "bcc_deductiontype";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string TotalDeduction = "bcc_totaldeduction";

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

        public enum DeductionType_OptionSet
        {
            Amount = 755700000,
            Day = 755700001
        }

        #endregion OptionSets
    }
}
