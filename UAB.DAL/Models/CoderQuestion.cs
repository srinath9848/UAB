using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class CoderQuestion
    {
        public int CoderQuestionId { get; set; }
        public int ClinicalCaseId { get; set; }
        public string Question { get; set; }
        public int QuestionBy { get; set; }
        public DateTime QuestionDate { get; set; }
        public string Answer { get; set; }
        public int? AnsweredBy { get; set; }
        public DateTime? AnsweredDate { get; set; }

        public virtual ClinicalCase ClinicalCase { get; set; }
    }
}
