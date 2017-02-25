using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using XRM.Deploy.Core.Helpers;

namespace XRM.Deploy.Core.Models
{
    internal class WebResourceModel
    {
        private string m_fullName;

        public WebResourceModel(string prefix)
        {
            Prefix = prefix;
        }

        public string File { get; set; }

        public string ChangeType { get; set; }

        public string DisplayName { get; set; }

        public string Prefix { get; set; }

        public string FileStream
        {
            get
            {
                return GetEncodedFileStream();
            }
        }

        public int FileType
        {
            get
            {
                return CrmExtensionsHelper.GetCrmExtensionNumber(File);
            }
        }

        public bool Deleting
        {
            get
            {
                return ChangeType.ToLower().Equals("delete");
            }
        }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(m_fullName))
                {
                    m_fullName = GetFullName();
                }

                return m_fullName;
            }
            set
            {
                m_fullName = value;
            }
        }

        private string GetFullName()
        {
            var name = string.Empty;
            var index = File.IndexOf(Prefix);
            if (index != -1) name = File.Substring(index).Replace("\\", "/");
            return name;
        }

        private string GetEncodedFileStream()
        {
            using (var fs = new FileStream(File, FileMode.Open, FileAccess.Read))
            {
                byte[] binaryData = new byte[fs.Length];
                long bytesRead = fs.Read(binaryData, 0, (int)fs.Length);
                return Convert.ToBase64String(binaryData, 0, binaryData.Length);
            }
        }

        public Entity ToEntity()
        {
            Entity webResource = null;

            if (!Deleting)
            {
                webResource = new Entity("webresource");
                webResource["content"] = FileStream;
                webResource["displayname"] = FullName;
                webResource["description"] = FullName;
                webResource["name"] = FullName;
                webResource["webresourcetype"] = new OptionSetValue(FileType);
            }

            return webResource;
        }
    }
}
