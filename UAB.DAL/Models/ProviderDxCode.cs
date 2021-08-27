using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ProviderDxCode
    {
        public int ProviderDxCodeId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int VersionId { get; set; }
        public string DxCode { get; set; }
        public int? ClaimId { get; set; }
    }
}
