using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.Models
{
    public partial class BlockCategory
    {
        public int BlockCategoryId  { get; set; }
        [Required(ErrorMessage = "Block Category is required.")]
        [RegularExpression(@"^(([A-za-z0-9 ]+[\s]{1}[A-za-z0-9 ]+)|([A-Za-z0-9 ]+))$", ErrorMessage = "Special Character not allowed")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Block Type is required.")]
        public string BlockType { get; set; } 
    }
}
