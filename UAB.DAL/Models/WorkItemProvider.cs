using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class WorkItemProvider
    {
        public int WorkItemProviderId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int VersionId { get; set; }
        public int ProviderId { get; set; }
        public int PayorId { get; set; }
        public int ProviderFeedbackId { get; set; }
    }
}
