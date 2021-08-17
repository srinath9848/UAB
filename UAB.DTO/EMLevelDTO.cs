using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DTO
{
    public class EMLevelDTO
    {
       
        public int EMLevelId { get; set; } 
        public string EMCode { get; set; }
        public int EMLevel { get; set; }
        public int ProjectId  { get; set; }
        public string ProjectName  { get; set; }

    }
}
