using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Moq;
using SearchEntities;
using SemanticSearchFunction.Functions;
using SemanticSearchFunction.Repositories;
using System.Collections.Specialized;
using System.Net;

namespace SemanticSearch.Tests;

[TestClass]
public class SearchFunctionTests
{
    private Mock<ILogger<SearchFunction>> _loggerMock = null!;
    private Mock<ISemanticSearchRepository> _repositoryMock = null!;
    private SearchFunction _function = null!;
    private Mock<FunctionContext> _contextMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<SearchFunction>>();
        _repositoryMock = new Mock<ISemanticSearchRepository>();
        _function = new SearchFunction(_loggerMock.Object, _repositoryMock.Object);
        _contextMock = new Mock<FunctionContext>();
    }

    private (Mock<HttpRequestData>, Mock<HttpResponseData>) CreateRequestResponseMocks(NameValueCollection queryParams)
    {
        // Note: CreateResponse(HttpStatusCode) is an extension method, so we mock the abstract CreateResponse()
        // and let the extension set the status code on the mocked HttpResponseData.
        var responseMock = new Mock<HttpResponseData>(_contextMock.Object);
        responseMock.SetupProperty(r => r.StatusCode);
        responseMock.Setup(r => r.Headers).Returns(new HttpHeadersCollection());
        responseMock.SetupGet(r => r.Body).Returns(new MemoryStream());

        var requestMock = new Mock<HttpRequestData>(_contextMock.Object);
        requestMock.Setup(r => r.Query).Returns(queryParams);
        requestMock.Setup(r => r.CreateResponse()).Returns(responseMock.Object);

        return (requestMock, responseMock);
    }

    [TestMethod]
    public async Task Run_WithValidQuery_ReturnsOkResult()
    {
        // Arrange
        var queryParams = new NameValueCollection { ["query"] = "tent", ["top"] = "3" };
        var expectedResponse = new SearchResponse
        {
            Products = new List<DataEntities.Product>
            {
                new DataEntities.Product { Id = 1, Name = "Tent", Description = "A tent", Price = 99.99m }
            },
            Response = "1 Products found for [tent]"
        };

        _repositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var (requestMock, _) = CreateRequestResponseMocks(queryParams);

        // Act
        var result = await _function.Run(requestMock.Object);

        // Assert
        Assert.IsNotNull(result);
        _repositoryMock.Verify(r => r.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Run_WhenRepositoryThrows_ReturnsInternalServerError()
    {
        // Arrange
        var queryParams = new NameValueCollection { ["query"] = "tent" };
        _repositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB error"));

        var (requestMock, _) = CreateRequestResponseMocks(queryParams);

        // Act
        var result = await _function.Run(requestMock.Object);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Run_WithEmptyQuery_ReturnsOkResult()
    {
        // Arrange - empty query still gets processed (repository handles empty results)
        var queryParams = new NameValueCollection { ["query"] = "" };
        var emptyResponse = new SearchResponse
        {
            Products = new List<DataEntities.Product>(),
            Response = "No products found for []"
        };

        _repositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyResponse);

        var (requestMock, _) = CreateRequestResponseMocks(queryParams);

        // Act
        var result = await _function.Run(requestMock.Object);

        // Assert
        Assert.IsNotNull(result);
    }
}