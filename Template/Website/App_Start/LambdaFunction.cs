using Microsoft.AspNetCore.Hosting;
using Olive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Website
{
    public class LambdaFunction : Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            var start = LocalTime.Now;
            RegisterResponseContentEncodingForContentType("font/woff2", Amazon.Lambda.AspNetCoreServer.ResponseContentEncoding.Base64);
            builder.UseStartup(typeof(Startup).Assembly.FullName);
        }
    }
}

