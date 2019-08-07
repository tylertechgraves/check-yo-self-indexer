using check_yo_self_indexer.Server.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace check_yo_self_indexer.Server.Extensions
{
    public static class ServiceCollectionExtensions
    {       
        public static IServiceCollection AddCustomizedMvc(this IServiceCollection services)
        {
            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ModelValidationFilter));
            })
            .AddJsonOptions(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            return services;
        }        

        public static IServiceCollection RegisterCustomServices(this IServiceCollection services)
        {
            services.AddScoped<ApiExceptionFilter>();
            return services;
        }
    }
}
