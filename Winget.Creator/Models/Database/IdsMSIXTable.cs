using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Winget.Pusher.Core.Models.Database
{
    [Table("ids")]
    public class IdsMSIXTable
    {
        [Key]
        public long rowid { get; set; }
        public string id { get; set; }
    }
}
