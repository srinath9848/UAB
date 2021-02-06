using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DTO
{
    public class CodingSubmitDTO
    {
        [Required(ErrorMessage = "Feedback Provider is required.")]
        public int ProviderFeedbackID { get; set; }
        public int AssignedTo { get; set; }
        [Required(ErrorMessage = "Payor is required.")]
        public string PayorID { get; set; }
        [Required(ErrorMessage = "Provider is required.")]
        public string ProviderID { get; set; }
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
    }
}
