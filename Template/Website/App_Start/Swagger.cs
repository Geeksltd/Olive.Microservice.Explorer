using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace Website
{
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            return services.AddSwaggerGen(c =>
             {
                 c.SwaggerDoc("v1", new Info { Title = "MY.MICROSERVICE.NAME", Version = "v1" });
                 c.DocInclusionPredicate((docName, apiDesc) => apiDesc.HttpMethod != null);
             });
        }

        public static void ConfigureSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger().UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"));
        }
    }
}
