using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XRM.Telemetry
{
    public class TelemetryWrapper
    {
        private static volatile TelemetryClient m_client;
        private static object m_sync = new object();

        public static TelemetryClient Instance
        {
            get
            {
                if (m_client == null)
                {
                    lock (m_sync)
                    {
                        if (m_client == null)
                        {
                            var config = TelemetryConfiguration.CreateDefault();
                            config.InstrumentationKey = "bb1f7c2e-10ad-42ff-8bbf-e9d02846cb5f";
                            config.DisableTelemetry = Debugger.IsAttached;
                            m_client = new TelemetryClient(config);
                        }
                    }
                }

                return m_client;
            }
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
