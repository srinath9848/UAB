using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.Models
{
    public class CptAuditDTO
    {
        public int CPTAuditId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName  { get; set; }

        [StringLength(5, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string CPTCode { get; set; }
        public bool IsActive { get; set; }

    }
}
