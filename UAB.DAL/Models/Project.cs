using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class Project
    {
        public Project()
        {
            ClinicalCase = new HashSet<ClinicalCase>();
        }

        public int ProjectId { get; set; }
        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual Client Client { get; set; }
        public virtual ICollection<ClinicalCase> ClinicalCase { get; set; }
    }
}
