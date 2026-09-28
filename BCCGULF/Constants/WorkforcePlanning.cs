// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\WorkforcePlanning.cs
// Created    : 2026-07-05 10:55:54
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Workforce Planning, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class WorkforcePlanning
    {
        public const string EntityName = "bcc_workforceplanning";
        public const string EntityCollectionName = "bcc_workforceplannings";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_workforceplanningid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string From = "bcc_from";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Hiring Need, OptionSetType: Picklist, DefaultFormValue: 755700000</summary>
        public const string HiringNeeds = "bcc_hiringneeds";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string To = "bcc_to";

        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string Project = "bcc_projectid";
 /// <summary>Type: String, RequiredLevel: None, MaxLength: 2000, Format: Text</summary>
        public const string Description = "bcc_description";


        #endregion Attributes

        #region OptionSets

        public enum HiringNeeds_OptionSet
        {
            NewRole = 755700000,
            BackFill = 755700001,
            Expansion = 755700002
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
