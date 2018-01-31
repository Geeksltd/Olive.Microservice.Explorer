namespace Website
{
    using System.Globalization;
    using Domain;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.DataProtection;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Olive;
    using Olive.Security;
    using Olive.Email;
    using Olive.Hangfire;
    using Olive.Mvc.Testing;

    public class Startup : Olive.Mvc.Startup
    {
        protected override CultureInfo GetRequestCulture() => new CultureInfo("en-GB");

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);
            services.AddScheduledTasks();
        }

        public override void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(x => x.AllowAnyOrigin().AllowAnyHeader());

            app.UseWebTest(ReferenceData.Create, config => config.AddTasks().AddEmail());

            base.Configure(app, env);

            if (Config.Get<bool>("Automated.Tasks:Enabled"))
                app.UseScheduledTasks(TaskManager.Run);
        }

        protected override void ConfigureApplicationCookie(CookieAuthenticationOptions options)
        {
            base.ConfigureApplicationCookie(options);

            options.Cookie.Domain = "." + Microservice.RootDomain;
            options.DataProtectionProvider = new SymmetricKeyDataProtector(Microservice.RootDomain);
        }
    }
}