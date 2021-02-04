using System;

namespace UAB.DTO
{
    public class DashboardDTO
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int StatusID { get; set; }
        public int Cnt { get; set; }
        //public int AvailableCount { get; set; }
        //public int IncorrectCount { get; set; }
        //public int ApprovedCount { get; set; }
    }
}
