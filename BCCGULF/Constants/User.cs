// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\User.cs
// Created    : 2026-04-04 15:50:31
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>OwnershipType: BusinessOwned, IntroducedVersion: 5.0.0.0</summary>
    public static class User
    {
        public const string EntityName = "systemuser";
        public const string EntityCollectionName = "systemusers";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "systemuserid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 200, Format: Text</summary>
        public const string PrimaryName = "fullname";
        /// <summary>Type: Picklist, RequiredLevel: SystemRequired, DisplayName: Access Mode, OptionSetType: Picklist, DefaultFormValue: 0</summary>
        public const string AccessMode = "accessmode";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string ApplicationID = "applicationid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1024, Format: Text</summary>
        public const string ApplicationIDURI = "applicationiduri";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string AzureADObjectID = "azureactivedirectoryobjectid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string BotapplicationID = "msdyn_botapplicationid";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Bot Provider, OptionSetType: Picklist, DefaultFormValue: 192350001</summary>
        public const string BotProvider = "msdyn_botprovider";
        /// <summary>Type: Lookup, RequiredLevel: SystemRequired, Targets: businessunit</summary>
        public const string BusinessUnit = "businessunitid";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 0, MaxValue: 2147483647</summary>
        public const string Capacity = "msdyn_capacity";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_presence</summary>
        public const string DefaultPresence = "msdyn_defaultpresenceiduser";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: queue</summary>
        public const string DefaultQueue = "queueid";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 2000</summary>
        public const string Description = "msdyn_botdescription";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Email</summary>
        public const string Email2 = "personalemailaddress";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 256, Format: Text</summary>
        public const string FirstName = "firstname";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 50, Format: Text</summary>
        public const string HomePhone = "homephone";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Invitation Status, OptionSetType: Picklist, DefaultFormValue: 0</summary>
        public const string InvitationStatus = "invitestatuscode";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 256, Format: Text</summary>
        public const string LastName = "lastname";
        /// <summary>Type: Picklist, RequiredLevel: SystemRequired, DisplayName: CAL Type, OptionSetType: Picklist, DefaultFormValue: 0</summary>
        public const string LicenseType = "caltype";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: mailbox</summary>
        public const string Mailbox = "defaultmailbox";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: systemuser</summary>
        public const string Manager = "parentsystemuserid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Email</summary>
        public const string MobileAlertEmail = "mobilealertemail";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: mobileofflineprofile</summary>
        public const string MobileOfflineProfile = "mobileofflineprofileid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 64, Format: Text</summary>
        public const string MobilePhone = "mobilephone";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 50, Format: Text</summary>
        public const string Nickname = "nickname";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: position</summary>
        public const string Position = "positionid";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Preferred Address, OptionSetType: Picklist, DefaultFormValue: 1</summary>
        public const string PreferredAddress = "preferredaddresscode";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Preferred Phone, OptionSetType: Picklist, DefaultFormValue: 1</summary>
        public const string PreferredPhone = "preferredphonecode";
        /// <summary>Type: String, RequiredLevel: SystemRequired, MaxLength: 100, Format: Email</summary>
        public const string PrimaryEmail = "internalemailaddress";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: site</summary>
        public const string Site = "siteid";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string Status = "isdisabled";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: territory</summary>
        public const string Territory = "territoryid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 128, Format: Text</summary>
        public const string Title = "title";
        /// <summary>Type: String, RequiredLevel: SystemRequired, MaxLength: 1024, Format: Text</summary>
        public const string UserName = "domainname";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: User type, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Usertype = "msdyn_agentType";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1024, Format: Email</summary>
        public const string WindowsLiveID = "windowsliveid";

        #endregion Attributes

        #region OptionSets

        public enum AccessMode_OptionSet
        {
            Read_Write = 0,
            Administrative = 1,
            Read = 2,
            SupportUser = 3,
            Non_interactive = 4,
            DelegatedAdmin = 5
        }
        public enum BotProvider_OptionSet
        {
            VirtualAgent = 192350000,
            Other = 192350001,
            None = 192350002
        }
        public enum InvitationStatus_OptionSet
        {
            InvitationNotSent = 0,
            Invited = 1,
            InvitationNearExpired = 2,
            InvitationExpired = 3,
            InvitationAccepted = 4,
            InvitationRejected = 5,
            InvitationRevoked = 6
        }
        public enum LicenseType_OptionSet
        {
            Professional = 0,
            Administrative = 1,
            Basic = 2,
            DeviceProfessional = 3,
            DeviceBasic = 4,
            Essential = 5,
            DeviceEssential = 6,
            Enterprise = 7,
            DeviceEnterprise = 8,
            Sales = 9,
            Service = 10,
            FieldService = 11,
            ProjectService = 12
        }
        public enum PreferredAddress_OptionSet
        {
            MailingAddress = 1,
            OtherAddress = 2
        }
        public enum PreferredPhone_OptionSet
        {
            MainPhone = 1,
            OtherPhone = 2,
            HomePhone = 3,
            MobilePhone = 4
        }
        public enum Usertype_OptionSet
        {
            Applicationuser = 192350000,
            Botapplicationuser = 192350001
        }

        #endregion OptionSets
    }
}
