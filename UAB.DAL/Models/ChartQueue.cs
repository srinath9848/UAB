using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ChartQueue
    {
        public ChartQueue()
        {
            ChartVersion = new HashSet<ChartVersion>();
        }

        public int ChartQueueId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ChartVersion> ChartVersion { get; set; }
    }
}
