using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.Models
{
    public partial class ErrorType
    {
        public int ErrorTypeId { get; set; }
        [Required(ErrorMessage = "Error Type is required.")]
        [RegularExpression(@"^(?!\s*$).+", ErrorMessage = "Empty String Not Allowed")]
        public string Name { get; set; }
    }
}
