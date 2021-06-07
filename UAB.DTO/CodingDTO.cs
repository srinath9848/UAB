using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DTO
{
    public class CodingDTO
    {
        public int ClinicalCaseID { get; set; }
        public string PatientMRN { get; set; }
        public string Name { get; set; }
        public string DateOfService { get; set; }
        public string ListName  { get; set; }
    }
    public class BlockResponseDTO
    {
        public int BlockResponseId { get; set; }
        public int ClinicalCaseId { get; set; }
        public string ResponseRemarks { get; set; }
        public int ResponseByUserId { get; set; }
        public string ResponseByUserName { get; set; }
        public DateTime ResponseDate { get; set; }

        public string BlockCategory { get; set; }
        public string BlockRemarks { get; set; }
        public DateTime BlockedDate { get; set; }
        public string Blockedbyuser { get; set; }

        public string BlockedInQueueKind { get; set; }

    }
    public class QADTO
    {
        public string CoderRebuttal { get; set; }
        //[Required(ErrorMessage = "Error Type is required.")]
        public int? ErrorType { get; set; }
    }

    public class ShadowQADTO
    {
        public string CoderRebuttal { get; set; }
        //[Required(ErrorMessage = "Error Type is required.")]
        public int? ErrorType { get; set; }
        public string NotesfromJen { get; set; }
        public bool OkaytoPost  { get; set; }
    }
}
