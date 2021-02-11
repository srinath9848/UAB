using System;
using System.Collections.Generic;
using System.Text;

namespace UAB.DAL.LoginDTO
{
    public class Auth
    {
        public string access_token { get; set; }
        public DateTime expires_in { get; set; }
        public string Role { get; set; }
        public static string CurrentRole { get; set; }
        public string userName { get; set; }
        public static bool isAuth { get; set; }
        public static string AccessToken;
        public static string CurrentUserName { get; set; }
        public bool isAuthenticated { get; set; }
    }
}
