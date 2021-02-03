using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ClinicalCase
    {
        public ClinicalCase()
        {
            ChartCptCode = new HashSet<ChartCptCode>();
            ChartDxCode = new HashSet<ChartDxCode>();
            ChartVersion = new HashSet<ChartVersion>();
            CoderQuestion = new HashSet<CoderQuestion>();
            CustomField = new HashSet<CustomField>();
            WorkItem = new HashSet<WorkItem>();
        }

        public int ClinicalCaseId { get; set; }
        public int ProjectId { get; set; }
        public int ListId { get; set; }
        public string PatientMrn { get; set; }
        public string PatientLastName { get; set; }
        public string PatientFirstName { get; set; }
        public DateTime DateOfService { get; set; }
        public string EncounterNumber { get; set; }
        public int? ProviderId { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual List List { get; set; }
        public virtual Project Project { get; set; }
        public virtual ICollection<ChartCptCode> ChartCptCode { get; set; }
        public virtual ICollection<ChartDxCode> ChartDxCode { get; set; }
        public virtual ICollection<ChartVersion> ChartVersion { get; set; }
        public virtual ICollection<CoderQuestion> CoderQuestion { get; set; }
        public virtual ICollection<CustomField> CustomField { get; set; }
        public virtual ICollection<WorkItem> WorkItem { get; set; }
    }
}
