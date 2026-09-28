// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\LeaveBalance.cs
// Created    : 2026-04-04 15:50:31
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Leave Balance, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class LeaveBalance
    {
        public const string EntityName = "bcc_leavebalance";
        public const string EntityCollectionName = "bcc_leavebalances";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_leavebalanceid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string AvailableLeaves = "bcc_availableleaves";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "bcc_bookableresourceid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string ID = "bcc_id";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_leavetype</summary>
        public const string LeaveType = "bcc_leavetypeid";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string TotalLeaves = "bcc_totalleaves";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string UsedLeaves = "bcc_usedleaves";

        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>

        public const string LeaveBal = "bcc_leavebalance";

        #endregion Attributes

        #region OptionSets

        public enum StatusReason_OptionSet
        {
            Active = 1,
            Inactive = 2
        }

        #endregion OptionSets
    }
}
