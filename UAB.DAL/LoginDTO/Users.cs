using System;
using System.Collections.Generic;

namespace UAB.DAL.LoginDTO
{
    public partial class Users
    {
        public Users()
        {
            Attempts = new HashSet<Attempts>();
            EmailVerifications = new HashSet<EmailVerifications>();
            HistoricPasswords = new HashSet<HistoricPasswords>();
        }

        public int UsersId { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public DateTime? InviteExpiration { get; set; }
        public int? Version { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public int? Strikes { get; set; }
        public DateTime? StrikeValue1 { get; set; }
        public DateTime? StrikeValue2 { get; set; }
        public DateTime? StrikeValue3 { get; set; }
        public DateTime? StrikeValue4 { get; set; }
        public DateTime? StrikeValue5 { get; set; }
        public DateTime? LockoutStart { get; set; }
        public DateTime? LockoutFinish { get; set; }
        public bool? HasValidatedEmail { get; set; }

        public virtual ICollection<Attempts> Attempts { get; set; }
        public virtual ICollection<EmailVerifications> EmailVerifications { get; set; }
        public virtual ICollection<HistoricPasswords> HistoricPasswords { get; set; }
    }
}
