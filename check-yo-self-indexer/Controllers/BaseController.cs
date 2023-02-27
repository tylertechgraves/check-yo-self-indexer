using check_yo_self_indexer.Server.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace check_yo_self_indexer.Server.Controllers.api;

[Authorize]
[ServiceFilter(typeof(ApiExceptionFilter))]
[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class BaseController : Controller
{
    public BaseController()
    {
    }
}
