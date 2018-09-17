using System;

namespace CRMDevLabs.Toolkit.Models.Telemetry
{
    public class ToolkitException : Exception
    {
        public ToolkitException(string message) : base(message) { }
    }
}
