using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Olive;
using Olive.PassiveBackgroundTasks;
using System.Threading.Tasks;

namespace Website
{
    public class StartupProduction : Startup
    {
        public StartupProduction(IHostingEnvironment env, IConfiguration config, ILoggerFactory factory) : base(env, config, factory)
        {

        }

        protected override bool IsProduction() => true;

        protected override void SetUpIdentity()
        {
            Configuration.LoadAwsSecrets();
        }

        protected override void ConfigureAuthCookie(CookieAuthenticationOptions options)
        {
            base.ConfigureAuthCookie(options);
            options.DataProtectionProvider = new Olive.Security.Aws.KmsDataProtectionProvider();
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection().PersistKeysToAWSSystemsManager(Configuration["Aws:Secrets:Id"].WithPrefix("/"));
            base.ConfigureServices(services);
            services.AddS3BlobStorageProvider();
            services.AddAwsEventBus();
        }

        protected override void ConfigureRequestHandlers(IApplicationBuilder app)
        {
            Task.Factory.RunSync(() => app.UseScheduledTasks<TaskManager>());

            base.ConfigureRequestHandlers(app);

        }

        protected override void ConfigureScheduledTasks(IServiceCollection services)
        {
            services.AddScheduledTasks<Domain.BackgroundTask>();
        }

        protected override void ConfigureMvc(IMvcBuilder mvc)
        {
            mvc.SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
            base.ConfigureMvc(mvc);
        }
    }
}
