// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : C:\Users\DELL\Downloads\SubContractor.cs
// Created    : 2026-07-07 10:24:59
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Sub Contractor, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class SubContractor
    {
        public const string EntityName = "bcc_subcontractor";
        public const string EntityCollectionName = "bcc_subcontractors";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_subcontractorid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string ID = "bcc_id";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string WorkType = "bcc_worktype";

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
