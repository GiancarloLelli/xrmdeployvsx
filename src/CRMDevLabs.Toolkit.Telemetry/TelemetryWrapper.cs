using CRMDevLabs.Toolkit.Models.Telemetry;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CRMDevLabs.Toolkit.Telemetry
{
    public class TelemetryWrapper
    {
        public readonly TelemetryClient Instance;
        public readonly string VisualStudioVersion;
        public readonly string VsxVersion;
        public readonly VisualStudioUserInfo UserInfo;

        public TelemetryWrapper(string visualStudioVersion, string vsxVersion)
        {
            UserInfo = RegistryKeyReader.GetUserInfo();
            VisualStudioVersion = visualStudioVersion;
            VsxVersion = vsxVersion;

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
            if (Debugger.IsAttached)
                return;

            var metrics = Metrics(client.UserInfo);

            if (client != null)
            {
                metrics.Add("Visual Studio Version", client.VisualStudioVersion);
                metrics.Add("Toolkit Version", client.VsxVersion);
            }

            client.Instance.TrackException(ex, metrics);
        }

        public static void TrackCustomEventWithCustomMetrics(this TelemetryWrapper client, string eventName, MetricData data)
        {
            if (Debugger.IsAttached)
                return;

            var metrics = Metrics(client.UserInfo);
            var eventData = new EventTelemetry(eventName);
            eventData.Timestamp = DateTime.UtcNow;
            eventData.Properties.Add(data.Key, data.Value);

            foreach (var itemMetric in metrics)
            {
                eventData.Properties.Add(itemMetric.Key, itemMetric.Value);
            }

            if (client != null)
            {
                eventData.Properties.Add("Visual Studio Version", client.VisualStudioVersion);
                eventData.Properties.Add("Toolkit Version", client.VsxVersion);
            }

            client.Instance.TrackEvent(eventData);
        }

        private static Dictionary<string, string> Metrics(VisualStudioUserInfo info)
        {
            var metrics = new Dictionary<string, string>();

            metrics.Add("Username", Environment.UserName);
            metrics.Add("Domain Name", Environment.UserDomainName);
            metrics.Add("Machine Name", Environment.MachineName);
            metrics.Add("OS", Environment.OSVersion.ToString());
            metrics.Add("Version", Environment.Version.ToString());
            metrics.Add("x64", Environment.Is64BitProcess.ToString());

            if (info != null)
            {
                metrics.Add("VS User", info.Name);
                metrics.Add("VS User Email", info.Email);
                metrics.Add("VS Profile Link", info.Url);
            }

            return metrics;
        }
    }
}
