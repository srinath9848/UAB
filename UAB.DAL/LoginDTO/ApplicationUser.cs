using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.LoginDTO
{
    public class ApplicationUser
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; }
        public bool IsActive { get; set; }
        [Required(ErrorMessage = "Role is required.")]
        public int RoleId { get; set; } 
        public string RoleName  { get; set; }
        [Required(ErrorMessage = "Project is required.")]
        public int ProjectId { get; set; } 
        public string ProjectName  { get; set; }
        public int ProjectUserId { get; set; }

        public string hdnProjectAndRole { get; set; } 
    }
}
