// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\Attendence.cs
// Created    : 2026-04-04 15:50:31
// *********************************************************************
namespace BCCGULF.Workflows.Constants
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class Attendence
    {
        public const string EntityName = "new_biometric";
        public const string EntityCollectionName = "new_biometrics";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "new_biometricid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string PrimaryName = "new_name";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: DateOnly</summary>
        public const string Date = "new_date";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string Employee_ID = "new_employee_id";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 21, Format: Text</summary>
        public const string InTime = "bcc_intime";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Mid1 = "bcc_mid1";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Mid1Location = "bcc_mid1location";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsProcessed = "bcc_isprocessed";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_project</summary>
        public const string MachineNameIN = "bcc_machinenamein";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_project</summary>
        public const string MachineNameOUT = "bcc_machinenameout";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 20, Format: Text</summary>
        public const string OutTime = "bcc_outtime";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 21, Format: Text</summary>
        public const string Overtime = "bcc_overtime";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 18, Format: Text</summary>
        public const string TotalWorkingHours = "bcc_totalworkinghours";
          /// <summary>Type: decimal, RequiredLevel: None, MaxLength: 18, Format: decimal</summary>
        public const string TotalWorkingHoursDec = "bcc_totalworkinghoursdecimal";
          /// <summary>Type: decimal, RequiredLevel: None, MaxLength: 18, Format: decimal</summary>
        public const string OvertimeHours = "bcc_overtimehours";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string User_ID = "new_user_id";

        /// <summary>Type: choice, RequiredLevel: None</summary>
        public const string UserIsLate = "bcc_latecheckin";

        /// <summary>Type: choice, RequiredLevel: None</summary>
        public const string UserEarlyCheckout = "bcc_earlycheckout";  
        
          /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_project </summary>
        public const string Project = "bcc_projectid";


        #endregion Attributes

        #region OptionSets

        public enum Status_OptionSet
        {
            Active = 0,
            Inactive = 1
        }

        public enum CheckInStatus_OptionSet
        {
            NO = 755700000,
            Yes = 755700001
        }


        public enum StatusReason_OptionSet
        {
            Active = 1,
            Inactive = 2
        }

        #endregion OptionSets
    }
}
