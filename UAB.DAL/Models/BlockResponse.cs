using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.Models
{
    public partial class BlockResponse
    {
        public int BlockResponseId { get; set; } 
        public int ClinicalCaseId { get; set; }
        public string ResponseRemarks { get; set; }
        public int ResponseByUserId  { get; set; }
        public DateTime ResponseDate  { get; set; }
    }
}
