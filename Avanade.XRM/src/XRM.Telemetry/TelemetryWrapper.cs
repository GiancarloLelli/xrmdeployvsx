using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XRM.Telemetry
{
    public class TelemetryWrapper
    {
        public readonly TelemetryClient Instance;

        public TelemetryWrapper()
        {
            var config = TelemetryConfiguration.CreateDefault();
            config.InstrumentationKey = "bb1f7c2e-10ad-42ff-8bbf-e9d02846cb5f";
            config.DisableTelemetry = Debugger.IsAttached;
            Instance = new TelemetryClient(config);
        }
    }

    public static class TelemetryWrapperExtension
    {
        public static void TrackExceptionWithCustomMetrics(this TelemetryClient client, Exception ex, string visualStudioVersion = "Not Available")
        {
            var metrics = new Dictionary<string, string>();
            metrics.Add("Username", Environment.UserName);
            metrics.Add("OS", Environment.OSVersion.ToString());
            metrics.Add("VS Version", visualStudioVersion);
            client.TrackException(ex, metrics);
        }
    }
}
