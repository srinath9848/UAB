using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.Models
{
    public class ProviderPosted
    {
        public int ProviderPostedId { get; set; }
        public string ProviderName { get; set; }
        public DateTime PostingDate { get; set; }
        public string CoderComment { get; set; }
        public int ClinicalCaseId { get; set; }
        public int ProviderId { get; set; }
    }
}
