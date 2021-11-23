using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.Models
{
    public partial class EMLevel
    {
        [Key] 
        public int Id { get; set; }

        [Required(ErrorMessage = "EM Level is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "only numbers allowed")]
        public string Level { get; set; }  
        public int ProjectId { get; set; }

    }
}
