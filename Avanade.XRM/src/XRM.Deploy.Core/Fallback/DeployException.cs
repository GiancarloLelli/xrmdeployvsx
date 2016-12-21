using System;

namespace XRM.Deploy.Core.Fallback
{
    public class DeployException : Exception
    {
        public DeployException(string message) : base(message)
        {
        }
    }
}
