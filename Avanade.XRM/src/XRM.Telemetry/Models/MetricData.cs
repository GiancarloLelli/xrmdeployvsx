namespace XRM.Telemetry.Models
{
    public class MetricData
    {
        public MetricData(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public string Value { get; set; }
    }
}
