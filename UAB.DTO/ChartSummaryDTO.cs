﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DTO
{
    public class ChartSummaryDTO
    {
        public ChartSummaryDTO()
        {
            CodingDTO = new CodingDTO();
            QADTO = new QADTO();
            ShadowQADTO = new ShadowQADTO();
            BlockResponseDTO = new BlockResponseDTO();
            blockHistories = new List<BlockDTO>();
        }
        //[Required(ErrorMessage = "Feedback Provider is required.")]
        public string ProviderFeedbackID { get; set; }
        public string ProviderFeedbackText { get; set; }
        public int AssignedTo { get; set; }
        [Required(ErrorMessage = "*")]
        public int PayorID { get; set; }
        public string PayorText { get; set; }
        public string Status { get; set; }

        [Required(ErrorMessage = "*")]
        public int ProviderID { get; set; }
        [Required(ErrorMessage = "*")]
        public int BillingProviderID { get; set; }
        public string ProviderName { get; set; }
        public string ProviderText { get; set; }
        public string BillingProviderText { get; set; }
        [Required(ErrorMessage = "*")]
        public string CPTCode { get; set; }
        public string RejectedCpt { get; set; }
        public string NoteTitle { get; set; }
        [Required(ErrorMessage = "*")]
        public string Dx { get; set; }

        public string RejectedDx { get; set; }
        public bool IsCoderRebutted { get; set; }

        //[Required(ErrorMessage = "Modifiers is required.")]
        public string Mod { get; set; }
        public int Qty { get; set; }
        public string Links { get; set; }
        public string CoderQuestion { get; set; }
        [Required(ErrorMessage = "Answer is required.")]
        public string Answer { get; set; }
        public CodingDTO CodingDTO { get; set; }
        public QADTO QADTO { get; set; }
        public ShadowQADTO ShadowQADTO { get; set; }
        public BlockResponseDTO BlockResponseDTO { get; set; }
        [Required(ErrorMessage = "*")]
        public int QAPayorID { get; set; }
        public string QAPayorText { get; set; }
        public string ShadowQAPayorText { get; set; }
        [Required(ErrorMessage = "*")]
        public string QAPayorRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public int QAProviderID { get; set; }
        [Required(ErrorMessage = "*")]
        public int QABillingProviderID { get; set; }
        public string QAProviderText { get; set; }
        public string QABillingProviderText { get; set; }
        public string ShadowQAProviderText { get; set; }
        public string ShadowQABillingProviderText { get; set; }
        [Required(ErrorMessage = "*")]
        public string QAProviderRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string QABillingProviderRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string QAMod { get; set; }
        [Required(ErrorMessage = "*")]
        public string QAModRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string QADx { get; set; }
        [Required(ErrorMessage = "*")]
        public string QADxRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string QACPTCode { get; set; }
        [Required(ErrorMessage = "*")]
        public string QACPTCodeRemarks { get; set; }
        //[Required(ErrorMessage = "*")]
        public string QAProviderFeedbackID { get; set; }
        public string QAProviderFeedbackText { get; set; }
        public string QAErrorTypeText { get; set; }
        public string ShadowQAErrorTypeText { get; set; }
        public string ShadowQAProviderFeedbackText { get; set; }
        //[Required(ErrorMessage = "*")]
        public string QAProviderFeedbackRemarks { get; set; }

        [Required(ErrorMessage = "*")]
        public int PostedProviderId { get; set; }

        [Required(ErrorMessage = "*")]
        public int PostedPayorId { get; set; }
        [Required(ErrorMessage = "*")]
        public string PostedDx { get; set; }
        [Required(ErrorMessage = "*")]
        public string PostedCpt { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public string ProjectTypename { get; set; }

        [Required(ErrorMessage = "*")]
        public int ShadowQAPayorID { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAPayorRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public int ShadowQAProviderID { get; set; }
        [Required(ErrorMessage = "*")]
        public int ShadowQABillingProviderID { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAProviderRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQABillingProviderRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAMod { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAModRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQADx { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQADxRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQACPTCode { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQACPTCodeRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAProviderFeedbackID { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAProviderFeedbackRemarks { get; set; }
        public string CodedBy { get; set; }
        public string QABy { get; set; }
        public string ShadowQABy { get; set; }

        public string RevisedPayorRemarks { get; set; }
        public string RevisedProviderRemarks { get; set; }
        public string RevisedBillingProviderRemarks { get; set; }
        public string RevisedCPTRemarks { get; set; }
        public string RevisedModRemarks { get; set; }
        public string RevisedDXRemarks { get; set; }
        public string RevisedProviderFeedbackRemarks { get; set; }

        public string BlockCategory { get; set; }
        public string BlockRemarks { get; set; }
        public DateTime BlockedDate { get; set; }
        public string Blockedbyuser { get; set; }

        public int? ClaimId { get; set; }
        public int TabIndex { get; set; }
        public bool IsAuditRequired { get; set; }
        public bool SubmitAndPostAlso { get; set; }

        public int ProviderPostedId { get; set; }
        [Required(ErrorMessage = "*")]
        public DateTime PostingDate { get; set; }
        [Required(ErrorMessage = "*")]
        public string CoderComment { get; set; }
        public string CCIDs { get; set; }
        public bool isWrongProvider { get; set; }
        public AuditDTO auditDTO { get; set; }
        public List<BlockDTO> blockHistories { get; set; }
    }
    public class LelvellingReportDTO
    {
        [Required(ErrorMessage = "*")]
        public int ProjectID { get; set; }
        [Required(ErrorMessage = "*")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "*")]
        public DateTime EndDate { get; set; }
        public int? ProviderID { get; set; }
        public int? ListID { get; set; }
    }
}
