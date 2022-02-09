using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Winget.Pusher.Core.Models.Database
{
    [Table("norm_publishers")]
    public class PublishersMSIXTable
    {
        [Key]
        public long rowid { get; set; }
        public string norm_publisher { get; set; }
    }
}
