// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\TimeEntry.cs
// Created    : 2026-04-04 15:46:57
// *********************************************************************
namespace BCCGULF.Workflows.Constants
{
    /// <summary>DisplayName: Time Entry, OwnershipType: UserOwned, IntroducedVersion: 1.0.0.0</summary>
    public static class TimeEntry
    {
        public const string EntityName = "msdyn_timeentry";
        public const string EntityCollectionName = "msdyn_timeentries";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "msdyn_timeentryid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string PrimaryName = "msdyn_description";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalPayrollCost = "bcc_totalpayrollcost";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string DeprecatedStageId = "stageid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1250, Format: Text</summary>
        public const string DeprecatedTraversedPath = "traversedpath";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Absent Reason, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string AbsentReason = "bcc_absentreason";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: systemuser</summary>
        public const string Approver = "bcc_approver";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string BasicTotalCost = "bcc_basictotalcost";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: new_biometric</summary>
        public const string Biometric = "bcc_biometric";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresource</summary>
        public const string BookableResource = "msdyn_bookableresource";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookableresourcebooking</summary>
        public const string BookableResourceBooking = "msdyn_bookableresourcebooking";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bookingstatus</summary>
        public const string BookingStatus = "msdyn_bookingstatus";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: transactioncurrency</summary>
        public const string Currency = "transactioncurrencyid";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string Date = "new_date";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 30, Format: Text</summary>
        public const string DayofDate = "bcc_dayofdate";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string days = "bcc_days";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 0, MaxValue: 2147483647</summary>
        public const string Duration = "msdyn_duration";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string Duration1 = "new_duration";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string End = "msdyn_end";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Time Entry Status, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string EntryStatus = "msdyn_entrystatus";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Entry Type, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string EntryType = "new_entrytype";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: 0.0000000001, MaxValue: 100000000000, Precision: 12</summary>
        public const string ExchangeRate = "exchangerate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string ExternalComments = "msdyn_externaldescription";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string HolidayEndDate = "bcc_holidayenddate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string HolidayStartDate = "bcc_holidaystartdate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string InactiveDays = "bcc_inactivedays";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 1048576</summary>
        public const string InternalFlags = "msdyn_internalflags";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsHoliday = "bcc_isholiday";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsProcessed = "bcc_isprocessed";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string newDuration = "bcc_newduration";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Overtime1TotalCost = "bcc_overtime1totalcost";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Overtime2TotalCost = "bcc_overtime2totalcost";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: 0, MaxValue: 16, Precision: 2</summary>
        public const string Overtime3OnlyonSundaysPublicHolidays = "new_overtime3";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Overtime3TotalCost = "bcc_overtime3totalcost";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: 0, MaxValue: 12, Precision: 2</summary>
        public const string Overtime1 = "new_overtime1";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: 0, MaxValue: 12, Precision: 2</summary>
        public const string Overtime2 = "new_overtime2";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string ProcessId = "processid";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_project</summary>
        public const string Project = "bcc_project";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string RejectReason = "bcc_rejectreason";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string Start = "msdyn_start";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Task, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Task = "new_activity";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: msdyn_servicetasktype</summary>
        public const string Task1 = "new_task";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_timeoffrequest</summary>
        public const string TimeOffRequest = "msdyn_timeoffrequest";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_timeentrysetting</summary>
        public const string TimeSource = "msdyn_timeentrysettingid";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: 0, MaxValue: 24, Precision: 2</summary>
        public const string Total_Hours = "bcc_total_hours";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: msdyn_timeentrytype, OptionSetType: Picklist, DefaultFormValue: 192350000</summary>
        public const string Type = "msdyn_type";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string Upload = "bcc_upload";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string weekdays = "bcc_weekdays";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_workorder</summary>
        public const string WorkOrder = "msdyn_workorder";

          /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_payslip</summary>
        public const string Payslip = "bcc_payslipid";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_roster</summary>
        public const string Roster = "bcc_rosterid";

        #endregion Attributes

        #region OptionSets

        public enum AbsentReason_OptionSet
        {
            SickLeave = 755700000,
            Vacationleave = 755700001,
            Unpaid = 755700002,
            FieldEmergencyLeave = 755700003
        }
        public enum EntryStatus_OptionSet
        {
            Draft = 192350000,
            Rejected = 192350001,
            Approved = 192350002,
            Submitted = 192350003,
            Cancelled = 192354320,
            PendingForApproval = 192350004
        }
        public enum EntryType_OptionSet
        {
            Present = 100000000,
            Absent = 100000001
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
        public enum Task_OptionSet
        {
            BlockFixing_External = 100000014,
            BlockFixing_Internal = 100000013,
            Concreting = 100000010,
            Demolitionworks = 100000028,
            Deshuttering = 100000012,
            ExternalPainting = 100000020,
            Flagman = 100000026,
            GeneralActivity = 100000000,
            GradeSlab = 100000006,
            Housekeepingworks = 100000024,
            InternalPainting = 100000019,
            LevellingCompaction = 100000003,
            Manualexcavation = 100000002,
            Marblelaying = 100000018,
            MaterialShifting = 100000025,
            Paverlaying = 100000023,
            PCC = 100000005,
            Pilebreaking = 100000029,
            Plastering_External = 100000016,
            Plastering_Internal = 100000015,
            RebarBendingCutting = 100000008,
            RebarFixing = 100000009,
            Roadbasecompaction = 100000004,
            Safetyteam = 100000027,
            Scaffolding_External = 100000022,
            Scaffolding_Internal = 100000021,
            Shuttering = 100000011,
            SoilBackfilling_levelling = 100000001,
            TileLayingskirting = 100000017,
            WParearepair_preparationforWP = 100000007
        }
        public enum Type_OptionSet
        {
            Overtime = 192354320,
            Work = 192350000,
            Absence = 192350001,
            Vacation = 192350002,
            OnBreak = 192355000,
            Travel = 192355001
        }

        #endregion OptionSets
    }
}
