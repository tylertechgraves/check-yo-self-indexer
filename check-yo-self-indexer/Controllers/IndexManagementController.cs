using System;
using System.Threading.Tasks;
using check_yo_self_indexer.Entities.Config;
using check_yo_self_indexer.Server.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenSearch.Client;

namespace check_yo_self_indexer.Server.Controllers.api;

[Route("api/[controller]")]
[AllowAnonymous]
public class IndexManagementController : BaseController
{
    private readonly ILogger _logger;
    private readonly AppConfig _appConfig;
    private readonly IOpenSearchClient _openSearchClient;

    public IndexManagementController(IOptionsSnapshot<AppConfig> appConfig, ILoggerFactory loggerFactory, IOpenSearchClient openSearchClient)
    {
        _logger = loggerFactory.CreateLogger<IndexManagementController>();
        _appConfig = appConfig.Value;
        _openSearchClient = openSearchClient;
    }

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        var indexName = _appConfig.Elasticsearch.IndexName;
        try
        {
            var indexExistsResponse = await _openSearchClient.Indices.ExistsAsync(indexName);

            if (indexExistsResponse.IsValid && indexExistsResponse.Exists)
            {
                throw new Exception("Index of name " + _appConfig.Elasticsearch.IndexName + " already exists");
            }

            var response = await CreateIndex();

            if (!response.IsValid)
            {
                _logger.LogError("Error creating index: {debugInfo}", response.DebugInformation);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to create Employees index '{indexName}'", indexName);
            return StatusCode(StatusCodes.Status500InternalServerError, ex);
        }

    }

    [HttpPost]
    [AllowAnonymous]
    [Route("CreateIndexIfNoneExists")]
    public async Task<IActionResult> AnonymousPost()
    {
        try
        {
            _logger.LogInformation("Uri: {uri}", _appConfig.Elasticsearch.Uri);
            _logger.LogInformation("IndexName: {indexName}", _appConfig.Elasticsearch.IndexName);
            _logger.LogInformation("Username: {username}", _appConfig.Elasticsearch.Username);
            _logger.LogInformation("Password: {password}", _appConfig.Elasticsearch.Password);

            var indexExistsResponse = await _openSearchClient.Indices.ExistsAsync(_appConfig.Elasticsearch.IndexName);

            if (!indexExistsResponse.Exists)
            {
                _logger.LogInformation("Index DOES NOT exist.  Creating index...");

                var response = await CreateIndex();

                if (!response.IsValid)
                {
                    _logger.LogError("Error creating index: {debufInfo}", response.DebugInformation);
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
            var indexExistsResponse = await _openSearchClient.Indices.ExistsAsync(_appConfig.Elasticsearch.IndexName);

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
            _logger.LogError(ex, "Unable to determine if index {indexName} exists", indexName);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete()
    {
        var indexName = _appConfig.Elasticsearch.IndexName;
        try
        {
            var indexExistsReponse = await _openSearchClient.Indices.ExistsAsync(indexName);

            if (indexExistsReponse.IsValid && indexExistsReponse.Exists)
            {
                var deleteIndexResult = await _openSearchClient.Indices.DeleteAsync(_appConfig.Elasticsearch.IndexName);
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
            _logger.LogError(ex, "Unable to delete Employees index named '{indexName}'", indexName);
            return StatusCode(StatusCodes.Status500InternalServerError, ex);
        }
    }

    private async Task<CreateIndexResponse> CreateIndex()
    {
        var response = await _openSearchClient.Indices.CreateAsync(_appConfig.Elasticsearch.IndexName, c => c
            .Settings(s => s
              .NumberOfReplicas(_appConfig.Elasticsearch.NumberOfReplicas)
              .NumberOfShards(_appConfig.Elasticsearch.NumberOfShards)
              .Analysis(a => a
                .Normalizers(n => n
                    .Custom("lowerCaseNormalizer", cu => cu
                        .Filters(new string[] { "lowercase" }))))
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
                  .Normalizer("lowerCaseNormalizer")
                )
                .Keyword(k => k
                  .Name(n => n.FirstName)
                  .Normalizer("lowerCaseNormalizer")
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
