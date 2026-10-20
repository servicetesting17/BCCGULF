// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\DOCS\Personal\BLCC\files\BookableResource.cs
// Created    : 2026-04-03 18:17:28
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Bookable Resource, OwnershipType: UserOwned, IntroducedVersion: 8.1.0.0</summary>
    public static class BookableResource
    {
        public const string EntityName = "bookableresource";
        public const string EntityCollectionName = "bookableresources";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bookableresourceid";
        /// <summary>Type: String, RequiredLevel: SystemRequired, MaxLength: 100, Format: Text</summary>
        public const string PrimaryName = "name";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 1, MaxValue: 50</summary>
        public const string deprecatedBookingsToDrip = "msdyn_bookingstodrip";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: account</summary>
        public const string Account = "accountid";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string BasicHourlyRate = "bcc_basichourlyrate";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: calendar</summary>
        public const string Calendar = "calendarid";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_camp</summary>
        public const string Camp = "bcc_camp";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: contact</summary>
        public const string Contact = "contactid";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Crew Strategy, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string CrewStrategy = "msdyn_crewstrategy";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string DateofBirth = "bcc_dateofbirth";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string DateOfDeactive = "bcc_dateofdeactive";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string DateofJoining = "new_dateofjoining";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string DeactivationRemark = "bcc_deactivationremark";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string DegreeEndDate = "bcc_degreeenddate";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string DegreeStartDate = "bcc_degreestartdate_";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string DegreeType = "bcc_degreetype";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Department, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Department = "bcc_department";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string DeriveCapacityFromGroupMembers = "msdyn_derivecapacity";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_manpowerquota</summary>
        public const string DesignationAsPerVisa = "bcc_designationaspervisa_";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_designation</summary>
        public const string DesignationID = "bcc_designation_id";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_designation</summary>
        public const string Designationincompany = "bcc_designationincompanycc";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: True</summary>
        public const string DisplayOnScheduleBoard = "msdyn_displayonscheduleboard";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Division, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Division = "new_division";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string EmiratesID = "bcc_emiratesid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string EmployeeID = "bcc_userid";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string EnableDripScheduling = "msdyn_enabledripscheduling";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: True</summary>
        public const string EnableforAvailabilitySearch = "msdyn_displayonscheduleassistant";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string EnableforFieldServiceMobilelegacyXamarinapp = "msdyn_enabledforfieldservicemobile";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Work Start Location Type, OptionSetType: Picklist, DefaultFormValue: 690970002</summary>
        public const string EndLocation = "msdyn_endlocation";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Faculty = "bcc_faculty";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string FromDate = "bcc_fromdate";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Gender, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Gender = "bcc_gender";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Generic Type, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string GenericTypeDeprecated = "msdyn_generictype";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string GraduationYear = "bcc_graduationyear";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: 0, MaxValue: 1000000000, Precision: 2</summary>
        public const string HourlyRate = "msdyn_hourlyrate";
        /// <summary>Type: Money, RequiredLevel: None, DisplayName: HRA Allowance, </summary>
        public const string HRAAllowance = "HRAAllowance";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Enable appointments option set, OptionSetType: Picklist, DefaultFormValue: 192350001</summary>
        public const string IncludeAppointments = "msdyn_enableappointments";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Enable appointments option set, OptionSetType: Picklist, DefaultFormValue: 192350001</summary>
        public const string IncludeOutlookFree_BusyinSearchResourceAvailabilityAPI = "msdyn_enableoutlookschedules";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string JoiningDate = "new_dateofjoining";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string LabourCardExpiry = "bcc_labourcardexpiry";
        /// <summary>Type: DateTime, RequiredLevel: None</summary>
        public const string LeaveEndDate = "bcc_leaveenddate";
        /// <summary>Type: DateTime, RequiredLevel: None</summary>
        public const string LeaveStartDate = "bcc_leavestartdate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string LabourCardNo = "bcc_labourcardno";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: systemuser</summary>
        public const string Manager = "bcc_managerid";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Medical Insurance, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string MedicalInsurance = "bcc_medicalinsurance";
        /// <summary>Type: Money, RequiredLevel: None, DisplayName: Conveyance Allowance</summary>
        public const string ConveyanceAllowance = "new_conveyanceallowance";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string MOLNumber = "bcc_molnumber";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string MonthlyAllowance = "bcc_monthlyallowance";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string MonthlyBasic = "bcc_monthlybasic";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_region</summary>
        public const string Nationality = "bcc_nationality";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Nationality1 = "bcc_nationality_";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Notice Period, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string NoticePeriod = "bcc_noticeperiod";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 0, MaxValue: 9999</summary>
        public const string OptimalCrewSize = "msdyn_optimalcrewsize";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_organizationalunit</summary>
        public const string OrganizationalUnit = "msdyn_organizationalunit";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string OT1HourlyRate = "bcc_ot1hourlyrate";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string OT2HourlyRate = "bcc_ot2hourlyrate";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string OT3HourlyRate = "bcc_ot3hourlyrate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string PassportNo = "bcc_passportno";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_payrollconfiguration</summary>
        public const string PayrollConfiguration = "bcc_payrollconfiguration";
        /// <summary>Type: Virtual, RequiredLevel: None, DisplayName: Pool Type, OptionSetType: Picklist</summary>
        public const string PoolType = "msdyn_pooltype";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string PPExpiryDate = "bcc_ppexpirydate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string ProbationCompletionDate = "bcc_probationcompletiondate";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Probation Period, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string ProbationPeriod = "bcc_probationperiod";
        /// <summary>Type: Lookup, RequiredLevel: Recommended, Targets: bcc_project</summary>
        public const string Project = "new_project";
        /// <summary>Type: Picklist, RequiredLevel: SystemRequired, DisplayName: Type of the resource., OptionSetType: Picklist, DefaultFormValue: 3</summary>
        public const string ResourceType = "resourcetype";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Work Start Location Type, OptionSetType: Picklist, DefaultFormValue: 690970002</summary>
        public const string StartLocation = "msdyn_startlocation";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string StudyMajors = "bcc_studymajors";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Test = "bcc_designationid";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string TimeOffApprovalRequired = "msdyn_timeoffapprovalrequired";
        /// <summary>Type: Integer, RequiredLevel: SystemRequired, MinValue: -1500, MaxValue: 1500</summary>
        public const string TimeZone = "timezone";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: -100000000000, MaxValue: 100000000000, Precision: 2</summary>
        public const string TotalSalary = "new_totalsalary";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Type, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Type = "new_type";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string UIDNo = "bcc_uidno";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_unit</summary>
        public const string Unit = "bcc_unit";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string UniversityName = "bcc_universityname";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: systemuser</summary>
        public const string User = "userid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string VisaExpiryDate = "bcc_visaexpirydate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string VisaNo = "bcc_visano";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_warehouse</summary>
        public const string Warehouse = "msdyn_warehouse";
        /// <summary>Type: Virtual, RequiredLevel: None, DisplayName: Weekly Rest, OptionSetType: Picklist</summary>
        public const string WeeklyRest = "bcc_weeklyrest";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Workmans Comp, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string WorkmansComp = "bcc_workmanscomp";
        /// <summary>Type: Currency, RequiredLevel: None, DisplayName: Accumulated Gratuity, DefaultFormValue: -1</summary>
        public const string AccumulatedGratuity = "bcc_accumulatedgratuity";
        /// <summary>Type: Currency, RequiredLevel: None, DisplayName: Gratuity Opening Balance, DefaultFormValue: -1</summary>
        public const string GratuityOpeningBalance = "bcc_gratuityopeningbalance";

        #endregion Attributes

        #region OptionSets

        public enum CrewStrategy_OptionSet
        {
            CrewLeaderManagement = 192350001,
            CrewMemberSelf_Management = 192350002,
            CascadeandAcceptCascadeCompletelyNotRecommended = 192350000
        }
        public enum Department_OptionSet
        {
            ACCOUNTS = 100000000,
            ADMIN = 100000011,
            ADMINDC = 100000003,
            CAMP = 100000016,
            CIVIL = 755700000,
            CONTRACTS = 100000013,
            COSTCONTROL = 100000014,
            DRIVER = 100000001,
            EMIRATI = 100000008,
            FINANCE = 100000015,
            HR = 100000010,
            HSE = 100000009,
            IT = 100000017,
            Item = 100000018,
            MEP = 755700001,
            PLANNING = 100000012,
            PROCUREMENTPURCHASE = 100000005,
            Q_SESTIMATION = 100000007,
            QA_QC = 100000004,
            STORETIMEKEEPING = 100000006,
            TECHNICAL = 100000002
        }
        public enum Division_OptionSet
        {
            ADMIN = 100000000,
            CIVIL = 100000001,
            HSE = 100000003,
            MEP = 100000002,
            OTHER = 100000004,
            OTHER_CORP = 100000005
        }
        public enum EndLocation_OptionSet
        {
            LocationAgnostic = 690970002,
            ResourceAddress = 690970000,
            OrganizationalUnitAddress = 690970001
        }
        public enum Gender_OptionSet
        {
            Male = 755700000,
            Female = 755700001
        }
        public enum GenericTypeDeprecated_OptionSet
        {
            ServiceCenter = 690970000
        }
        public enum IncludeAppointments_OptionSet
        {
            No = 192350000,
            Yes = 192350001
        }
        public enum IncludeOutlookFree_BusyinSearchResourceAvailabilityAPI_OptionSet
        {
            No = 192350000,
            Yes = 192350001
        }
        public enum MedicalInsurance_OptionSet
        {
            Yes = 755700000,
            No = 755700001,
            Applied = 755700002
        }
        public enum NoticePeriod_OptionSet
        {
            _1Month = 755700000,
            _2Month = 755700001,
            _3Month = 755700002
        }
        public enum PoolType_OptionSet
        {
            Account = 192350000,
            Contact = 192350001,
            User = 192350002,
            Equipment = 192350003,
            Facility = 192350004
        }
        public enum ProbationPeriod_OptionSet
        {
            _1Month = 755700000,
            _2Month = 755700001,
            _3Month = 755700002,
            _4Month = 755700003,
            _5Month = 755700004,
            _6Month = 755700005
        }
        public enum ResourceType_OptionSet
        {
            Generic = 1,
            Contact = 2,
            User = 3,
            Equipment = 4,
            Account = 5,
            Crew = 6,
            Facility = 7,
            Pool = 8
        }
        public enum StartLocation_OptionSet
        {
            LocationAgnostic = 690970002,
            ResourceAddress = 690970000,
            OrganizationalUnitAddress = 690970001
        }
        public enum Type_OptionSet
        {
            Staff = 100000001,
            Worker = 100000000
        }
        public enum WeeklyRest_OptionSet
        {
            Sunday = 755700000,
            Monday = 755700001,
            Tuesday = 755700002,
            Wednesday = 755700003,
            Thursday = 755700004,
            Friday = 755700005,
            Saturday = 755700006
        }
        public enum WorkmansComp_OptionSet
        {
            Yes = 755700000,
            No = 755700001
        }

        #endregion OptionSets
    }
}
