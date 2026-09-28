// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\Appointment.cs
// Created    : 2026-07-05 10:55:54
// *********************************************************************
namespace BCCGULF.Workflows.Constants
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 5.0.0.0</summary>
    public static class BioMarticLocations
    {
        public const string EntityName = "new_biometriclocation";
        public const string EntityCollectionName = "new_biometriclocations";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "new_biometriclocationid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 200, Format: Text</summary>
        public const string PrimaryName = "new_name";
        /// <summary>Type: Lookup, RequiredLevel: None, Target: new_project</summary>
          public const string Project = "new_project";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Mid1 = "new_id";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Mid1Location = "new_location";
    
        #endregion     
    }
}
