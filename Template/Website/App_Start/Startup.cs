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
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    public class Startup : Olive.Mvc.Microservices.Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration config, ILoggerFactory loggerFactory)
           : base(env, config, loggerFactory)
        {
            SetUpIdentity(env, config);
        }

        protected virtual void SetUpIdentity(IHostingEnvironment env, IConfiguration config)
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
            services.AddDataAccess(x => x.SqlServer());
            services.AddScheduledTasks();
            services.AddSwagger();

            if (Environment.IsDevelopment())
            {
                services.AddDevCommands(x => x.AddTempDatabase<SqlServerManager, ReferenceData>().AddClearApiCache());
                services.AddIOEventBus();
            }
            else
                services.AddAwsEventBus();
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
