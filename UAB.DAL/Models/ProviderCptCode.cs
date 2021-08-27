using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ProviderCptCode
    {
        public int ProviderCptCodeId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int VersionId { get; set; }
        public string Cptcode { get; set; }
        public string Modifier { get; set; }
        public string Qty { get; set; }
        public string Links { get; set; }
        public int? ClaimId { get; set; }
    }
}
