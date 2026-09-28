// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\DOCS\Personal\BLCC\Constants\EvaluationItem.cs
// Created    : 2026-05-08 08:44:38
// *********************************************************************
namespace BCCGULF.Workflows.Constants
{
    /// <summary>DisplayName: Evaluation Item, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class EvaluationItem
    {
        public const string EntityName = "bcc_evaluationitem";
        public const string EntityCollectionName = "bcc_evaluationitems";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_evaluationitemid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 0, MaxValue: 99</summary>
        public const string EmployeeInput = "bcc_employeeinput";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_evaluation</summary>
        public const string Evaluation = "bcc_evaluationid";
        /// <summary>Type: Integer, RequiredLevel: Recommended, MinValue: 0, MaxValue: 99</summary>
        public const string ManagerInput = "bcc_managerinput";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 4000, Format: TextArea</summary>
        public const string Remarks = "bcc_remarks";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string Weightage = "bcc_weightage";

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
