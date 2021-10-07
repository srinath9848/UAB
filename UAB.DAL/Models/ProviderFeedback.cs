using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UAB.DAL.Models
{
    public partial class ProviderFeedback
    {
        public int ProviderFeedbackId { get; set; }
        [Required(ErrorMessage = "Provider Feedback is required.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Empty String Not Allowed")]
        public string Feedback { get; set; }
    }
}
