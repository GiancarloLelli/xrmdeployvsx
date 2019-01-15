using CRMDevLabs.Toolkit.Models.Telemetry;
using Microsoft.Win32;

namespace CRMDevLabs.Toolkit.Telemetry
{
    public class RegistryKeyReader
    {
        public static VisualStudioUserInfo GetUserInfo()
        {
            VisualStudioUserInfo info = null;

            try
            {
                var subKey = "Software\\Microsoft\\VSCommon\\ConnectedUser\\IdeUser\\Cache";
                var emailAddressKeyName = "EmailAddress";
                var userNameKeyName = "DisplayName";
                var profileKeyName = "ProfileUri";

                var root = Registry.CurrentUser;
                var sk = root.OpenSubKey(subKey);

                if (sk != null)
                {
                    var email = sk.GetValue(emailAddressKeyName) as string;
                    var user = sk.GetValue(userNameKeyName) as string;
                    var profile = sk.GetValue(profileKeyName) as string;

                    info = new VisualStudioUserInfo
                    {
                        Email = email,
                        Name = user,
                        Url = profile
                    };
                }
            }
            catch (System.Exception)
            {

            }

            return info;
        }
    }
}
