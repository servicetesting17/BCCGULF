// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\JobPosting.cs
// Created    : 2026-07-05 10:55:54
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Job Posting, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class JobPosting
    {
        public const string EntityName = "bcc_jobposting";
        public const string EntityCollectionName = "bcc_jobpostings";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_jobpostingid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: TextArea</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string Applicationdeadline = "bcc_applicationdeadline";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 100, Format: Text</summary>
        public const string Location = "bcc_location";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bcc_manpowerrequest</summary>
        public const string ManpowerRequest = "bcc_manpowerrequestid";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bcc_designation</summary>
        public const string Position = "bcc_position";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired, DisplayName: Publish Type, OptionSetType: Picklist</summary>
        public const string PublishType = "bcc_publishtype";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 100, Format: Text</summary>
        public const string Requirements = "bcc_requirements";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bcc_workforceplanning</summary>
        public const string WorkforcePlanning = "bcc_workforceplanningid";

        #endregion Attributes

        #region OptionSets

        public enum PublishType_OptionSet
        {
            PublishInternally = 755700000,
            CareerSites = 755700001,
            BCCcareerspage = 755700002
        }
        public enum Status_OptionSet
        {
            Active = 0,
            Inactive = 1
        }
        public enum StatusReason_OptionSet
        {
            Draft = 1,
            Inactive = 2,
            Published = 755700001
        }

        #endregion OptionSets
    }
}
