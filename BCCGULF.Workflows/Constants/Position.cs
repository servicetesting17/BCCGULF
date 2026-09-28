// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\Position.cs
// Created    : 2026-07-05 10:55:54
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class Position
    {
        public const string EntityName = "bcc_designation";
        public const string EntityCollectionName = "bcc_designations";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_designationid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_designationid1";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_responsibility</summary>
        public const string Responsibility = "bcc_responsibilityid";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_role</summary>
        public const string Role = "bcc_roleid";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_skill</summary>
        public const string Skill = "bcc_skillid";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";

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

        #endregion OptionSets
    }
}
