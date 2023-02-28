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
using OpenSearch.Client;

namespace check_yo_self_indexer.Server.Controllers.api;

[Route("api/[controller]")]
[AllowAnonymous]
public class EmployeesController : BaseController
{
    private readonly ILogger _logger;
    private readonly AppConfig _appConfig;
    private readonly IOpenSearchClient _openSearchClient;

    public EmployeesController(IOptionsSnapshot<AppConfig> appConfig, ILoggerFactory loggerFactory, IOpenSearchClient openSearchClient)
    {
        _logger = loggerFactory.CreateLogger<EmployeesController>();
        _appConfig = appConfig.Value;
        _openSearchClient = openSearchClient;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<Employee>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            // Search for the document using the employeeId field.             
            var searchResponse = await _openSearchClient.SearchAsync<Employee>(s => s
                .Index(_appConfig.Elasticsearch.IndexName)
                .Size(1000)
                .Query(q => q
                    .MatchAll()
                )
                .Sort(so => so
                    .Ascending(a => a.LastName)
                    .Ascending(a => a.FirstName)
                    .Ascending(a => a.EmployeeId))
            );

            var employees = searchResponse.Documents.ToList();

            return Ok(employees);
        }
        catch (Exception ex)
        {
            _logger.LogError(1, ex, "Unable to load all employees");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("{employeeId}")]
    [ProducesResponseType(typeof(Employee), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int employeeId)
    {
        try
        {
            // Search for the document using the employeeId field.             
            var searchResponse = await _openSearchClient.SearchAsync<Employee>(s => s
                .Index(_appConfig.Elasticsearch.IndexName)
                .Size(1000)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.EmployeeId)
                        .Query(employeeId.ToString())
                    )
                )
            );

            // If found, delete the document
            if (searchResponse.Hits.Count > 0)
            {
                return Ok(searchResponse.Documents.FirstOrDefault());
            }
            else
            {
                // If not found, indicate as such
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load document with employeeId: {employeeId}", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("Last/{lastName}")]
    [ProducesResponseType(typeof(List<Employee>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByLastName(string lastName)
    {
        try
        {
            // Search for the document using the employeeId field.             
            var searchResponse = await _openSearchClient.SearchAsync<Employee>(s => s
                .Index(_appConfig.Elasticsearch.IndexName)
                .Size(1000)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.LastName)
                        .Query(lastName)
                    )
                )
            );

            // If found, delete the document
            if (searchResponse.Hits.Count > 0)
            {
                return Ok(searchResponse.Documents.ToList());
            }
            else
            {
                // If not found, indicate as such
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load documents with lastName: {lastName}", lastName);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpGet("FirstLast/{firstName}/{lastName}")]
    [ProducesResponseType(typeof(List<Employee>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByFirstAndLastName(string firstName, string lastName)
    {
        try
        {
            // Search for the document using the employeeId field.             
            var searchResponse = await _openSearchClient.SearchAsync<Employee>(s => s
                .Index(_appConfig.Elasticsearch.IndexName)
                .Size(1000)
                .Query(q => q
                    .Term(t => t.LastName, lastName) && q
                    .Term(t => t.FirstName, firstName)
                )
            );

            // If found, delete the document
            if (searchResponse.Hits.Count > 0)
            {
                return Ok(searchResponse.Documents.ToList());
            }
            else
            {
                // If not found, indicate as such
                return NotFound();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load documents with lastName: {lastName}", lastName);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> BulkPost([FromBody] IEnumerable<Employee> employees)
    {
        try
        {
            if (employees.Count() > _appConfig.Elasticsearch.MaxBulkInsertCount)
            {
                throw new Exception($"Number of employees exceeds the max allowed bulk index count of {_appConfig.Elasticsearch.MaxBulkInsertCount}");
            }

            var response = await _openSearchClient.IndexManyAsync(employees, _appConfig.Elasticsearch.IndexName);

            if (response.Errors || !response.IsValid)
            {
                _logger.LogError("Error conducting bulk index: {debugInfo}", response.DebugInformation);
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
            var searchResponse = await _openSearchClient.SearchAsync<Employee>(s => s
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
            if (searchResponse.Hits.Count > 0)
            {
                var deleteRequest = new DeleteRequest(_appConfig.Elasticsearch.IndexName, searchResponse.Hits.FirstOrDefault().Id);
                await _openSearchClient.DeleteAsync(deleteRequest);

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
            _logger.LogError(ex, "Unable to delete document with employeeId: {employeeId}", employeeId);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}
