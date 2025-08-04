using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Products.Models;
using Products.Services;
using Products.Services.Agents;
using DataEntities;
using System.Net;
using System.Text;
using System.Text.Json;
using SearchEntities;
using Store.Services;

namespace Products.Tests
{
    [TestClass]
    public sealed class A2AOrchestrationServiceTests
    {
        private ILogger<A2AOrchestrationService> _logger;
        private ILogger<InventoryAgent> _inventoryLogger;
        private ILogger<PromotionsAgent> _promotionsLogger;
        private ILogger<ResearcherAgent> _researchLogger;

        [TestInitialize]
        public void TestInit()
        {
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            _logger = loggerFactory.CreateLogger<A2AOrchestrationService>();
            _inventoryLogger = loggerFactory.CreateLogger<InventoryAgent>();
            _promotionsLogger = loggerFactory.CreateLogger<PromotionsAgent>();
            _researchLogger = loggerFactory.CreateLogger<ResearcherAgent>();
        }

        [TestMethod]
        public async Task ExecuteA2ASearchAsync_WithValidProducts_ReturnsEnrichedResults()
        {
            // Arrange
            var products = new List<DataEntities.Product>
            {
                new DataEntities.Product { Id = 1, Name = "Hiking Boots", Description = "Waterproof hiking boots", Price = 150, ImageUrl = "boots.jpg" },
                new DataEntities.Product { Id = 2, Name = "Camping Tent", Description = "4-person camping tent", Price = 200, ImageUrl = "tent.jpg" }
            };
            var mockProductService = new MockProductService(products);

            // Create mock HttpClientFactory and agents
            var httpClientFactory = new MockHttpClientFactory();
            var inventoryAgent = new InventoryAgent(httpClientFactory, _inventoryLogger);
            var promotionsAgent = new PromotionsAgent(httpClientFactory, _promotionsLogger);
            var researchAgent = new ResearcherAgent(httpClientFactory, _researchLogger);

            var orchestrationService = new A2AOrchestrationService(mockProductService, inventoryAgent, promotionsAgent, researchAgent, _logger);

            // Act
            var result = await orchestrationService.ExecuteA2ASearchAsync("hiking");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Products);
            Assert.IsTrue(result.Products.Count > 0);
            Assert.IsTrue(result.Response.Contains("Found"));

            var firstProduct = result.Products.First();
            Assert.AreEqual("Hiking Boots", firstProduct.Name);
            Assert.IsTrue(firstProduct.Stock >= 0); // Mock should return some stock
        }

        [TestMethod]
        public async Task ExecuteA2ASearchAsync_WithNoProducts_ReturnsEmptyResults()
        {
            // Arrange
            var products = new List<DataEntities.Product>();
            var mockProductService = new MockProductService(products);

            var httpClientFactory = new MockHttpClientFactory();
            var inventoryAgent = new InventoryAgent(httpClientFactory, _inventoryLogger);
            var promotionsAgent = new PromotionsAgent(httpClientFactory, _promotionsLogger);
            var researchAgent = new ResearcherAgent(httpClientFactory, _researchLogger);

            var orchestrationService = new A2AOrchestrationService(mockProductService, inventoryAgent, promotionsAgent, researchAgent, _logger);

            // Act
            var result = await orchestrationService.ExecuteA2ASearchAsync("nonexistent");

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Products);
            Assert.AreEqual(0, result.Products.Count);
        }
        // Simple mock IProductService for testing
        public class MockProductService : Store.Services.IProductService
        {
            private readonly List<DataEntities.Product> _products;
            public MockProductService(List<DataEntities.Product> products)
            {
                _products = products;
            }
            public Task<List<DataEntities.Product>> GetProducts() => Task.FromResult(_products);
            public Task<SearchEntities.SearchResponse?> Search(string searchTerm, bool semanticSearch = false)
            {
                var filtered = _products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                return Task.FromResult<SearchEntities.SearchResponse?>(new SearchEntities.SearchResponse { Products = filtered });
            }
            public Task<SearchEntities.SearchResponse?> SearchWithType(string searchTerm, Store.Services.SearchType searchType)
            {
                var filtered = _products.Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)).ToList();
                return Task.FromResult<SearchEntities.SearchResponse?>(new SearchEntities.SearchResponse { Products = filtered });
            }
        }
    }

    // Mock HttpClientFactory for testing
    public class MockHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            var handler = new MockHttpMessageHandler();
            return new HttpClient(handler) { BaseAddress = new Uri("http://localhost") };
        }
    }

    // Mock HttpMessageHandler for testing
    public class MockHttpMessageHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK);

            if (request.RequestUri?.AbsolutePath.Contains("/api/inventory/check") == true)
            {
                var inventoryResponse = new { ProductId = "1", Stock = 42 };
                response.Content = new StringContent(JsonSerializer.Serialize(inventoryResponse), Encoding.UTF8, "application/json");
            }
            else if (request.RequestUri?.AbsolutePath.Contains("/api/promotions/active") == true)
            {
                var promotionsResponse = new { ProductId = "1", Promotions = new[] { new { Title = "Special Offer", Discount = 15 } } };
                response.Content = new StringContent(JsonSerializer.Serialize(promotionsResponse), Encoding.UTF8, "application/json");
            }
            else if (request.RequestUri?.AbsolutePath.Contains("/api/researcher/insights") == true)
            {
                var insightsResponse = new { ProductId = "1", Insights = new[] { new { Review = "Great product!", Rating = 4.5 } } };
                response.Content = new StringContent(JsonSerializer.Serialize(insightsResponse), Encoding.UTF8, "application/json");
            }

            return await Task.FromResult(response);
        }
    }
}