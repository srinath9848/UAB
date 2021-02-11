using System;
using System.Collections.Generic;

namespace UAB.DAL.LoginDTO
{
    public partial class Attempts
    {
        public int AttemptsId { get; set; }
        public int? UserId { get; set; }
        public DateTime? Timestamp { get; set; }
        public string Email { get; set; }
        public string Action { get; set; }
        public string Result { get; set; }
        public string UserCode { get; set; }

        public virtual Users User { get; set; }
    }
}
