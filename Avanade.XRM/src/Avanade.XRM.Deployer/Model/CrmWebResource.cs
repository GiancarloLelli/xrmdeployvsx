using Microsoft.Xrm.Sdk;
using System;
using System.IO;

namespace Avanade.XRM.Deployer.Model
{
	public class CrmWebResource
	{
		private string m_fullName;

		public CrmWebResource(string prefix)
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
				return GetFileType();
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

		private int GetFileType()
		{
			var info = new FileInfo(File);
			int type = 99;

			switch (info.Extension.ToLower())
			{
				case ".htm":
				case ".html":
					type = 1;
					break;
				case ".css":
					type = 2;
					break;
				case ".js":
					type = 3;
					break;
				case ".xml":
					type = 4;
					break;
				case ".png":
					type = 5;
					break;
				case ".jpg":
					type = 6;
					break;
				case ".gif":
					type = 7;
					break;
				case ".xap":
					type = 8;
					break;
				case ".xsl":
				case ".xslt":
					type = 9;
					break;
				case ".ico":
					type = 10;
					break;
			}

			return type;
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
