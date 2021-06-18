using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UAB.DAL.Models
{
    public partial class List
    {
        public List()
        {
            ClinicalCase = new HashSet<ClinicalCase>();
        }
        [Required(ErrorMessage = "ListId  is required.")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Use Numbers only please")]
        public long  ListId { get; set; }
        [Required(ErrorMessage = "List Name is required.")]
        public string Name { get; set; }

        public virtual ICollection<ClinicalCase> ClinicalCase { get; set; }
    }
}
