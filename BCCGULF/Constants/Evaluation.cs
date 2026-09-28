// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\DOCS\Personal\BLCC\Constants\Evaluation.cs
// Created    : 2026-05-08 08:44:38
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class Evaluation
    {
        public const string EntityName = "bcc_evaluation";
        public const string EntityCollectionName = "bcc_evaluations";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_evaluationid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Months, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Month = "bcc_month";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_project</summary>
        public const string Project = "bcc_projectid";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Quarter, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Quater = "bcc_quater";/// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Grade, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Rating = "bcc_rating";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string TotalEmployeeInput = "bcc_totalemployeeinput";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string TotalEmployeeInputLastUpdatedOn = "bcc_totalemployeeinput_date";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string TotalEmployeeInputState = "bcc_totalemployeeinput_state";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string TotalMangerInput = "bcc_totalmangerinput";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string TotalMangerInputLastUpdatedOn = "bcc_totalmangerinput_date";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string TotalMangerInputState = "bcc_totalmangerinput_state";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string TotalWeitage = "bcc_totalweitage";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string TotalWeitageLastUpdatedOn = "bcc_totalweitage_date";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string TotalWeitageState = "bcc_totalweitage_state";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 4000, Format: Text</summary>
        public const string Year = "bcc_year";

        #endregion Attributes

        #region OptionSets
        public enum Quater_OptionSet
        {
            Q1 = 755700000,
            Q2 = 755700001,
            Q3 = 755700002,
            Q4 = 755700003
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
