namespace Website
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
    using System.Threading.Tasks;
    using Olive.Entities.Data;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.AspNetCore.Authentication;

    public abstract class Startup : Olive.Mvc.Startup
    {
        protected Startup(IHostingEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
        {
            SetUpIdentity();
        }

        protected abstract void SetUpIdentity();

        protected virtual bool IsProduction() => false;

        protected virtual void ConfigureScheduledTasks(IServiceCollection services) => services.AddScheduledTasks();

        protected override CultureInfo GetRequestCulture() => new CultureInfo("en-GB");

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddDataAccess(x => x.SqlServer());
        }

        protected override void ConfigureExceptionPage(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage(); // even in production
        }

        protected override void ConfigureAuthCookie(CookieAuthenticationOptions options)
        {
            base.ConfigureAuthCookie(options);
            options.Cookie.Domain = Configuration["Authentication:Cookie:Domain"];
        }
    }
}