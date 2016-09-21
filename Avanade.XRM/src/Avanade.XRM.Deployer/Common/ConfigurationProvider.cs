using System;
using System.Configuration;

namespace Avanade.XRM.Deployer.Common
{
    public class ConfigurationProvider
    {
        public static string GetConnectionString(string key)
        {
            string value = string.Empty;

            try
            {
                if (ConfigurationManager.ConnectionStrings[key] != null)
                {
                    value = ConfigurationManager.ConnectionStrings[key].ConnectionString;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return value;
        }

        public static string GetAppSettingsValue(string key)
        {
            string value = string.Empty;

            try
            {
                if (ConfigurationManager.AppSettings[key] != null)
                {
                    value = ConfigurationManager.AppSettings[key];
                }
            }
            catch (Exception)
            {
                throw;
            }

            return value;
        }
    }
}
