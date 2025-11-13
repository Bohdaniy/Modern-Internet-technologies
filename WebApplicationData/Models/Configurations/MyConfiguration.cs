namespace WebApplicationData.Models.Configurations
{
    public class MyConfiguration
    {
        public string? ApplicationName { get; set; }
        public string? ApiKey { get; set; }
        public ConnectionStrings? ConnectionStrings { get; set; }
        public CustomSettings? CustomSettings { get; set; }
    }

    public class ConnectionStrings
    {
        public string? DefaultConnection { get; set; }
    }

    public class CustomSettings
    {
        public string? Message { get; set; }
    }
}
