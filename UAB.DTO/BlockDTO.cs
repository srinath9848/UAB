using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DTO
{
    public class BlockDTO
    {
        [Required]
        public int BlockCategoryId { get; set; }
        public int ClinicalCaseId  { get; set; }
        public string Name { get; set; }
        [Required]
        public string Remarks   { get; set; }
        public DateTime ?CreateDate { get; set; }
    }
}
