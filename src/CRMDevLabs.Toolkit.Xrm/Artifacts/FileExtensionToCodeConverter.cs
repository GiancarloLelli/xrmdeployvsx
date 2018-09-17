using System.IO;

namespace CRMDevLabs.Toolkit.Xrm.Artifacts
{
    public class FileExtensionToCodeConverter
    {
        public static int Convert(string file)
        {
            var info = new FileInfo(file);
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
    }
}
