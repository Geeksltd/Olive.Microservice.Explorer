﻿namespace Website
{
    using System.Globalization;
    using Domain;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Olive;
    using Olive.Security;
    using Olive.Hangfire;
    using Olive.Mvc.Testing;
    using System;

    public class Startup : Olive.Mvc.Startup
    {
        protected override CultureInfo GetRequestCulture() => new CultureInfo("en-GB");

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddScheduledTasks();
            services.AddSwagger();
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.ConfigureSwagger();
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowCredentials());
            app.UseWebTest(ReferenceData.Create, config => config.AddTasks());

            base.Configure(app, env);
            Console.Title = Microservice.Me.Name;

            if (Config.Get<bool>("Automated.Tasks:Enabled"))
                app.UseScheduledTasks(TaskManager.Run);
        }

        protected override void ConfigureApplicationCookie(CookieAuthenticationOptions options)
        {
            base.ConfigureApplicationCookie(options);
            options.DataProtectionProvider = new SymmetricKeyDataProtector("Auth");
        }
    }
}