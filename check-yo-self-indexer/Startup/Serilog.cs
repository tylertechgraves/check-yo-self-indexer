using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace check_yo_self_indexer.Server.Startup;

public static class Serilog
{
    public static void Setup(WebHostBuilderContext hostingContext)
    {
        var log = new LoggerConfiguration().ReadFrom.Configuration(hostingContext.Configuration);
        Log.Logger = log.CreateLogger();
    }
}
