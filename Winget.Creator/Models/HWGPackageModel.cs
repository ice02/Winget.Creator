using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winget.Creator.Models
{
    public class HWGPackageModel
    {
        public string PackageId { get; set; }
        public string Name { get; set; }
        public string Publisher { get; set; }
        public string YamlUri { get; set; }
        public string ProductCode { get; set; }
        public PackageVersion PackageVersion { get; set; }
        public List<PackageVersion> Versions { get; set; }
    }

    public class PackageVersion
    {
        public string Version { get; set; }
        public string YamlUri { get; set; }
    }
}
