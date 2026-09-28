// *********************************************************************
// Created by : Latebound Constants Generator 1.2025.8.1 for XrmToolBox
// Tool Author: Jonas Rapp https://jonasr.app/
// GitHub     : https://github.com/rappen/LCG-UDG/
// Source Org : https://bccdev.crm15.dynamics.com/
// Filename   : D:\Projects\BCCGULF\BCCGULF\BCCGULF\Constants\InterviewQuestion.cs
// Created    : 2026-07-05 10:55:54
// *********************************************************************
namespace BCCGULF.Constants
{
    /// <summary>DisplayName: Interview Question, OwnershipType: UserOwned, IntroducedVersion: 1.0</summary>
    public static class InterviewQuestion
    {
        public const string EntityName = "bcc_interviewquestion";
        public const string EntityCollectionName = "bcc_interviewquestions";

        #region Attributes

        /// <summary>Type: Uniqueidentifier, RequiredLevel: SystemRequired</summary>
        public const string PrimaryKey = "bcc_interviewquestionid";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 850, Format: Text</summary>
        public const string PrimaryName = "bcc_name";
        /// <summary>Type: String, RequiredLevel: None, MaxLength: 1000, Format: Text</summary>
        public const string Answer = "bcc_answer";
        /// <summary>Type: State, RequiredLevel: SystemRequired, DisplayName: Status, OptionSetType: State</summary>
        public const string Status = "statecode";
        /// <summary>Type: Status, RequiredLevel: None, DisplayName: Status Reason, OptionSetType: Status</summary>
        public const string StatusReason = "statuscode";

        #endregion Attributes

        #region OptionSets

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

        #endregion OptionSets
    }
}
