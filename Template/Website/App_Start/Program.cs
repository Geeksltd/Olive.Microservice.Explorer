namespace Website
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Olive;
    using Olive.Logging;
    using System;

    public class Program
    {
        public static void Main(string[] args) => CreateWebHostBuilder(args).Build().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureLogging(ConfigureLogging)
                .UseSetting("detailedErrors", "true").CaptureStartupErrors(true)
                .UseStartup(typeof(Startup).Assembly.FullName);
        }

        static void ConfigureLogging(WebHostBuilderContext context, ILoggingBuilder logging)
        {
            // You can customise logging here
            if (context.HostingEnvironment.IsProduction())
                logging.AddEventBus();
        }
    }
}