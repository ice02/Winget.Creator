using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Winget.Pusher.Core.Models.Database
{
    [Table("productcodes_map")]
    public class ProductCodesMapMSIXTable
    {
        public long manifest { get; set; }
        public long productcode { get; set; }
    }
}
