using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public class ProjectType
    {
        public ProjectType()
        {
            Project = new HashSet<Project>();
        }

        public int ProjectTypeId { get; set; }
        public string ProjectTypeName { get; set; }

        public virtual ICollection<Project> Project { get; set; }
    }
}
