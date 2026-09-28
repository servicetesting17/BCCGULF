// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccgulf.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\Shift.cs
// Created    : 2026-06-15 12:41:49
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class Shift
    {
        public const string EntityName = "bcc_shift";
        public const string EntityCollectionName = "bcc_shifts";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_shiftid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string Shifttimefrom = "bcc_shifttimefrom";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string Shifttimeto = "bcc_shifttimeto";
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
