// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\Appointment.cs
// Created    : 2026-07-05 10:55:54
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>OwnershipType: UserOwned, IntroducedVersion: 5.0.0.0</summary>
    public static class Appointment
    {
        public const string EntityName = "appointment";
        public const string EntityCollectionName = "appointments";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "activityid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 200, Format: Text</summary>
        public const string PrimaryName = "subject";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string DeprecatedProcessStage = "stageid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1250, Format: Text</summary>
        public const string DeprecatedTraversedPath = "traversedpath";
        /// <summary>Type: EntityName, RequiredLevel: SystemRequired, DisplayName: Activity Type, OptionSetType: Picklist</summary>
        public const string ActivityType = "activitytypecode";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 0, MaxValue: 2147483647</summary>
        public const string ActualDuration = "actualdurationminutes";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string ActualEnd = "actualend";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string ActualStart = "actualstart";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 8192</summary>
        public const string AdditionalParameters = "activityadditionalparams";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string AllDayEvent = "isalldayevent";
        /// <summary>Type: Picklist, RequiredLevel: SystemRequired, DisplayName: Appointment Type, OptionSetType: Picklist, DefaultFormValue: 0</summary>
        public const string AppointmentType = "instancetypecode";
        /// <summary>Type: Integer, RequiredLevel: SystemRequired, MinValue: 0, MaxValue: 2147483647</summary>
        public const string AttachmentCount = "attachmentcount";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Attachment Errors, OptionSetType: Picklist, DefaultFormValue: 0</summary>
        public const string AttachmentErrors = "attachmenterrors";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 250, Format: Text</summary>
        public const string Category = "category";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: transactioncurrency</summary>
        public const string Currency = "transactioncurrencyid";
        /// <summary>Type: Memo, RequiredLevel: None, MaxLength: 1048576</summary>
        public const string Description = "description";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 0, MaxValue: 2147483647</summary>
        public const string Duration = "scheduleddurationminutes";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string EndTime = "scheduledend";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: 0.000000000001, MaxValue: 100000000000, Precision: 12</summary>
        public const string ExchangeRate = "exchangerate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string FormattedEndDate = "formattedscheduledend";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: TimeZoneIndependent</summary>
        public const string FormattedStartDate = "formattedscheduledstart";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Interview Round, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string InterviewRound = "bcc_interviewround";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsBilled = "isbilled";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsOnlineMeeting = "isonlinemeeting";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsPrivate = "ismapiprivate";
        /// <summary>Type: Boolean, RequiredLevel: SystemRequired, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsRegularActivity = "isregularactivity";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsWorkflowCreated = "isworkflowcreated";
        /// <summary>Type: Boolean, RequiredLevel: SystemRequired, True: 1, False: 0, DefaultValue: False</summary>
        public const string IsDraft = "isdraft";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: 0, MaxValue: 2147483647</summary>
        public const string IsUnsafe = "isunsafe";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string LastOnHoldTime = "lastonholdtime";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: sla</summary>
        public const string LastSLAapplied = "slainvokedid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 200, Format: Text</summary>
        public const string Location = "location";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string OnHoldTimeMinutes = "onholdtime";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 200, Format: Text</summary>
        public const string OnlineMeetingChatId = "onlinemeetingchatid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 300, Format: Text</summary>
        public const string OnlineMeetingId = "onlinemeetingid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 600, Format: Text</summary>
        public const string OnlineMeetingJoinUrl = "onlinemeetingjoinurl";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Online Meeting Type, OptionSetType: Picklist</summary>
        public const string OnlineMeetingType = "onlinemeetingtype";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string OriginalStartDate = "originalstartdate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 300, Format: Text</summary>
        public const string OutlookAppointment = "globalobjectid";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string OutlookAppointmentOwner = "outlookownerapptid";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Priority, OptionSetType: Picklist, DefaultFormValue: 1</summary>
        public const string Priority = "prioritycode";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string Process = "processid";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: account,adx_invitation,bcc_jobapplication,bookableresourcebooking,bookableresourcebookingheader,bulkoperation,campaign,campaignactivity,contact,contract,entitlement,entitlementtemplate,incident,invoice,knowledgearticle,knowledgebaserecord,lead,msdyn_agreement,msdyn_agreementbookingdate,msdyn_agreementbookingincident,msdyn_agreementbookingproduct,msdyn_agreementbookingservice,msdyn_agreementbookingservicetask,msdyn_agreementbookingsetup,msdyn_agreementinvoicedate,msdyn_agreementinvoiceproduct,msdyn_agreementinvoicesetup,msdyn_bookingalertstatus,msdyn_bookingrule,msdyn_bookingtimestamp,msdyn_customerasset,msdyn_fieldservicesetting,msdyn_incidenttypecharacteristic,msdyn_incidenttypeproduct,msdyn_incidenttypeservice,msdyn_inventoryadjustment,msdyn_inventoryadjustmentproduct,msdyn_inventoryjournal,msdyn_inventorytransfer,msdyn_payment,msdyn_paymentdetail,msdyn_paymentmethod,msdyn_paymentterm,msdyn_playbookinstance,msdyn_postalbum,msdyn_postalcode,msdyn_productinventory,msdyn_purchaseorder,msdyn_purchaseorderbill,msdyn_purchaseorderproduct,msdyn_purchaseorderreceipt,msdyn_purchaseorderreceiptproduct,msdyn_purchaseordersubstatus,msdyn_quotebookingincident,msdyn_quotebookingproduct,msdyn_quotebookingservice,msdyn_quotebookingservicetask,msdyn_resourceterritory,msdyn_rma,msdyn_rmaproduct,msdyn_rmareceipt,msdyn_rmareceiptproduct,msdyn_rmasubstatus,msdyn_rtv,msdyn_rtvproduct,msdyn_rtvsubstatus,msdyn_salessuggestion,msdyn_shipvia,msdyn_swarm,msdyn_systemuserschedulersetting,msdyn_timegroup,msdyn_timegroupdetail,msdyn_timeoffrequest,msdyn_warehouse,msdyn_workorder,msdyn_workordercharacteristic,msdyn_workorderincident,msdyn_workorderproduct,msdyn_workorderresourcerestriction,msdyn_workorderservice,msdyn_workorderservicetask,mspp_adplacement,mspp_pollplacement,mspp_publishingstatetransitionrule,mspp_redirect,mspp_shortcut,mspp_website,new_biometric,opportunity,quote,salesorder,site</summary>
        public const string Regarding = "regardingobjectid";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string SeriesId = "seriesid";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: service</summary>
        public const string Service = "serviceid";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: sla</summary>
        public const string SLA = "slaid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string SortDate = "sortdate";
        /// <summary>Type: DateTime, RequiredLevel: ApplicationRequired, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string StartTime = "scheduledstart";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 250, Format: Text</summary>
        public const string Sub_Category = "subcategory";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string Subscription = "subscriptionid";

        #endregion Attributes

        #region OptionSets

        public enum ActivityType_OptionSet
        {
            Fax = 4204,
            PhoneCall = 4210,
            Email = 4202,
            Letter = 4207,
            Appointment = 4201,
            Task = 4212,
            RecurringAppointment = 4251,
            Teamschat = 10253,
            InviteRedemption = 10403,
            PortalComment = 10404,
            QuickCampaign = 4406,
            CampaignActivity = 4402,
            CampaignResponse = 4401,
            CaseResolution = 4206,
            ServiceActivity = 4214,
            OpportunityClose = 4208,
            OrderClose = 4209,
            QuoteClose = 4211,
            CustomerVoicealert = 10715,
            CustomerVoicesurveyinvite = 10730,
            CustomerVoicesurveyresponse = 10733,
            Conversation = 10831,
            Session = 10848,
            CopilotTranscript = 11081,
            BookingAlert = 11279
        }
        public enum AppointmentType_OptionSet
        {
            NotRecurring = 0,
            RecurringMaster = 1,
            RecurringInstance = 2,
            RecurringException = 3,
            RecurringFutureException = 4
        }
        public enum AttachmentErrors_OptionSet
        {
            None = 0,
            TheappointmentwassavedasaMicrosoftDynamics365appointmentrecordbutnotalltheattachmentscouldbesavedwithitAnattachmentcannotbesavedifitisblockedorifitsfiletypeisinvalid = 1
        }
        public enum InterviewRound_OptionSet
        {
            Round1 = 755700000,
            Round2 = 755700001,
            Round3 = 755700002
        }
        public enum OnlineMeetingType_OptionSet
        {
            TeamsMeeting = 1
        }
        public enum Priority_OptionSet
        {
            Low = 0,
            Normal = 1,
            High = 2
        }
        public enum Status_OptionSet
        {
            Open = 0,
            Completed = 1,
            Canceled = 2,
            Scheduled = 3
        }
        public enum StatusReason_OptionSet
        {
            Free = 1,
            Tentative = 2,
            Completed = 3,
            Canceled = 4,
            Busy = 5,
            OutofOffice = 6
        }

        #endregion OptionSets
    }
}
