using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DTO
{
    public class BlockDTO
    {
        public int BlockCategoryId { get; set; }
        public int ClinicalCaseId  { get; set; }
        public string Name { get; set; }
        public string Remarks   { get; set; }
    }
}
