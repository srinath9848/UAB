using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace UAB.DAL.Models
{
    public partial class Provider
    {
        public int ProviderId { get; set; }
        [Required(ErrorMessage = "Provider is required.")]
        public string Name { get; set; }
    }
}
