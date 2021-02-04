using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ProviderFeedback
    {
        public ProviderFeedback()
        {
            Chart = new HashSet<Chart>();
            WorkItem = new HashSet<WorkItem>();
        }

        public int ProviderFeedbackId { get; set; }
        public string Feedback { get; set; }

        public virtual ICollection<Chart> Chart { get; set; }
        public virtual ICollection<WorkItem> WorkItem { get; set; }
    }
}
