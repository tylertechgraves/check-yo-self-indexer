using check_yo_self_indexer.Entities.Config;
using check_yo_self_indexer.Entities.Startup;
using check_yo_self_indexer.Server;
using check_yo_self_indexer.Server.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace check_yo_self_indexer;

public class Startup
{
    static readonly string _title = "check-yo-self-indexer";

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<AppConfig>(Configuration)
            .AddOptions()
            .AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                builder => builder.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod());
            })
            .AddResponseCompression(options =>
            {
                options.MimeTypes = DefaultMimeTypes.Get;
            })
            .AddMemoryCache()
            .RegisterCustomServices()
            .AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN")
            .AddCustomizedMvc()
            .AddSwaggerDocument(config =>
            {
                config.Title = _title;
            })
            .AddSingleton<IHttpContextAccessor, HttpContextAccessor>()
            .AddHealthChecks();

        // Add custom DI
        services.AddOpenSearchClient();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.AddDevMiddlewares();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
            app.UseResponseCompression();
        }

        app.UseXsrf()
           .UseCors("AllowAll")
           .UseStaticFiles()
           .UseRouting()
           .UseAuthorization()
           .UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                // add fallback for api routes
                endpoints.Map("/api/{**route}", request => { request.Response.StatusCode = 404; return System.Threading.Tasks.Task.CompletedTask; });

                endpoints.MapHealthChecks("/health");
            })
           .UseAuthentication()
           // Enable middleware to serve generated Swagger as a JSON endpoint
           .UseOpenApi()
           .UseSwaggerUi3(); ;


        var httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
        Context.Configure(httpContextAccessor);
    }
}
