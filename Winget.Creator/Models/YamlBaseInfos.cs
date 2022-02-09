using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winget.Creator.Models
{
    public class YamlBaseInfos
    {
        public string PackageIdentifier { get; set; }
        public string PackageVersion { get; set; }
        public string DefaultLocale { get; set; }
        public string ManifestType { get; set; }
        public string ManifestVersion { get; set; }

    }
}
