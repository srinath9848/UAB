using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ChartDxCode
    {
        public int ChartDxCodeId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int ChartVersionId { get; set; }
        public string DxCode { get; set; }

        public virtual ClinicalCase ClinicalCase { get; set; }
    }
}
