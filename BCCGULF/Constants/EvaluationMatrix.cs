// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\DOCS\Personal\BLCC\Constants\EvaluationMatrix.cs
// Created    : 2026-05-06 22:10:16
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Evaluation Matrix, OwnershipType: OrganizationOwned, IntroducedVersion: 1.0</summary>
    public static class EvaluationMatrix
    {
        public const string EntityName = "bcc_evaluationmatrix";
        public const string EntityCollectionName = "bcc_evaluationmatrixes";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_evaluationmatrixid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Applicable To, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string ApplicableTo = "bcc_applicableto";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Department, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Department = "bcc_department";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Division, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Division = "new_division";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: organization</summary>
        public const string OrganizationId = "organizationid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 4000, Format: TextArea</summary>
        public const string Remarks = "bcc_remarks";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1000, Format: Text</summary>
        public const string SerialNo = "bcc_description";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Integer, RequiredLevel: ApplicationRequired, MinValue: 1, MaxValue: 50</summary>
        public const string Weightage = "bcc_weightage";

        #endregion Attributes

        #region OptionSets

        public enum ApplicableTo_OptionSet
        {
            Worker = 100000000,
            Staff = 100000001
        }
        public enum Department_OptionSet
        {
            ACCOUNTS = 100000000,
            ADMIN = 100000011,
            ADMINDC = 100000003,
            CAMP = 100000016,
            CIVIL = 755700000,
            CONTRACTS = 100000013,
            COSTCONTROL = 100000014,
            DRIVER = 100000001,
            EMIRATI = 100000008,
            FINANCE = 100000015,
            HR = 100000010,
            HSE = 100000009,
            IT = 100000017,
            Item = 100000018,
            MEP = 755700001,
            PLANNING = 100000012,
            PROCUREMENTPURCHASE = 100000005,
            Q_SESTIMATION = 100000007,
            QA_QC = 100000004,
            STORETIMEKEEPING = 100000006,
            TECHNICAL = 100000002
        }
        public enum Division_OptionSet
        {
            ADMIN = 100000000,
            CIVIL = 100000001,
            HSE = 100000003,
            MEP = 100000002,
            OTHER = 100000004,
            OTHER_CORP = 100000005
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
