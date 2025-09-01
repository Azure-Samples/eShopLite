using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;
using Products.Models;
using Products.Memory;
using DataEntities;
using VectorEntities;

namespace Products.Tests
{
    [TestClass]
    public sealed class MemoryContextTests
    {
        private DbContextOptions<Context> _dbOptions = null!;
        private Mock<ILogger> _mockLogger = null!;
        private Mock<IEmbeddingGenerator<string, Embedding<float>>> _mockEmbeddingGenerator = null!;

        [TestInitialize]
        public void TestInit()
        {
            // Use a unique database name for each test run to ensure isolation
            _dbOptions = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _mockLogger = new Mock<ILogger>();
            _mockEmbeddingGenerator = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        }

        [TestMethod]
        public async Task InitMemoryContextAsync_PopulatesCollection()
        {
            // Arrange
            using (var context = new Context(_dbOptions))
            {
                context.Product.AddRange(new List<Product>
                {
                    new Product { Id = 1, Name = "Test Product 1", Description = "Test Description 1", Price = 10.99m, ImageUrl = "img1.png" },
                    new Product { Id = 2, Name = "Test Product 2", Description = "Test Description 2", Price = 20.99m, ImageUrl = "img2.png" }
                });
                context.SaveChanges();
            }

            // Create a simplified mock embedding generator wrapper
            var mockEmbeddingClient = new Mock<TestEmbeddingClient>();
            var memoryContext = new MemoryContext(_mockLogger.Object, null, mockEmbeddingClient.Object);

            using (var context = new Context(_dbOptions))
            {
                // Act
                var result = await memoryContext.InitMemoryContextAsync(context);

                // Assert
                Assert.IsTrue(result, "InitMemoryContextAsync should return true");
                
                // Verify logger was called for initialization
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Initializing memory context")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.AtLeastOnce,
                    "Should log initialization message");

                // Verify logger was called for completion
                _mockLogger.Verify(
                    x => x.Log(
                        LogLevel.Information,
                        It.IsAny<EventId>(),
                        It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("DONE! Filling products in memory")),
                        It.IsAny<Exception>(),
                        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                    Times.Once,
                    "Should log completion message");

                // Verify products collection is not null (indicating it was initialized)
                Assert.IsNotNull(memoryContext._productsCollection, "Products collection should be initialized");
            }
        }
    }

    // Simple test embedding client that implements the interface directly
    public class TestEmbeddingClient : IEmbeddingGenerator<string, Embedding<float>>
    {
        public EmbeddingGeneratorMetadata Metadata => new("test-embedding-model");

        public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(IEnumerable<string> values, EmbeddingGenerationOptions? options = null, CancellationToken cancellationToken = default)
        {
            var embeddings = values.Select(_ => new Embedding<float>(new ReadOnlyMemory<float>(new float[] { 0.1f, 0.2f, 0.3f, 0.4f })));
            return Task.FromResult(new GeneratedEmbeddings<Embedding<float>>(embeddings));
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return null;
        }

        public void Dispose()
        {
        }
    }
}