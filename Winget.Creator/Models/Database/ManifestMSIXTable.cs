using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Winget.Pusher.Core.Models.Database
{
    [Table("manifest")]
    public class ManifestMSIXTable
    {
        [Key]
        public long rowid { get; set; }
        public long id { get; set; }
        public long name { get; set; }
        public long moniker { get; set; }
        public long version { get; set; }
        public long channel { get; set; }
        public long pathpart { get; set; }
        public byte[] hash { get; set; }
    }
}
