using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class DxCode
    {
        public int DxCodeId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int VersionId { get; set; }
        public string DxCode1 { get; set; }
    }
}
