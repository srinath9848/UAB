using System;
using System.Collections.Generic;

namespace UAB.DAL.LoginDTO
{
    public partial class HistoricPasswords
    {
        public int HistoricPasswordsId { get; set; }
        public int? UserId { get; set; }
        public int? Version { get; set; }
        public string Data { get; set; }
        public DateTime? CreateDate { get; set; }
        public string UserCode { get; set; }

        public virtual Users User { get; set; }
    }
}
