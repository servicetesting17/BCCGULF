// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\DOCS\Personal\BLCC\Constants\Grade.cs
// Created    : 2026-05-06 22:10:16
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>OwnershipType: OrganizationOwned, IntroducedVersion: 1.0</summary>
    public static class Grade
    {
        public const string EntityName = "bcc_grade";
        public const string EntityCollectionName = "bcc_grades";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_gradeid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Grade, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Grade_ = "bcc_grade";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: organization</summary>
        public const string OrganizationId = "organizationid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Percentage = "bcc_percentage";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";

        #endregion Attributes

        #region OptionSets

        public enum Grade_OptionSet
        {
            _4_Excellent = 755700003,
            _3_Good = 755700002,
            _2_Average = 755700001,
            _1_Poor = 755700000
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
