// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccgulf.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\JobOffer.cs
// Created    : 2026-06-15 12:41:49
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Job Offer, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class JobOffer
    {
        public const string EntityName = "bcc_joboffer";
        public const string EntityCollectionName = "bcc_joboffers";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_jobofferid";
        /// <summary>Type: String, RequiredLevel: ApplicationRequired, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_newcolumn";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string SignedEmployeeContract = "bcc_signedemployeecontract";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string DeprecatedStageId = "stageid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1250, Format: Text</summary>
        public const string DeprecatedTraversedPath = "traversedpath";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Address = "bcc_address";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: account</summary>
        public const string Agency = "bcc_agency";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Allowance = "bcc_allowance";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Amount = "bcc_amountiloe";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Amount1 = "bcc_amountme";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: 0, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Amount2 = "bcc_amount";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: 0, MaxValue: 922337203685477, Precision: 2</summary>
        public const string AmountChargeCS = "bcc_amountchargecs";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Application Form, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string ApplicationForm = "bcc_applicationform";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Application From_, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string ApplicationFrom = "bcc_applicationfrom_";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Approval, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Approval = "bcc_approval";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string AttatchAirTicket = "bcc_attatchairticket";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Basic = "bcc_basic";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string BccOfferLetter = "bcc_otherdocument";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string ImageId = "bcc_imageid";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string CancelDocument = "bcc_canceldocument";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string CancelDocumentUploadDate = "bcc_canceldocumentuploaddate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string CancelReason = "bcc_cancelreason";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string CancellationForm = "bcc_cancellationform";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string CancellationFormUploadDate = "bcc_cancellationformuploaddate";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string CanclePopDone = "bcc_canclepopdone";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired</summary>
        public const string ChangeStatus = "bcc_changestatus";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string ChangeStausUploadDate = "bcc_changestausuploaddate";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: 0, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Charge = "bcc_charge";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: contact</summary>
        public const string Contact = "bcc_contact";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Contract, OptionSetType: Picklist, DefaultFormValue: 755700000</summary>
        public const string Contract = "bcc_contract";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: transactioncurrency</summary>
        public const string Currency = "transactioncurrencyid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string DateOfArrival = "bcc_dateofarrival";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string DateOfBirth = "bcc_dateofbirth";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string DegreeCertificate = "bcc_degreecertificate";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string DegreeCertificate2 = "bcc_degreecertificate2";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string DegreeCertificate2UploadDate = "bcc_degreecertificate2uploaddate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string DegreeCertificateUploadDate = "bcc_degreecertificateuploaddate";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string DegreeCertificationjo = "bcc_degreecertificationjo";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string DegreeEndDate = "bcc_degreeenddate";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string DegreeStartDate = "bcc_degreestartdate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string DegreeType = "bcc_degreetype";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_designation</summary>
        public const string Designation = "bcc_designation";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_manpowerquota</summary>
        public const string DesignationAsPerVisa = "bcc_designationaspervisa";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired</summary>
        public const string EID = "bcc_eid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string EIDUploadDate = "bcc_eiduploaddate";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string EmiratesIDAppFees = "bcc_emiratesidappfees";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string EmployeeContract = "bcc_employeecontract";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string EmployeeContractUploadDate = "bcc_employeecontractdocdate";
        /// <summary>Type: Decimal, RequiredLevel: None, MinValue: 0.0000000001, MaxValue: 100000000000, Precision: 12</summary>
        public const string ExchangeRate = "exchangerate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Faculty = "bcc_faculty";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string Fine = "bcc_fine";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Gender, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Gender = "bcc_gender";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string GivenNames = "bcc_givennames";
        /// <summary>Type: Integer, RequiredLevel: None, MinValue: -2147483648, MaxValue: 2147483647</summary>
        public const string GraduationYear = "bcc_graduationyear";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string GrandTotal = "bcc_grandtotalre";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string GrandTotal1 = "bcc_grandtotalt";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: 0, MaxValue: 922337203685477, Precision: 2</summary>
        public const string GrandTotal2 = "bcc_grandtotal";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string HRA = "bcc_hrajoboffer";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string ILOEDocument = "bcc_iloedocument";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string ILOEInsurenceCharge = "bcc_iloeinsurencechargeiloe";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string ILOEInsurenceCharge1 = "bcc_iloeinsurencecharge";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string ILOEInsurenceFees = "bcc_iloeinsurencefees";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired</summary>
        public const string InsuranceMolAttachment = "bcc_insurancemolattachment";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string InsuranceMolAttachmentUploadDate = "bcc_insurancemolattachmentuploaddate";
        /// <summary>Type: Lookup, RequiredLevel: ApplicationRequired, Targets: bcc_jobapplication</summary>
        public const string JobApplication = "bcc_jobapplication";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_jobapplication</summary>
        public const string JobApplicationLookup = "bcc_jobapplicationlookup";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string JobApplicationNumber = "bcc_jobapplicationnumber";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string JobOffer_ = "bcc_joboffer";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string JobofferNumber = "bcc_joboffernumber";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Job Offer Status, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string JobOfferStatus = "bcc_jobofferstatus";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string JobOfferUploadDate = "bcc_jobofferdate";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired</summary>
        public const string Labourfee = "bcc_labourfee";
        /// <summary>Type: Boolean, RequiredLevel: None, True: 1, False: 0, DefaultValue: False</summary>
        public const string LabourFeePaid = "bcc_labourfeepaid";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string LabourFeeUploadDate = "bcc_labourfeeuploaddate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string LastName = "bcc_lastname";
        /// <summary>Type: Virtual, RequiredLevel: None, DisplayName: Mail Status, OptionSetType: Picklist</summary>
        public const string MailStatus = "bcc_mailstatus";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_manpowerquota</summary>
        public const string ManPowerQuota = "bcc_manpowerquota";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_manpowerrequest</summary>
        public const string ManPowerRequest = "bcc_manpowerrequest";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string MedicalApplicationFees = "bcc_medicalapplicationfees";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string MedicalCertificate = "bcc_medicalcertificate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string MedicalCertificateUploadDate = "bcc_medicalcertificateuploaddate";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired</summary>
        public const string MedicalFee = "bcc_medicalfee";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string MedicalFeeUploadDate = "bcc_medicalfeeuploaddate";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string MedicalResult = "bcc_medicalresult";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string MedicalResultUploadDate = "bcc_medicalresultuploaddate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string MiddleName = "bcc_middlename";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string MohreLetter = "bcc_mohreletter";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Name = "bcc_name";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: msdyn_region</summary>
        public const string Nationality = "bcc_nationality";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string Nationality1 = "bcc_nationality_";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Notice Period, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string NoticePeriod = "bcc_noticeperiod";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string OtherDocumentUploadDate = "bcc_otherdocumentuploaddate";
        /// <summary>Type: Lookup, RequiredLevel: None, Targets: bcc_joboffer</summary>
        public const string Parentjoboffer = "bcc_parentjoboffer";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string Passport = "bcc_passport";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string Passport2 = "bcc_passport2";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string Passport21 = "bcc_passport2jo";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string Passport2UploadDate = "bcc_passport2uploaddate";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string Passport3 = "bcc_passport3";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string Passport3UploadDate = "bcc_passport3uploaddate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string PassportExpiryDate = "bcc_passportexpirydate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string PassportIssueDate = "bcc_passportissuedate";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string PassportNo = "bcc_passportno";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string PassportUploadDate = "bcc_passportuploaddate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string PhotoUploadDate = "bcc_photouploaddate";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Initial Approval, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string PostApproval = "bcc_initialapproval";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string PreApprovalWorkPermit = "bcc_preapprovalworkpermit";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string PreApprovalWorkPermitUploadDate = "bcc_preapprovalworkpermitdate";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Probation Period, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string ProbationPeriod = "bcc_probationperiod";
        /// <summary>Type: Uniqueidentifier, RequiredLevel: None</summary>
        public const string ProcessId = "processid";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string RejectDoc = "bcc_rejectdoc";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string RejectDocUploadDate = "bcc_rejectdocuploaddate";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string RejectDoc1 = "bcc_rejectdoc1";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string RejectRemark = "bcc_rejectremark";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string ResidenceCharge = "bcc_residencecharge";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired</summary>
        public const string ResidenceDocument = "bcc_residencedocument";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string ResidenceDocumentUploadDate = "bcc_residencedocumentuploaddate";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string ResidenceFees = "bcc_residencefees";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string SalaryDetail = "bcc_salarydetail";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string SignContract = "bcc_signcontract";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string SignContractUploadDate = "bcc_signcontractdocdate";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string Signature = "bcc_signature";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string SignedJobOffer = "bcc_signedjoboffer";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string SignedJobOfferUploadDate = "bcc_signedjobofferdate";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Status, OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string Status = "bcc_status";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status1 = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string StudyMajors = "bcc_studymajors";
        /// <summary>Type: Picklist, RequiredLevel: None, DisplayName: Sub Stage Status, OptionSetType: Picklist, DefaultFormValue: 755700000</summary>
        public const string SubStageStatus = "bcc_substagestatus";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired</summary>
        public const string TawjeeTraining = "bcc_tawjeetraining";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string TawjeeTrainingUploadDate = "bcc_tawjeetraininguploaddate";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string TawjeehTrainingDocument = "bcc_tawjiitrainingdocument";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TawjiiTrainingClassFees = "bcc_tawjiitrainingclassfees";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalAmount = "bcc_totalamountla";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalAmount1 = "bcc_totalamounttt";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalAmount2 = "bcc_totalamount";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalCharge = "bcc_totalchargela";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalCharge1 = "bcc_totalchargere";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalCharge2 = "bcc_totalcharget";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalCharge3 = "bcc_totalcharge";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalChargeEID = "bcc_totalchargeeid";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: 0, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalChargeMF = "bcc_totalchargemf";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalEID = "bcc_totaleid";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string TotalPreminum = "bcc_totalpreminum";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: 0, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalPremium = "bcc_totalpremium";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalPremium1 = "bcc_totalpremiummf";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string TotalSalary = "bcc_totalsalary";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string TransactionNumber = "bcc_transactionid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string TypingElectronicPreApprovalforWorkPermitApp = "bcc_typingelectronicpreapprovalforworkpermitapp";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string TypingNewJobOfferletter = "bcc_typingnewjobofferletter";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string UniversityName = "bcc_universityname";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string UploadMadicalTest = "bcc_uploadmadicaltest";
        /// <summary>Type: Picklist, RequiredLevel: ApplicationRequired, DisplayName: Apply Visa , OptionSetType: Picklist, DefaultFormValue: -1</summary>
        public const string VisaFrom = "bcc_applyvisa";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string VisaCharge = "bcc_visachargevf";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 100, Format: Text</summary>
        public const string VisaCharge1 = "bcc_visacharge";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateOnly, DateTimeBehavior: UserLocal</summary>
        public const string Visadateofexpiry = "bcc_visadateofexpiry";
        /// <summary>Type: Virtual, RequiredLevel: ApplicationRequired</summary>
        public const string VisaFee = "bcc_visafee";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string VisaFeeUploadDate = "bcc_visafeeuploaddate";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string VisaFeeUploadDate1 = "bcc_visafeeuploaddate_";
        /// <summary>Type: Money, RequiredLevel: None, MinValue: -922337203685477, MaxValue: 922337203685477, Precision: 2</summary>
        public const string VisaFees = "bcc_visafees";
        /// <summary>Type: Virtual, RequiredLevel: None, DisplayName: Weekly Rest, OptionSetType: Picklist</summary>
        public const string WeeklyRest = "bcc_weeklyrest";
        /// <summary>Type: Virtual, RequiredLevel: None</summary>
        public const string WorkerForm = "bcc_workerform";
        /// <summary>Type: DateTime, RequiredLevel: None, Format: DateAndTime, DateTimeBehavior: UserLocal</summary>
        public const string WorkerFormUploadDate = "bcc_workerformuploaddate";

        #endregion Attributes

        #region OptionSets

        public enum ApplicationForm_OptionSet
        {
            Worker = 755700000,
            Staff = 755700001
        }
        public enum ApplicationFrom_OptionSet
        {
            InsideCountry = 755700000,
            OutsideCountry = 755700001
        }
        public enum Approval_OptionSet
        {
            Approved = 755700000,
            Reject = 755700001
        }
        public enum Contract_OptionSet
        {
            NewContract = 755700000,
            RenewalContract = 755700001,
            GoldenVisa = 755700002,
            RelativeVisa = 755700003
        }
        public enum Gender_OptionSet
        {
            Male = 755700000,
            Female = 755700001
        }
        public enum JobOfferStatus_OptionSet
        {
            Successful = 755700000
        }
        public enum MailStatus_OptionSet
        {
            MailtoCandidate = 755700000,
            MailtoAgency = 755700001
        }
        public enum NoticePeriod_OptionSet
        {
            _1Month = 755700000,
            _2Month = 755700001,
            _3Month = 755700002
        }
        public enum PostApproval_OptionSet
        {
            postApproval = 755700000,
            PostRejection = 755700001
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
        public enum JobStatus_OptionSet
        {
            Finished = 755700000,
            Pending = 755700001
        }
        public enum Status_OptionSet
        {
            Active = 0,
            Inactive = 1
        }
        public enum StatusReason_OptionSet
        {
            Active = 1,
            Initialization = 100000000,
            PreApprovalWorkPermit = 100000001,
            VisaStage = 100000002,
            Inactive = 2,
            Hired = 100000003,
            InsuranceMOLForm = 755700001,
            LabourFee = 755700002,
            VisaFee = 755700003,
            ChangeStatus = 755700004,
            MedicalFee = 755700005,
            EIDFee = 755700007,
            ILOEFee = 755700008,
            TawjeeFee = 755700009,
            Residency = 755700010,
            Hired1 = 755700011,
            Cancelled = 755700012,
            ArrivedInCountry = 755700013,
            MedicalFee_Tawjee = 755700014,
            SignedOffer = 755700015,
            Approval = 755700016,
            Cancelled1 = 755700017
        }
        public enum SubStageStatus_OptionSet
        {
            PendingwithPRO = 755700000,
            PendingwithHR = 755700001,
            ProcessComplete = 755700002,
            Cancelled = 755700003
        }
        public enum VisaFrom_OptionSet
        {
            InsideCountry = 755700000,
            OutsideCountry = 755700001
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

        #endregion OptionSets
    }
}
