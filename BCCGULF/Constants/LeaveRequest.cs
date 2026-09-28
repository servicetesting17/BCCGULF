// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\LeaveRequest.cs
// Created    : 2026-04-04 15:50:31
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Leave Request, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class LeaveRequest
    {
        public const string EntityName = "bcc_leaverequest";
        public const string EntityCollectionName = "bcc_leaverequests";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_leaverequestid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string EndTime = "bcc_endtime";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string ID = "bcc_id";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string LeaveDays = "bcc_leavedays";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bcc_leavetype</summary>
        public const string LeaveType = "bcc_leavetypeid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string StartTime = "bcc_starttime";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string PreviousLeaveDateFrom = "bcc_previousleavedatefrom";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string PreviousLeaveDateTo = "bcc_previousleavedateto";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string PreviousLeaveDays = "bcc_previousleavedays";

        #endregion Attributes

        #region OptionSets

        public enum Status_OptionSet
        {
            Active = 0,
            Inactive = 1
        }
        public enum StatusReason_OptionSet
        {
            PendingApproval = 1,
            HRApproved = 2,
            HRRejected = 755700001,
            Draft = 755700002,
            ManagerApproved = 755700003,
            ManagerRejected = 755700004
        }

        #endregion OptionSets
    }
}
