using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class Client
    {
        public Client()
        {
            Project = new HashSet<Project>();
        }

        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool? IsActive { get; set; }

        public virtual ICollection<Project> Project { get; set; }
    }
}
