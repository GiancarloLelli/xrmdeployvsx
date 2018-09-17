using XRM.Deploy.Vsix.Models;

namespace XRM.Deploy.Vsix.Extensions
{
    public static class DeployConfigurationModelFacadeExtension
    {
        public static bool AreCustomFieldsValid(this DeployConfigurationModelFacade model, out string errorContainer)
        {
            var validConfiguration = true;
            errorContainer = string.Empty;

            if (model.IsOnline)
            {
                validConfiguration = !string.IsNullOrEmpty(model.User) && !string.IsNullOrEmpty(model.Pat);
                if (!validConfiguration)
                {
                    errorContainer = "You must specify you PAT and user to connect to a VSTS instance.";
                }
            }

            return validConfiguration;
        }
    }
}
