using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
    public class ApplicationUser
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public int RoleId { get; set; } 
        public string RoleName  { get; set; }
        public int ProjectId { get; set; } 
        public string ProjectName  { get; set; }
        public int ProjectUserId { get; set; } 
    }
}
