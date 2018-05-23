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

    public class Startup : Olive.Mvc.Startup
    {
        protected override CultureInfo GetRequestCulture() => new CultureInfo("en-GB");

        protected override void ConfigureApplicationCookie(CookieAuthenticationOptions options)
        {
            base.ConfigureApplicationCookie(options);
            options.DataProtectionProvider = new SymmetricKeyDataProtector("Auth");
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddScheduledTasks();
            services.AddSwagger();
        }

        protected override void ConfigureSecurity(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader().AllowCredentials().AllowAnyMethod());
            base.ConfigureSecurity(app, env);
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment()) app.UseWebTest(config => config.AddTasks().AddClearApiCache());
            base.Configure(app, env);

            app.ConfigureSwagger();

            Console.Title = Microservice.Me.Name;

            if (Config.Get<bool>("Automated.Tasks:Enabled"))
                app.UseScheduledTasks(TaskManager.Run);
        }

        public override async Task OnStartUpAsync(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                await app.InitializeTempDatabase<SqlServerManager>(() => ReferenceData.Create());

            // Add any other initialization logic that needs the database to be ready here.
        }
    }
}