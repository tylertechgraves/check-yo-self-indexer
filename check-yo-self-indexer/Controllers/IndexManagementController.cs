using System;
using System.Threading.Tasks;
using check_yo_self_indexer.Entities.Config;
using check_yo_self_indexer.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;

namespace check_yo_self_indexer.Server.Controllers.api
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class IndexManagementController : BaseController
    {
        private readonly ILogger _logger;
        private readonly AppConfig _appConfig;
        private IElasticClient _elasticClient;

        public IndexManagementController(IOptionsSnapshot<AppConfig> appConfig, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<IndexManagementController>();
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
        public async Task<IActionResult> Post()
        {
            try
            {
                ExistsResponse indexExistsResponse = await _elasticClient.Indices.ExistsAsync(_appConfig.Elasticsearch.IndexName);

                if (indexExistsResponse.IsValid && indexExistsResponse.Exists)
                {
                    throw new Exception("Index of name " + _appConfig.Elasticsearch.IndexName + " already exists");
                }

                CreateIndexResponse response = await CreateIndex();

                if (!response.IsValid)
                {
                    _logger.LogError("Error creating index: " + response.DebugInformation);
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, "Unable to create Employees index; index already exists");
                return BadRequest();
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("CreateIndexIfNoneExists")]
        public async Task<IActionResult> AnonymousPost()
        {
            try
            {
                _logger.LogInformation("Uri: " + _appConfig.Elasticsearch.Uri);
                _logger.LogInformation("IndexName: " + _appConfig.Elasticsearch.IndexName);
                _logger.LogInformation("Username: " + _appConfig.Elasticsearch.Username);
                _logger.LogInformation("Password: " + _appConfig.Elasticsearch.Password);

                ExistsResponse indexExistsResponse = await _elasticClient.Indices.ExistsAsync(_appConfig.Elasticsearch.IndexName);

                if (!indexExistsResponse.Exists)
                {
                    _logger.LogInformation("Index DOES NOT exist.  Creating index...");

                    CreateIndexResponse response = await CreateIndex();

                    if (!response.IsValid)
                    {
                        _logger.LogError("Error creating index: " + response.DebugInformation);
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }
                else
                {
                    if (indexExistsResponse.Exists)
                        _logger.LogInformation("Index already exists.  Skipping index creation...");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, "Unable to create Employees index");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpGet]
        [Route("{indexName}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(string indexName)
        {
            try
            {
                ExistsResponse indexExistsResponse = await _elasticClient.Indices.ExistsAsync(_appConfig.Elasticsearch.IndexName);

                if (indexExistsResponse.IsValid && indexExistsResponse.Exists)
                {
                    return Ok(indexName);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, "Unable to determine if index " + indexName + "exists");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete()
        {
            try
            {
                ExistsResponse indexExistsReponse = await _elasticClient.Indices.ExistsAsync(_appConfig.Elasticsearch.IndexName);

                if (indexExistsReponse.IsValid && indexExistsReponse.Exists)
                {
                    DeleteIndexResponse deleteIndexResult = await _elasticClient.Indices.DeleteAsync(_appConfig.Elasticsearch.IndexName);
                    if (!deleteIndexResult.IsValid)
                    {
                        _logger.LogError("Unable to delete employee index");
                        return StatusCode(StatusCodes.Status500InternalServerError);
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(1, ex, "Unable to delete Employees index; does not exists");
                return BadRequest();
            }
        }

        private async Task<CreateIndexResponse> CreateIndex()
        {
            var response = await _elasticClient.Indices.CreateAsync(_appConfig.Elasticsearch.IndexName, c => c
                .Settings(s => s
                  .NumberOfReplicas(_appConfig.Elasticsearch.NumberOfReplicas)
                  .NumberOfShards(_appConfig.Elasticsearch.NumberOfShards)
                )
                .Map<Employee>(e => e
                  .AutoMap()
                  .Dynamic(DynamicMapping.Strict)
                  .Properties(ps => ps
                    .Keyword(k => k
                      .Name(n => n.EmployeeId)
                    )
                    .Keyword(k => k
                      .Name(n => n.LastName)
                    )
                    .Keyword(k => k
                      .Name(n => n.FirstName)
                    )
                    .Keyword(k => k
                      .Name(n => n.FirstPaycheckDate)
                    )
                    .Keyword(k => k
                      .Name(n => n.Salary)
                    )
                )
              )
            );

            return response;
        }
    }
}