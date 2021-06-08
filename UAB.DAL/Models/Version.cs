using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class Version
    {
        public int VersionId { get; set; }
        public int ClinicalCaseId { get; set; }
        public DateTime VersionDate { get; set; }
        public int UserId { get; set; }
        public int StatusId { get; set; }
        public string Remarks { get; set; }
    }
}
