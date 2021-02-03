using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class Status
    {
        public Status()
        {
            Chart = new HashSet<Chart>();
            WorkItem = new HashSet<WorkItem>();
        }

        public int StatusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Chart> Chart { get; set; }
        public virtual ICollection<WorkItem> WorkItem { get; set; }
    }
}
