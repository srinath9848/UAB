﻿using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DTO
{
    public class SearchParametersDTO
    {
        public string ClinicalCaseId { get; set; }
        public string  FirstName   { get; set; }
        public string LastName { get; set; }
        public string MRN { get; set; }
        

        //public string ProviderId { get; set; }
        public string DoS { get; set; }

        //public ProjectDTO Project { get; set; }
        public string ProjectId  { get; set; }

        //public StatusDTO Status { get; set; }
        public string StatusId  { get; set; }


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
        //public string ProviderId { get; set; }
        public string DoS { get; set; }
        public string Status { get; set; }
        public string ProjectName { get; set; }
        public string AssignUserEmail  { get; set; }
    }
}
