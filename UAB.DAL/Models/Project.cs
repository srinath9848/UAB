using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UAB.DAL.Models
{
    public partial class Project
    {
        public Project()
        {
            ClinicalCase = new HashSet<ClinicalCase>();
        }

        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Client is required.")]
        public int ClientId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public int ProjectTypeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string InputFileLocation { get; set; }
        public string InputFileFormat { get; set; }
        public int SLAInDays { get; set; }
        //public string ClientName { get; set; }
        //public string ProjectTypeName { get; set; }
        public virtual Client Client { get; set; }
        public virtual ICollection<ClinicalCase> ClinicalCase { get; set; }
    }
}
