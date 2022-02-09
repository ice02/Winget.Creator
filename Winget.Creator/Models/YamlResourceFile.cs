using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Winget.Creator.Models
{
    public class YamlResourceFile
    {
        public string PackageIdentifier { get; set; }
        public string PackageVersion { get; set; }
        public string PackageLocale { get; set; }
        public string Publisher { get; set; }
        public string PublisherUrl { get; set; }
        public string PublisherSupportUrl { get; set; }
        public string PrivacyUrl { get; set; }
        public string Author { get; set; }
        public string PackageName { get; set; }
        public string PackageUrl { get; set; }
        public string License { get; set; }
        public string LicenseUrl { get; set; }
        public string Copyright { get; set; }
        public string CopyrightUrl { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Moniker { get; set; }
        public List<string> Tags { get; set; }
        public string Agreements { get; set; }
        public string ReleaseNotes { get; set; }
        public string ReleaseNotesUrl { get; set; }
        public string ManifestType { get; set; }
        public string ManifestVersion { get; set; }
    }
}
