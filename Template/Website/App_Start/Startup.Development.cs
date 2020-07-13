using Domain;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Olive;
using Olive.Entities.Data;
using Olive.Mvc.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website
{
    public class StartupDevelopment : Startup
    {
        public StartupDevelopment(IHostingEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
        {

        }

        protected override void SetUpIdentity()
        {
            Configuration.LoadAwsDevIdentity();
        }


        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddDevCommands(x => x.AddTempDatabase<SqlServerManager, ReferenceData>().AddClearApiCache());
            services.AddIOEventBus();
        }
    }
}
