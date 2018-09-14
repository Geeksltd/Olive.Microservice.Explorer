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

    public class Startup : Olive.Mvc.Startup
    {
        public Startup(IHostingEnvironment env, IConfiguration config) : base(env, config) { }

        protected override CultureInfo GetRequestCulture() => new CultureInfo("en-GB");

        protected override void ConfigureAuthCookie(CookieAuthenticationOptions options)
        {
            base.ConfigureAuthCookie(options);

            if (Environment.IsProduction())
                options.DataProtectionProvider = new Olive.Security.Aws.KmsDataProtectionProvider();
            else options.DataProtectionProvider = new SymmetricKeyDataProtector("Auth");
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            if (Environment.IsProduction()) services.AddAwsIdentity();

            base.ConfigureServices(services);
            services.AddScheduledTasks();
            services.AddSwagger();
        }

        protected override void ConfigureSecurity(IApplicationBuilder app)
        {
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowCredentials().AllowAnyMethod());
            base.ConfigureSecurity(app);
        }

        public override void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment()) app.UseWebTest(config => config.AddTasks().AddClearApiCache());
            base.Configure(app);

            app.ConfigureSwagger();

            Console.Title = Microservice.Me.Name;

            if (Config.Get<bool>("Automated.Tasks:Enabled"))
                app.UseScheduledTasks(TaskManager.Run);
        }

        public override async Task OnStartUpAsync(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
                await app.InitializeTempDatabase<SqlServerManager>(() => ReferenceData.Create());

            // Add any other initialization logic that needs the database to be ready here.
        }
    }
}