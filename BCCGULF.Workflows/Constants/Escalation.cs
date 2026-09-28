// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\Escalation.cs
// Created    : 2026-05-26 14:23:53
// *********************************************************************
namespace BCCGULF.Workflows.Constants
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class Escalation
    {
        public const string EntityName = "bcc_escalation";
        public const string EntityCollectionName = "bcc_escalations";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_escalationid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string DateOfJoining = "bcc_dateofjoining";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Department, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Department = "bcc_department";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string EmployeeCode = "bcc_employeecode";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: TextArea</summary>
        public const string EmployeeComments = "bcc_employeecomments";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_evaluation</summary>
        public const string Evaluation = "bcc_evaluationid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string ID = "bcc_id";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: TextArea</summary>
        public const string ManagerComments = "bcc_managercomments";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Months, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Month = "bcc_month";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Quarter, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Quater = "bcc_quater";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Grade, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Rating = "bcc_rating";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";

        #endregion Attributes

        #region OptionSets

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
        public enum Month_OptionSet
        {
            Jan = 755700000,
            Feb = 755700001,
            Mar = 755700002,
            Apr = 755700003,
            May = 755700004,
            Jun = 755700005,
            Jul = 755700006,
            Aug = 755700007,
            Sep = 755700008,
            Oct = 755700009,
            Nov = 755700010,
            Dev = 755700011
        }
        public enum Quater_OptionSet
        {
            Q1 = 755700000,
            Q2 = 755700001,
            Q3 = 755700002,
            Q4 = 755700003
        }
        public enum Rating_OptionSet
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
