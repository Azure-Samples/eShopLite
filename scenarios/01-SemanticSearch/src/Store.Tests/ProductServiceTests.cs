using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Store.Services;
using SearchEntities;
using System.Net;
using System.Text.Json;

namespace Store.Tests
{
    [TestClass]
    public sealed class ProductServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpHandler = null!;
        private HttpClient _httpClient = null!;
        private Mock<ILogger<ProductService>> _mockLogger = null!;
        private ProductService _productService = null!;

        [TestInitialize]
        public void TestInit()
        {
            _mockHttpHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpHandler.Object)
            {
                BaseAddress = new Uri("https://localhost/")
            };
            _mockLogger = new Mock<ILogger<ProductService>>();
            _productService = new ProductService(_httpClient, _mockLogger.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _httpClient?.Dispose();
        }

        [TestMethod]
        public async Task Search_WithSemanticSearch_CallsAISearchEndpoint()
        {
            // Arrange
            var searchTerm = "tent";
            var expectedResponse = new SearchResponse 
            { 
                Response = "Found camping tents",
                Products = new List<DataEntities.Product>()
            };
            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _productService.Search(searchTerm, semanticSearch: true);

            // Assert
            Assert.IsNotNull(result, "Search result should not be null");
            Assert.AreEqual("Found camping tents", result.Response);
            
            // Verify the HTTP call was made
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task Search_WithStandardSearch_CallsProductSearchEndpoint()
        {
            // Arrange
            var searchTerm = "tent";
            var expectedResponse = new SearchResponse 
            { 
                Response = "Found products",
                Products = new List<DataEntities.Product>()
            };
            var responseJson = JsonSerializer.Serialize(expectedResponse);
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseJson, System.Text.Encoding.UTF8, "application/json")
            };

            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _productService.Search(searchTerm, semanticSearch: false);

            // Assert
            Assert.IsNotNull(result, "Search result should not be null");
            Assert.AreEqual("Found products", result.Response);
            
            // Verify the HTTP call was made
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [TestMethod]
        public async Task Search_WhenHttpCallFails_ReturnsDefaultResponse()
        {
            // Arrange
            var searchTerm = "tent";
            _mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            var result = await _productService.Search(searchTerm, semanticSearch: true);

            // Assert
            Assert.IsNotNull(result, "Search result should not be null");
            Assert.AreEqual("No response", result.Response);
            
            // Verify error was logged
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during Search")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once,
                "Should log error when search fails");
        }
    }
}
