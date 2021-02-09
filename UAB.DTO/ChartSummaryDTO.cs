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
        }
        [Required(ErrorMessage = "Feedback Provider is required.")]
        public int ProviderFeedbackID { get; set; }
        public int AssignedTo { get; set; }
        [Required(ErrorMessage = "Payor is required.")]
        public int PayorID { get; set; }

        [Required(ErrorMessage = "Provider is required.")]
        public int ProviderID { get; set; }
        [Required(ErrorMessage = "CPT Code is required.")]
        public string CPTCode { get; set; }

        public string NoteTitle { get; set; }
        [Required(ErrorMessage = "Dx Code is required.")]
        public string Dx { get; set; }

        [Required(ErrorMessage = "Modifiers is required.")]
        public string Mod { get; set; }
        public string CoderQuestion { get; set; }
        public CodingDTO CodingDTO { get; set; }
        public QADTO QADTO { get; set; }
        public int QAPayorID { get; set; }
        public string QAPayorRemarks { get; set; }
        public int QAProviderID { get; set; }
        public string QAProviderRemarks { get; set; }
        public string QAMod { get; set; }
        public string QAModRemarks { get; set; }
        public string QADx { get; set; }
        public string QADxRemarks { get; set; }
        public string QACPTCode { get; set; }
        public string QACPTCodeRemarks { get; set; }
        public int QAProviderFeedbackID { get; set; }
        public string QAProviderFeedbackRemarks { get; set; }        
    }
}
