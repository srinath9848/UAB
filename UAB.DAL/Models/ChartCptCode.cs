using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ChartCptCode
    {
        public int ChartDataId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int ChartVersionId { get; set; }
        public string Cptcode { get; set; }
        public string Modifier { get; set; }

        public virtual ClinicalCase ClinicalCase { get; set; }
    }
}
