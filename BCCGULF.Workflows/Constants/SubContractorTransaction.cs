// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : C:\Users\DELL\Downloads\SubContractorTransaction.cs
// Created    : 2026-07-07 10:24:59
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Sub Contractor Transaction, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class SubContractorTransaction
    {
        public const string EntityName = "bcc_subcontractortransaction";
        public const string EntityCollectionName = "bcc_subcontractortransactions";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_subcontractortransactionid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Integer, RequiredLevel: ApplicationRequired, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string Count = "bcc_count";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string Date = "bcc_date";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_project</summary>
        public const string Project = "bcc_Project";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_subcontractor</summary>
        public const string SubContractor = "bcc_subcontractorid";

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
