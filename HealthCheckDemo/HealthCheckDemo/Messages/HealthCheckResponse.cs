namespace HealthCheckDemo.Messages
{
    public class HealthCheckResponse
    {
        public string Status { get; set; }
        public string TotalDuration { get; set; }
        public DependencyService[] DependencyServices { get; set; }

    }

    public class DependencyService
    {
        public string Key { get; set; }
        public string Status { get; set; }
        public string Duration { get; set; }
        public string Tags { get; set; }
        public string Exception { get; set; }
    }
}
