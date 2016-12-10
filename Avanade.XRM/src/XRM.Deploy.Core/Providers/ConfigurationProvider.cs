using System;
using System.Configuration;

namespace XRM.Deploy.Core.Providers
{
    [Obsolete("This class is obsolete. It has been replaced by a per-project configuration.", true)]
    internal class ConfigurationProvider
    {
        internal static string GetConnectionString(string key)
        {
            string value = string.Empty;

            if (ConfigurationManager.ConnectionStrings[key] != null)
            {
                value = ConfigurationManager.ConnectionStrings[key].ConnectionString;
            }

            return value;
        }

        internal static string GetAppSettingsValue(string key)
        {
            string value = string.Empty;

            if (ConfigurationManager.AppSettings[key] != null)
            {
                value = ConfigurationManager.AppSettings[key];
            }

            return value;
        }
    }
}
