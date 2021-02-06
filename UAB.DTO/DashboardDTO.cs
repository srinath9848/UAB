using System;

namespace UAB.DTO
{
    public class DashboardDTO
    {
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int StatusID { get; set; }
        public int Cnt { get; set; }
        public int AvailableCharts { get; set; }
        public int RebuttalCharts { get; set; }
        public int ReadyForPostingCharts { get; set; }
    }
}
