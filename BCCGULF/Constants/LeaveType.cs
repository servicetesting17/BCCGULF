// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\LeaveType.cs
// Created    : 2026-04-04 15:50:31
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Leave Type, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class LeaveType
    {
        public const string EntityName = "bcc_leavetype";
        public const string EntityCollectionName = "bcc_leavetypes";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_leavetypeid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: Boolean, RequiredLevel: ApplicationRequired, True: 1, False: 0, DefaultValue: False</summary>
        public const string ApprovalRequired = "bcc_approvalrequired";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string AttachmentRequired = "bcc_attachmentrequired";
        /// <summary>Type: Virtual, RequiredLevel: None, DisplayName: Gender, OptionSetType: Picklist</summary>
        public const string GenderApplicableTo = "bcc_genderapplicableto";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 1, MaxValue: 60</summary>
        public const string LeaveDays = "bcc_leavedays";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Resource Type, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string RessourceType = "bcc_resourcetype";

        #endregion Attributes

        #region OptionSets

        public enum GenderApplicableTo_OptionSet
        {
            Male = 755700000,
            Female = 755700001
        }
        public enum StatusReason_OptionSet
        {
            Active = 1,
            Inactive = 2
        }

        #endregion OptionSets
    }
}
