using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using XRM.Telemetry.Models;

namespace XRM.Telemetry
{
    public class TelemetryWrapper
    {
        public readonly TelemetryClient Instance;
        public readonly string VisualStudioVersion;

        public TelemetryWrapper(string visualStudioVersion)
        {
            var config = TelemetryConfiguration.CreateDefault();
            config.InstrumentationKey = "bb1f7c2e-10ad-42ff-8bbf-e9d02846cb5f";
            config.DisableTelemetry = Debugger.IsAttached;
            Instance = new TelemetryClient(config);
        }
    }

    public static class TelemetryWrapperExtension
    {
        public static void TrackExceptionWithCustomMetrics(this TelemetryWrapper client, Exception ex)
        {
            var metrics = new Dictionary<string, string>();
            metrics.Add("Username", Environment.UserName);
            metrics.Add("Machine Name", Environment.MachineName);
            metrics.Add("OS", Environment.OSVersion.ToString());
            metrics.Add("Visual Studio Version", client.VisualStudioVersion);
            client.Instance.TrackException(ex, metrics);
        }

        public static void TrackCustomEventWithCustomMetrics(this TelemetryWrapper client, string eventName, MetricData data)
        {
            var eventData = new EventTelemetry(eventName);
            eventData.Timestamp = DateTime.UtcNow;
            eventData.Properties.Add(data.Key, data.Value);
            client.Instance.TrackEvent(eventData);
        }
    }
}
