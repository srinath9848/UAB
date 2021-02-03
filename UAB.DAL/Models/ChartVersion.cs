using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ChartVersion
    {
        public int ChartVersionId { get; set; }
        public int ClinicalCaseId { get; set; }
        public DateTime VersionDate { get; set; }
        public int UserId { get; set; }
        public int ChartQueueId { get; set; }

        public virtual ChartQueue ChartQueue { get; set; }
        public virtual ClinicalCase ClinicalCase { get; set; }
    }
}
