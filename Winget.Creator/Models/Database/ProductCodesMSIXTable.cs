using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Winget.Pusher.Core.Models.Database
{
    [Table("productcodes")]
    public class ProductCodesMSIXTable
    {
        [Key]
        public long rowid { get; set; }
        public string productcode { get; set; }

    }
}
