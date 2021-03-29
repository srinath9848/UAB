using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DTO
{
    public class SearchParametersDTO
    {
        public string ClinicalCaseId { get; set; }
        public string  FirstName   { get; set; }
        public string LastName { get; set; }
        public string MRN { get; set; }
        public string ProviderName  { get; set; }
        //public string DoS { get; set; }
        
        public DateTime? DoSFrom   { get; set; }
        public DateTime? DoSTo  { get; set; }

        //public ProjectDTO Project { get; set; }
        public string ProjectName   { get; set; }

        //public StatusDTO Status { get; set; }
        public string StatusName  { get; set; }
        public bool IncludeBlocked  { get; set; }
        public string Assigneduser { get; set; }


    }
    public class ProjectDTO
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
    }
    public class StatusDTO 
    {
        public int StatusId { get; set; }
        public string StatusName { get; set; }
    }
    public class SearchResultDTO
    {
        public string ClinicalCaseId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MRN { get; set; }
        public string ProviderName  { get; set; }
        public DateTime DoS { get; set; }
        public string Status { get; set; }
        public string ProjectName { get; set; }
        [Required(ErrorMessage = "Assign From User Email is required.")]
        public string AssignFromUserEmail  { get; set; }
        
        [Required(ErrorMessage = "Assign To User Email is required.")]
        public string AssignToUserEmail { get; set; } 
        public bool IsPriority  { get; set; }
        public string IncludeBlocked { get; set; }
        public string Assigneduser { get; set; }
    }
}
