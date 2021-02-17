using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
    public class Auth
    {
        public static string CurrentRole { get; set; }
        public static bool IsAuth { get; set; }
        public static string AccessToken;
        public static string EmailId { get; set; }
        public static int UserId { get; set; }
    }
}
