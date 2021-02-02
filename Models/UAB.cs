using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UAB.Models
{
    public class UABDashboardDetails
    {
        public string ProjectName { get; set; }
        public int AvailableCount { get; set; }
        public int IncorrectCount { get; set; }
        public int ApprovedCount { get; set; }
    }
    public class QADetails
    {
        public string name { get; set; }
    }
}
