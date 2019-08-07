using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using check_yo_self_indexer.Entities.Config;
using check_yo_self_indexer.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using NSwag.Annotations;

namespace check_yo_self_indexer.Server.Controllers.api
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class EmployeeIndexerController : BaseController
    {
        private readonly ILogger _logger;
        private readonly AppConfig _appConfig;
        private IElasticClient _elasticClient;

        public EmployeeIndexerController(IOptionsSnapshot<AppConfig> appConfig, ILoggerFactory loggerFactory) 
        {
            _logger = loggerFactory.CreateLogger<EmployeeIndexerController>();
            _appConfig = appConfig.Value;

            var node = new Uri(_appConfig.Elasticsearch.Uri);
            var elasticSettings = new ConnectionSettings(node);

            if (_appConfig.Elasticsearch.UseAuthentication)
            {
                _logger.LogDebug("We did not skip basic auth");
                elasticSettings.BasicAuthentication(_appConfig.Elasticsearch.Username, _appConfig.Elasticsearch.Password);
            }
            else
              _logger.LogDebug("We skipped basic auth");

            _elasticClient = new ElasticClient(elasticSettings);
        }

        [HttpPost]
		[Route("Employees")]
        public async Task<IActionResult> BulkPost([FromBody]IEnumerable<Employee> registrations)
        {
            try
            {
				if (registrations.Count() > _appConfig.Elasticsearch.MaxBulkInsertCount)
				{
					throw new Exception($"Number of employees exceeds the max allowed bulk index count of {_appConfig.Elasticsearch.MaxBulkInsertCount}");
				}

                BulkResponse response = await _elasticClient.IndexManyAsync(registrations, _appConfig.Elasticsearch.IndexName);

                if (response.Errors || !response.IsValid)
                {
                    _logger.LogError("Error conducting bulk index: " + response.DebugInformation);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, "Unable to index employees entity collection");
                return BadRequest();
            }

        }
    }
}