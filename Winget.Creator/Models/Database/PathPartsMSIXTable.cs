using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Winget.Pusher.Core.Models.Database
{
    [Table("pathparts")]
    public class PathPartsMSIXTable
    {
        [Key]
        public long rowid { get; set; }
        public long parent { get; set; }
        public string pathpart { get; set; }

    }
}
