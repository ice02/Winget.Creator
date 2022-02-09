using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Winget.Pusher.Core.Models.Database
{
    [Table("norm_publishers_map")]
    public class PublishersMapMSIXTable
    {
        public long manifest { get; set; }
        public long norm_publisher { get; set; }

    }
}
