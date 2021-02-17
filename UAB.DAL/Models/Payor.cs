using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.Models
{
    public partial class Payor
    {
        public int PayorId { get; set; }
        [Required(ErrorMessage = "Payor is required.")]
        [RegularExpression(@"^(([A-za-z0-9]+[\s]{1}[A-za-z0-9]+)|([A-Za-z0-9]+))$", ErrorMessage = "Special Character not allowed. Please check and remove special character from Payor")]
        public string Name { get; set; }
    }
}
