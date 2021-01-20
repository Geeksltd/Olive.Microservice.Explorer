using Domain;
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
        public StartupDevelopment(IWebHostEnvironment env, IConfiguration config, ILoggerFactory factory) :
            base(env, config, factory)
        {

        }
    }
}
