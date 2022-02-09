using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Winget.Pusher.Core.Models.Database
{
    [Table("metadata")]
    public class MetadataMSIXTable
    {
        public string name { get; set; }
        public string value { get; set; }

    }
}
