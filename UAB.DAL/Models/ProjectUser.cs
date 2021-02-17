using System;
using System.Collections.Generic;

namespace UAB.DAL.Models
{
    public partial class ProjectUser
    {
        public int ProjectUserId { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
        public int RoleId { get; set; }
        public bool? IsActive { get; set; }
    }
}
