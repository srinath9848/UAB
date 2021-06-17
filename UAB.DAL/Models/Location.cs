using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.Models
{
    public partial class  Location
    {
        public int LocationId  { get; set; }
        [Required(ErrorMessage = "Location is required.")]
        [RegularExpression(@"^(([A-za-z0-9 ]+[\s]{1}[A-za-z0-9 ]+)|([A-Za-z0-9 ]+))$", ErrorMessage = "Special Character not allowed")]
        public string Name { get; set; }
    }
}
