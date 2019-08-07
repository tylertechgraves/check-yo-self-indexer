using System;
using Microsoft.AspNetCore.Http;

namespace check_yo_self_indexer.Server
{
    public static class Context
    {
        private static IHttpContextAccessor HttpContextAccessor;
        public static void Configure(IHttpContextAccessor httpContextAccessor)
        {
            HttpContextAccessor = httpContextAccessor;
        }

        private static Uri GetAbsoluteUri()
        {
            var request = HttpContextAccessor.HttpContext.Request;
            UriBuilder uriBuilder = new UriBuilder();
            uriBuilder.Scheme = request.Scheme;
            uriBuilder.Host = request.Host.Host;
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
}