// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccgulf.crm15.dynamics.com/
// Filename   : D:\DOCS\Personal\BLCC\Constants\ProjectedLeaves.cs
// Created    : 2026-10-13 10:58:17
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Projected Leaves, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class ProjectedLeaves
    {
        public const string EntityName = "bcc_projectedleaves";
        public const string EntityCollectionName = "bcc_projectedleaveses";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_projectedleavesid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 1, MaxValue: 12</summary>
        public const string Month = "bcc_month";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Month, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string MonthOfYear = "bcc_monthofyear";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 2026, MaxValue: 9999</summary>
        public const string Year = "bcc_year";

        #endregion Attributes

        #region OptionSets

        public enum Month_OptionSet
        {
            January = 1,
            February = 2,
            March = 3,
            April = 4,
            May = 5,
            June = 6,
            July = 7,
            August = 8,
            September = 9,
            October = 10,
            November = 11,
            December = 12
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
