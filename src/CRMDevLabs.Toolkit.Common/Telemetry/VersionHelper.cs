using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace CRMDevLabs.Toolkit.Common.Telemetry
{
    public class VersionHelper
    {
        public static string GetVersionFromManifest()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var assemblyUri = new UriBuilder(assembly.CodeBase);
            var assemblyPath = Uri.UnescapeDataString(assemblyUri.Path);
            var assemblyDirectory = Path.GetDirectoryName(assemblyPath);
            var vsixManifestPath = Path.Combine(assemblyDirectory, "extension.vsixmanifest");

            var doc = new XmlDocument();
            doc.Load(vsixManifestPath);
            var metaData = doc.DocumentElement.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Metadata");
            var identity = metaData.ChildNodes.Cast<XmlElement>().First(x => x.Name == "Identity");
            return identity.GetAttribute("Version");
        }
    }
}
