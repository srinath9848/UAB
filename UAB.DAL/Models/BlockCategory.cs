using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace UAB.DAL.Models
{
    public partial class BlockCategory
    {
        public int BlockCategoryId  { get; set; }
        public string Name { get; set; }

        public string BlockType { get; set; } 
    }
}
