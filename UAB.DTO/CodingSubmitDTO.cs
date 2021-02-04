using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DTO
{
    public class CodingSubmitDTO
    {
        public int ProviderFeedbackID { get; set; }
        public int AssignedTo { get; set; }
        public string PayorID { get; set; }
        [Required(ErrorMessage = "ProviderID is required.")]
        public string ProviderID { get; set; }
        public string CPTCode { get; set; }
        public string NoteTitle { get; set; }
        public string Dx { get; set; }
        public string Mod { get; set; }
        public string CoderQuestion { get; set; }
        public CodingDTO CodingDTO { get; set; }
    }
}
