using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace UAB.DAL.Models
{
    public class ApplicationProject
    {
        public int ProjectId { get; set; }
        public int ClientId { get; set; }
        public int ProjectTypeId { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; }
        public string InputFileLocation { get; set; }
        [StringLength(5, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string InputFileFormat { get; set; }
        public string ClientName { get; set; }
        public string ProjectTypeName { get; set; }
    }
}
