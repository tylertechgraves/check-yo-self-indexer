using System;
using check_yo_self_indexer.Server.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenSearch.Client;

namespace check_yo_self_indexer.Server.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomizedMvc(this IServiceCollection services)
    {
        services.AddMvc(options =>
        {
            options.Filters.Add(typeof(ModelValidationFilter));
        })
        .AddNewtonsoftJson(options =>
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

    public static IServiceCollection AddOpenSearchClient(this IServiceCollection services)
    {
        return services.AddSingleton<IOpenSearchClient, OpenSearchClient>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();

            var node = new Uri(configuration.GetValue<string>("Elasticsearch:Uri"));
            var useAuth = configuration.GetValue<bool>("Elasticsearch:UseAuthentication");
            var settings = new ConnectionSettings(node);
            var username = configuration.GetValue<string>("Elasticsearch:Username");
            var password = configuration.GetValue<string>("Elasticsearch:Password");

            if (useAuth)
            {
                settings.BasicAuthentication(username, password);
            }

            return new OpenSearchClient(settings);
        });
    }
}
