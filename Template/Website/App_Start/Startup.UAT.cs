using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Olive;

namespace Website
{
    public class StartupUAT : Startup
    {
        public StartupUAT(IHostingEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
        {
        }

        protected override void SetUpIdentity(IHostingEnvironment env, IConfiguration config)
        {
            config.LoadAwsIdentity();
        }
    }
}