using System;
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
        }
        //[Required(ErrorMessage = "Feedback Provider is required.")]
        public int? ProviderFeedbackID { get; set; }
        public string ProviderFeedbackText { get; set; }
        public int AssignedTo { get; set; }
        [Required(ErrorMessage = "*")]
        public int PayorID { get; set; }
        public string PayorText { get; set; }

        [Required(ErrorMessage = "*")]
        public int ProviderID { get; set; }
        public string ProviderText { get; set; }
        [Required(ErrorMessage = "*")]
        public string CPTCode { get; set; }
        public string RejectedCpt { get; set; }
        public string NoteTitle { get; set; }
        [Required(ErrorMessage = "*")]
        public string Dx { get; set; }

        public string RejectedDx { get; set; }

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
        [Required(ErrorMessage = "*")]
        public int QAPayorID { get; set; }
        public string QAPayorText { get; set; }
        [Required(ErrorMessage = "*")]
        public string QAPayorRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public int QAProviderID { get; set; }
        public string QAProviderText { get; set; }
        [Required(ErrorMessage = "*")]
        public string QAProviderRemarks { get; set; }
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
        public int QAProviderFeedbackID { get; set; }
        public string QAProviderFeedbackText { get; set; }
        //[Required(ErrorMessage = "*")]
        public string QAProviderFeedbackRemarks { get; set; }

        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        [Required(ErrorMessage = "*")]
        public int ShadowQAPayorID { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAPayorRemarks { get; set; }
        [Required(ErrorMessage = "*")]
        public int ShadowQAProviderID { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAProviderRemarks { get; set; }
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
        public int ShadowQAProviderFeedbackID { get; set; }
        [Required(ErrorMessage = "*")]
        public string ShadowQAProviderFeedbackRemarks { get; set; }
        public string CodedBy { get; set; }
        public string QABy { get; set; }
        public string ShadowQABy { get; set; }

        public string RevisedPayorRemarks { get; set; }
        public string RevisedProviderRemarks { get; set; }
        public string RevisedCPTRemarks { get; set; }
        public string RevisedModRemarks { get; set; }
        public string RevisedDXRemarks { get; set; }
        public string RevisedProviderFeedbackRemarks { get; set; }

        public string BlockCategory { get; set; }
        public string BlockRemarks { get; set; }
        public DateTime BlockedDate { get; set; }

        public int? ClaimId { get; set; }
        public int TabIndex { get; set; }
        public bool IsAuditRequired { get; set; }
        public bool SubmitAndPostAlso { get; set; }

    }

    public class LelvellingReportDTO
    {
        public List<string> columns { get; set; }
        [Required(ErrorMessage = "*")]
        public int ProjectID { get; set; }
        [Required(ErrorMessage = "*")]
        public DateTime StartDate { get; set; }
        [Required(ErrorMessage = "*")]
        public DateTime EndDate { get; set; }

        public string EmLevel { get; set; }
        public string EmCode { get; set; }
        public string Date1 { get; set; }
        public string Date2 { get; set; }
    }
}
