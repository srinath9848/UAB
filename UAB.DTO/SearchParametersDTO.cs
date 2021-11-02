using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DTO
{
    public class SearchParametersDTO
    {
        public string ClinicalCaseId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MRN { get; set; }
        public int? ProviderId { get; set; }
        public string ProviderName { get; set; }
        public DateTime? DoSFrom { get; set; }
        public DateTime? DoSTo { get; set; }
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int? StatusId { get; set; }
        public string StatusName { get; set; }
        public bool IncludeBlocked { get; set; }
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

    public class ExportSearchResultDTO
    {
        public ExportSearchResultDTO()
        {
            CPTDxInfo = new List<CPTAndDxInfo>();
        }
        public DateTime DoS { get; set; }
        public string MRN { get; set; }
        public string PatientName { get; set; }
        public string ProviderName { get; set; }
        public string DxCodes { get; set; }
        public string CptCodes { get; set; }
        public string PostedBy { get; set; }
        public string PostedDate { get; set; }
        public string CodedBy { get; set; }
        public string QABy { get; set; }
        public string ShadowQABy { get; set; }
        public string ProjectName { get; set; }
        public string Status { get; set; }
        public List<CPTAndDxInfo> CPTDxInfo { get; set; }
    }

    public class SearchResultDTO
    {
        public SearchResultDTO()
        {
            CPTDxInfo = new List<CPTAndDxInfo>();
            ProjectUser = new List<ProjUser>();
        }
        public string ClinicalCaseId { get; set; }
        public int ProjectId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MRN { get; set; }
        public string ProviderName { get; set; }
        public DateTime DoS { get; set; }
        public string Status { get; set; }
        public string ProjectName { get; set; }
        [Required(ErrorMessage = "Assign From User Email is required.")]
        public string AssignFromUserEmail { get; set; }

        [Required(ErrorMessage = "Assign To User Email is required.")]
        public string AssignToUserEmail { get; set; }
        public bool IsPriority { get; set; }
        public string IncludeBlocked { get; set; }
        public string Assigneduser { get; set; }
        public List<CPTAndDxInfo> CPTDxInfo { get; set; }
        public string PostedBy { get; set; }
        public string PostedDate { get; set; }
        public string CodedBy { get; set; }
        public string QABy { get; set; }
        public string ShadowQABy { get; set; }
        public List<ProjUser> ProjectUser { get; set; }

    }
    public class ProjUser
    {
        public int ProjectId { get; set; }
        public int RoleId { get; set; }
    }
    public class CPTAndDxInfo
    {
        public string CPTCodes { get; set; }
        public string DxCodes { get; set; }
        public int ClaimOrder { get; set; }
    }
}
