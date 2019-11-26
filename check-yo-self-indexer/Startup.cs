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
using Microsoft.Extensions.Logging;

namespace check_yo_self_indexer
{
    public class Startup
    {
        static string title = "check-yo-self-indexer";

        public Startup(IConfiguration configuration, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            Configuration = configuration;
            _env = env;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }
        private IWebHostEnvironment _env { get; set; }
        private readonly ILogger<Startup> _logger;

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
                .AddSwaggerDocument(config => {
                    config.Title = title;
                })
                .AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

                // .AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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
               .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}"
                    );
                    // default route for MVC/API controllers
                    // endpoints.MapRoute(
                    //     name: "default",
                    //     template: "{controller=Home}/{action=Index}/{id?}");

                    // // fallback route for anything that does not match an MVC/API controller
                    // // this will load the angular app and allow for the angular routes to work.
                    // routes.MapSpaFallbackRoute(
                    //     name: "spa-fallback",
                    //     defaults: new { controller = "Home", action = "Index" });
                })
               .UseAuthentication()
               // Enable middleware to serve generated Swagger as a JSON endpoint
               .UseOpenApi();

            
            IHttpContextAccessor httpContextAccessor = app.ApplicationServices.GetRequiredService<IHttpContextAccessor>();
            Context.Configure(httpContextAccessor);
        }
    }
}
