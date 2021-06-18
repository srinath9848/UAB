using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace UAB.DAL.Models
{
    public class ApplicationProject
    {
        public int ProjectId { get; set; }
        [Required(ErrorMessage = "Client is required.")]
        public int ClientId { get; set; }
        [Required(ErrorMessage = "Project Type is required.")]
        public int ProjectTypeId { get; set; }
        [Required(ErrorMessage = "Project is required.")]
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public string InputFileLocation { get; set; }
        [StringLength(5, ErrorMessage = "The {0} value cannot exceed {1} characters. ")]
        public string InputFileFormat { get; set; }
        public string ClientName { get; set; }
        public string ProjectTypeName { get; set; }
        public int SLAInDays { get; set; }
    }
}
