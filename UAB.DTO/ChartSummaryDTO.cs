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
        }
        [Required(ErrorMessage = "Feedback Provider is required.")]
        public int ProviderFeedbackID { get; set; }
        public string ProviderFeedbackText { get; set; }
        public int AssignedTo { get; set; }
        [Required(ErrorMessage = "Payor is required.")]
        public int PayorID { get; set; }
        public string PayorText { get; set; }

        [Required(ErrorMessage = "Provider is required.")]
        public int ProviderID { get; set; }
        public string ProviderText { get; set; }
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
        public ShadowQADTO ShadowQADTO { get; set; }
        public int QAPayorID { get; set; }
        public string QAPayorRemarks { get; set; }
        public int QAProviderID { get; set; }
        public string QAProviderText { get; set; }
        public string QAPayorText { get; set; }
        public string QAProviderRemarks { get; set; }
        public string QAMod { get; set; }
        public string QAModRemarks { get; set; }
        public string QADx { get; set; }
        public string QADxRemarks { get; set; }
        public string QACPTCode { get; set; }
        public string QACPTCodeRemarks { get; set; }
        public int QAProviderFeedbackID { get; set; }
        public string QAProviderFeedbackText { get; set; }
        public string QAProviderFeedbackRemarks { get; set; }


        public int ShadowQAPayorID { get; set; }
        
        public string ShadowQAPayorRemarks { get; set; }
        public int ShadowQAProviderID { get; set; }
        
        public string ShadowQAProviderRemarks { get; set; }
        public string ShadowQAMod { get; set; }
        public string ShadowQAModRemarks { get; set; }
        public string ShadowQADx { get; set; }
        public string ShadowQADxRemarks { get; set; }
        public string ShadowQACPTCode { get; set; }
        public string ShadowQACPTCodeRemarks { get; set; }
        public int ShadowQAProviderFeedbackID { get; set; }       
        public string ShadowQAProviderFeedbackRemarks { get; set; }
    }
}