using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DTO
{
    public class CodingDTO
    {
        public int ClinicalCaseID { get; set; }
        public string PatientMRN { get; set; }
        public string Name { get; set; }
        public string DateOfService { get; set; }
    }
}
