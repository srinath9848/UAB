using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.Models
{
    public partial  class BlockHistory
    {
        public int BlockHistoryId  { get; set; }
        public int BlockedByUserId { get; set; }
        public int BlockCategoryId { get; set; }
        public string Remarks  { get; set; }
        public DateTime? CreateDate { get; set; }
        public int ClinicalCaseId { get; set; }
        public string BlockedInQueueKind  { get; set; }
    }
}
