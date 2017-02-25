using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Xrm.Deploy.Vsix.Helpers
{
    internal class XmlObjectsHelper
    {
        public static T Deserialize<T>(string path) where T : class
        {
            T result = default(T);
            XmlSerializer serializer = new XmlSerializer(typeof(T));

            using (FileStream fs = File.OpenRead(path))
            {
                using (StreamReader inputReader = new StreamReader(fs))
                {
                    using (XmlReader reader = XmlReader.Create(inputReader))
                    {
                        bool canSerialize = serializer.CanDeserialize(reader);
                        if (canSerialize) result = serializer.Deserialize(reader) as T;
                    }
                }
            }

            return result;
        }

        public static string Serialize<T>(T target) where T : class
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (var stringWriter = new StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                {
                    serializer.Serialize(xmlWriter, target);
                    return stringWriter.ToString();
                }
            }
        }
    }
}
