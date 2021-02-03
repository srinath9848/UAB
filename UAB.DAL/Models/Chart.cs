using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class Chart
    {
        public int ChartId { get; set; }
        public int ProjectId { get; set; }
        public int ListId { get; set; }
        public string PatientMrn { get; set; }
        public string PatientLastName { get; set; }
        public string PatientFirstName { get; set; }
        public DateTime DateOfService { get; set; }
        public int StatusId { get; set; }
        public int ProviderId { get; set; }
        public string EncounterNumber { get; set; }
        public int ProviderFeedbackId { get; set; }

        public virtual Project Project { get; set; }
        public virtual ProviderFeedback ProviderFeedback { get; set; }
        public virtual Status Status { get; set; }
    }
}
