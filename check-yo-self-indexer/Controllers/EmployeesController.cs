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
    public class EmployeesController : BaseController
    {
        private readonly ILogger _logger;
        private readonly AppConfig _appConfig;
        private IElasticClient _elasticClient;

        public EmployeesController(IOptionsSnapshot<AppConfig> appConfig, ILoggerFactory loggerFactory) 
        {
            _logger = loggerFactory.CreateLogger<EmployeesController>();
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> BulkPost([FromBody]IEnumerable<Employee> employees)
        {
            try
            {
				if (employees.Count() > _appConfig.Elasticsearch.MaxBulkInsertCount)
				{
					throw new Exception($"Number of employees exceeds the max allowed bulk index count of {_appConfig.Elasticsearch.MaxBulkInsertCount}");
				}

                BulkResponse response = await _elasticClient.IndexManyAsync(employees, _appConfig.Elasticsearch.IndexName);

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

        [HttpDelete("{employeeId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(int employeeId)
        {
            try
            {   
                // Search for the document using the employeeId field.             
                var searchResponse = await _elasticClient.SearchAsync<Employee>(s => s
                    .Index(_appConfig.Elasticsearch.IndexName)
                    .From(0)
                    .Size(10)
                    .Query(q => q
                        .Match(m => m
                            .Field(f => f.EmployeeId)
                            .Query(employeeId.ToString())
                        )
                    )
                );

                // If found, delete the document
                if (searchResponse.Hits.Count() > 0)
                {
                    var deleteRequest = new DeleteRequest(_appConfig.Elasticsearch.IndexName, searchResponse.Hits.FirstOrDefault().Id);
                    await _elasticClient.DeleteAsync(deleteRequest);

                    return NoContent();
                }
                else
                {
                    // If not found, indicate as such
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, "Unable to delete document with employeeId: " + employeeId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}