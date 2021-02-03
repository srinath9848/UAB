using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class CustomField
    {
        public int CustomFieldId { get; set; }
        public int ClinicalCaseId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public virtual ClinicalCase ClinicalCase { get; set; }
    }
}
