using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Olive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website
{
    public class StartupUAT : StartupProduction
    {
        public StartupUAT(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
        {

        }

        protected override bool IsProduction() => false;

        protected override void ConfigureMvc(IMvcBuilder mvc)
        {
            mvc.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
            base.ConfigureMvc(mvc);
        }
    }
} 