using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Moq;
using SemanticSearchFunction.Functions;
using SemanticSearchFunction.Repositories;

namespace SemanticSearch.Tests;

[TestClass]
public class SqlSemanticSearchRepositoryTests
{
    private static DbContextOptions<Context> CreateDbOptions() =>
        new DbContextOptionsBuilder<Context>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

    [TestMethod]
    public void Constructor_WithNullEmbeddingGenerator_ThrowsArgumentNullException()
    {
        using var db = new Context(CreateDbOptions());
        var logger = new Mock<ILogger<SearchFunction>>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new SqlSemanticSearchRepository(null!, db, logger));
    }

    [TestMethod]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var embeddingMock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        var logger = new Mock<ILogger<SearchFunction>>().Object;

        Assert.ThrowsExactly<ArgumentNullException>(() =>
            new SqlSemanticSearchRepository(embeddingMock.Object, null!, logger));
    }

    [TestMethod]
    public void Constructor_WithValidArguments_CreatesInstance()
    {
        var embeddingMock = new Mock<IEmbeddingGenerator<string, Embedding<float>>>();
        using var db = new Context(CreateDbOptions());
        var logger = new Mock<ILogger<SearchFunction>>().Object;

        var repository = new SqlSemanticSearchRepository(embeddingMock.Object, db, logger);

        Assert.IsNotNull(repository);
    }
}