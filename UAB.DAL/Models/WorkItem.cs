using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class WorkItem
    {
        public int WorkItemId { get; set; }
        public int ClinicalCaseId { get; set; }
        public int StatusId { get; set; }
        public DateTime? AssignedDate { get; set; }
        public int? AssignedTo { get; set; }
        public int? AssignedBy { get; set; }
        public string NoteTitle { get; set; }
    }
}
