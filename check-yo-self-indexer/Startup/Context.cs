using System;
using Microsoft.AspNetCore.Http;

namespace check_yo_self_indexer.Server;

public static class Context
{
    private static IHttpContextAccessor _httpContextAccessor;
    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private static Uri GetAbsoluteUri()
    {
        var request = _httpContextAccessor.HttpContext.Request;
        var uriBuilder = new UriBuilder
        {
            Scheme = request.Scheme,
            Host = request.Host.Host
        };
        if (request.Host.Port.HasValue)
        {
            uriBuilder.Port = request.Host.Port.Value;
        }
        uriBuilder.Path = request.Path;
        uriBuilder.Query = request.QueryString.ToUriComponent();
        return uriBuilder.Uri;
    }

    public static string GetHost()
    {
        var uri = GetAbsoluteUri();
        return uri.Scheme + "://" + uri.Host + ":" + uri.Port;
    }
    public static string GetAbsoluteUrl() { return GetAbsoluteUri().AbsoluteUri; }
    public static string GetAbsolutePath() { return GetAbsoluteUri().AbsolutePath; }
}
