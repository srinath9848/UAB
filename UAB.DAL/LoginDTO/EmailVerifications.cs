using System;
using System.Collections.Generic;

namespace UAB.DAL.LoginDTO
{
    public partial class EmailVerifications
    {
        public int EmailVerificationsId { get; set; }
        public int? UserId { get; set; }
        public string Code { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? Strikes { get; set; }
        public string UserCode { get; set; }

        public virtual Users User { get; set; }
    }
}
