using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class List
    {
        public List()
        {
            ClinicalCase = new HashSet<ClinicalCase>();
        }

        public int ListId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<ClinicalCase> ClinicalCase { get; set; }
    }
}
