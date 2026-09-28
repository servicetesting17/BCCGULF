// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCDocs\Roster.cs
// Created    : 2026-04-08 21:37:04
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class Roster
    {
        public const string EntityName = "bcc_roster";
        public const string EntityCollectionName = "bcc_rosters";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_rosterid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string Date = "bcc_date";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string ID = "bcc_id";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Status, OptionSetType: Picklist, DefaultFormValue: 755700000</summary>
        public const string Status = "bcc_status";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status1 = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_timeentry</summary>
        public const string TimeEntry = "bcc_timeentryid";

        #endregion Attributes

        #region OptionSets

        public enum RosterStatus_OptionSet
        {
            Present = 755700000,
            Absent = 755700001
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
