// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccgulf.crm15.dynamics.com/
// Filename   : D:\DOCS\Personal\BLCC\ExportedDll\FileAttachment.cs
// Created    : 2026-09-23 11:13:28
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>OwnershipType: None, IntroducedVersion: 9.1.0.0</summary>
    public static class FileAttachment
    {
        public const string EntityName = "fileattachment";
        public const string EntityCollectionName = "fileattachments";

        #region Relationships

        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "Signature"</summary>
        public const string Rel1M_JobOfferSignature = "FileAttachment_bcc_JobOffer_bcc_Signature";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "SignContract"</summary>
        public const string Rel1M_JobOfferSignContract = "FileAttachment_bcc_JobOffer_bcc_SignContract";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "TawjeehTrainingDocument"</summary>
        public const string Rel1M_JobOfferTawjeehTrainingDocument = "FileAttachment_bcc_JobOffer_bcc_TawjiiTrainingDocument";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "UploadMadicalTest"</summary>
        public const string Rel1M_JobOfferUploadMadicalTest = "FileAttachment_bcc_JobOffer_bcc_UploadMadicalTest";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "ILOEDocument"</summary>
        public const string Rel1M_JobOfferILOEDocument = "FileAttachment_bcc_JobOffer_bcc_ILOEDocument";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "ResidenceDocument"</summary>
        public const string Rel1M_JobOfferResidenceDocument = "FileAttachment_bcc_JobOffer_bcc_ResidenceDocument";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "AttatchAirTicket"</summary>
        public const string Rel1M_JobOfferAttatchAirTicket = "FileAttachment_bcc_JobOffer_bcc_AttatchAirTicket";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "JobOffer"</summary>
        public const string Rel1M_JobOfferJobOffer = "FileAttachment_bcc_JobOffer_bcc_JobOffer";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "SignedJobOffer"</summary>
        public const string Rel1M_JobOfferSignedJobOffer = "FileAttachment_bcc_JobOffer_bcc_SignedJobOffer";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "EmployeeContract"</summary>
        public const string Rel1M_JobOfferEmployeeContract = "FileAttachment_bcc_JobOffer_bcc_EmployeeContract";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "SignedEmployeeContract"</summary>
        public const string Rel1M_JobOfferSignedEmployeeContract = "FileAttachment_bcc_JobOffer_bcc_SignedEmployeeContract";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "Passport"</summary>
        public const string Rel1M_JobOfferPassport = "FileAttachment_bcc_JobOffer_bcc_Passport";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "WorkerForm"</summary>
        public const string Rel1M_JobOfferWorkerForm = "FileAttachment_bcc_JobOffer_bcc_WorkerForm";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "CancellationForm"</summary>
        public const string Rel1M_JobOfferCancellationForm = "FileAttachment_bcc_JobOffer_bcc_CancellationForm";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "PreApprovalWorkPermit"</summary>
        public const string Rel1M_JobOfferPreApprovalWorkPermit = "FileAttachment_bcc_JobOffer_bcc_PreApprovalWorkPermit";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "InsuranceMolAttachment"</summary>
        public const string Rel1M_JobOfferInsuranceMolAttachment = "FileAttachment_bcc_JobOffer_bcc_InsuranceMolAttachment";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "TotalPreminum"</summary>
        public const string Rel1M_JobOfferTotalPreminum = "FileAttachment_bcc_JobOffer_bcc_TotalPreminum";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "Labourfee"</summary>
        public const string Rel1M_JobOfferLabourfee = "FileAttachment_bcc_JobOffer_bcc_Labourfee";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "VisaFee"</summary>
        public const string Rel1M_JobOfferVisaFee = "FileAttachment_bcc_JobOffer_bcc_VisaFee";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "ChangeStatus"</summary>
        public const string Rel1M_JobOfferChangeStatus = "FileAttachment_bcc_JobOffer_bcc_ChangeStatus";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "MedicalFee"</summary>
        public const string Rel1M_JobOfferMedicalFee = "FileAttachment_bcc_JobOffer_bcc_MedicalFee";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "EID"</summary>
        public const string Rel1M_JobOfferEID = "FileAttachment_bcc_JobOffer_bcc_EID";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "TawjeeTraining"</summary>
        public const string Rel1M_JobOfferTawjeeTraining = "FileAttachment_bcc_JobOffer_bcc_TawjeeTraining";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "DegreeCertificate"</summary>
        public const string Rel1M_JobOfferDegreeCertificate = "FileAttachment_bcc_JobOffer_bcc_DegreeCertificate";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "Passport2"</summary>
        public const string Rel1M_JobOfferPassport2 = "FileAttachment_bcc_JobOffer_bcc_Passport2";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "DegreeCertificationjo"</summary>
        public const string Rel1M_JobOfferDegreeCertificationjo = "FileAttachment_bcc_JobOffer_bcc_DegreeCertificationjo";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "Passport2"</summary>
        public const string Rel1M_JobOfferPassport21 = "FileAttachment_bcc_JobOffer_bcc_Passport2jo";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "Passport3"</summary>
        public const string Rel1M_JobOfferPassport3 = "FileAttachment_bcc_JobOffer_bcc_Passport3";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "DegreeCertificate2"</summary>
        public const string Rel1M_JobOfferDegreeCertificate2 = "FileAttachment_bcc_JobOffer_bcc_DegreeCertificate2";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "BccOfferLetter"</summary>
        public const string Rel1M_JobOfferBccOfferLetter = "FileAttachment_bcc_JobOffer_bcc_OtherDocument";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "RejectDoc"</summary>
        public const string Rel1M_JobOfferRejectDoc = "FileAttachment_bcc_JobOffer_bcc_RejectDoc";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "RejectDoc1"</summary>
        public const string Rel1M_JobOfferRejectDoc1 = "FileAttachment_bcc_JobOffer_bcc_RejectDoc1";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "MedicalCertificate"</summary>
        public const string Rel1M_JobOfferMedicalCertificate = "FileAttachment_bcc_JobOffer_bcc_MedicalCertificate";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "MedicalResult"</summary>
        public const string Rel1M_JobOfferMedicalResult = "FileAttachment_bcc_JobOffer_bcc_MedicalResult";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "CancelDocument"</summary>
        public const string Rel1M_JobOfferCancelDocument = "FileAttachment_bcc_JobOffer_bcc_CancelDocument";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "MohreLetter"</summary>
        public const string Rel1M_JobOfferMohreLetter = "FileAttachment_bcc_JobOffer_bcc_MohreLetter";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "BiomatricDoc"</summary>
        public const string Rel1M_JobOfferBiomatricDoc = "FileAttachment_bcc_JobOffer_bcc_BiomatricDoc";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "NewBiomatricDoc"</summary>
        public const string Rel1M_JobOfferNewBiomatricDoc = "FileAttachment_bcc_JobOffer_bcc_NewBiomatricDoc";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "Visa"</summary>
        public const string Rel1M_JobOfferVisa = "FileAttachment_bcc_JobOffer_bcc_Visa";
        /// <summary>Parent: "FileAttachment" Child: "JobOffer" Lookup: "EmiratesID"</summary>
        public const string Rel1M_JobOfferEmiratesID = "FileAttachment_bcc_JobOffer_bcc_EmiratesID";

        #endregion Relationships
    }
}
