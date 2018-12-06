namespace Website
{
    using Domain;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Olive;
    using Olive.Entities.Data;
    using Olive.Hangfire;
    using Olive.Mvc.Testing;
    using Olive.Security;
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    public class Startup : Olive.Mvc.Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
           : base(env, config, loggerFactory)
        {
            if (env.IsProduction()) config.LoadAwsIdentity();
            else config.LoadAwsDevIdentity();
        }

        protected override CultureInfo GetRequestCulture() => new CultureInfo("en-GB");

        protected override void ConfigureAuthCookie(CookieAuthenticationOptions options)
        {
            base.ConfigureAuthCookie(options);

            if (Environment.IsProduction())
                options.DataProtectionProvider = new Olive.Security.Aws.KmsDataProtectionProvider();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddScheduledTasks();
            services.AddSwagger();

            if (Environment.IsDevelopment())
                services.AddDevCommands(x => x
                .AddTempDatabase<SqlServerManager, ReferenceData>()
                 .AddClearApiCache());
        }

        protected override void ConfigureSecurity(IApplicationBuilder app)
        {
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowCredentials().AllowAnyMethod());
            base.ConfigureSecurity(app);
        }

        public override void Configure(IApplicationBuilder app)
        {
            base.Configure(app);
            app.ConfigureSwagger();
            Console.Title = Microservice.Me.Name;
        }

        public override async Task OnStartUpAsync(IApplicationBuilder app)
        {
            await base.OnStartUpAsync(app);
            app.UseScheduledTasks<TaskManager>();
        }

        #region Show error screen even in production?
        // Uncomment the following:
        // protected override void ConfigureExceptionPage(IApplicationBuilder app) 
        //    => app.UseDeveloperExceptionPage();
        #endregion
    }
}