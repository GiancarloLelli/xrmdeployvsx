using Microsoft.Xrm.Sdk;
using System;
using System.IO;

namespace XRM.Deploy.Core.Models
{
	internal class WebResourceModel
	{
		private string m_fullName;

		internal WebResourceModel(string prefix)
		{
			Prefix = prefix;
		}

		internal string File { get; set; }
		internal string ChangeType { get; set; }
		internal string DisplayName { get; set; }
		internal string Prefix { get; set; }

		internal string FileStream
		{
			get
			{
				return GetEncodedFileStream();
			}
		}

		internal int FileType
		{
			get
			{
				return GetFileType();
			}
		}

		internal bool Deleting
		{
			get
			{
				return ChangeType.ToLower().Equals("delete");
			}
		}

		internal string FullName
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

		internal Entity ToEntity()
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
