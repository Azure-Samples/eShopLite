using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using SearchEntities;
using SemanticSearchFunction.Repositories;
using System.Net;
using System.Text.Json;

namespace SemanticSearchFunction.Functions;

public class SearchFunction
{
    private readonly ILogger<SearchFunction> _logger;
    private readonly ISemanticSearchRepository _repository;

    public SearchFunction(ILogger<SearchFunction> logger, ISemanticSearchRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    [Function("SemanticSearch")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "semanticsearch")] HttpRequestData req,
        CancellationToken cancellationToken = default)
    {
        var traceId = System.Diagnostics.Activity.Current?.Id ?? Guid.NewGuid().ToString();

        try
        {
            _logger.LogInformation("Semantic search request received. TraceId: {TraceId}", traceId);

            var queryParameters = req.Query;
            SearchRequest searchRequest = new()
            {
                query = queryParameters["query"]
            };

            int.TryParse(queryParameters["top"], out int topParam);
            searchRequest.top = topParam;

            _logger.LogInformation("Executing semantic search for query: '{Query}', top: {Top}, TraceId: {TraceId}",
                searchRequest.query, searchRequest.top, traceId);

            var results = await _repository.SearchAsync(searchRequest, cancellationToken);

            // Create successful response
            var httpResponse = req.CreateResponse(HttpStatusCode.OK);
            httpResponse.Headers.Add("Content-Type", "application/json");
            var jsonResponse = JsonSerializer.Serialize(results, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            await httpResponse.WriteStringAsync(jsonResponse, cancellationToken);

            _logger.LogInformation("Semantic search completed successfully. Found {Count} results. TraceId: {TraceId}", results.Products.Count, traceId);
            return httpResponse;

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during semantic search. TraceId: {TraceId}", traceId);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal server error", traceId);
        }
    }

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message, string traceId)
    {
        var errorResponse = new
        {
            error = message,
            traceId = traceId
        };

        var httpResponse = req.CreateResponse(statusCode);
        httpResponse.Headers.Add("Content-Type", "application/json");

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await httpResponse.WriteStringAsync(jsonResponse);
        return httpResponse;
    }
}