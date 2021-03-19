using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class CptCode
    {
        public int CptCodeId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int VersionId { get; set; }
        public string Cptcode1 { get; set; }
        public string Modifier { get; set; }
        public string Qty { get; set; }
        public string Links { get; set; }
    }
}
