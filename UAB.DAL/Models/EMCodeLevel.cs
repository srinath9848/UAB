﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.Models
{
    public partial class EMCodeLevel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "EM Code is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "only numbers allowed")]
        public string EMCode  { get; set; }

        [Required(ErrorMessage = "EM Level is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "only numbers allowed")]
        public int EMLevel  { get; set; }

    }
}
