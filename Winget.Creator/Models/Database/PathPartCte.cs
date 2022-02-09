using System;
using System.Collections.Generic;
using System.Text;

namespace Winget.Pusher.Core.Models.Database
{
    public class PathPartCte
    {
        public long rowid { get; set; }
        public long? parent { get; set; }
        public string path { get; set; }
    }
}
