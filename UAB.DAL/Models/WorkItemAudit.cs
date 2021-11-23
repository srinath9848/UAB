using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class WorkItemAudit
    {
        public int WorkItemAuditId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int VersionId { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public string Remark { get; set; }
        public string ErrorTypeId { get; set; }
    }
}
