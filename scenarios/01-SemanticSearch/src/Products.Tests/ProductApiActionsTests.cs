using Microsoft.EntityFrameworkCore;
using Products.Models;
using DataEntities;
using Products.Endpoints;

namespace Products.Tests
{
    [TestClass]
    public sealed class ProductApiActionsTests
    {
        private DbContextOptions<Context> _dbOptions;

        [TestInitialize]
        public void TestInit()
        {
            // Use a unique database name for each test run to ensure isolation
            _dbOptions = new DbContextOptionsBuilder<Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [TestMethod]
        public async Task GetAllProducts_ReturnsAllSeededProducts()
        {
            // Arrange
            using (var context = new Context(_dbOptions))
            {
                context.Product.AddRange(new List<Product>
                {
                    new Product { Id = 1, Name = "Test1", Description = "Desc1", Price = 10, ImageUrl = "img1" },
                    new Product { Id = 2, Name = "Test2", Description = "Desc2", Price = 20, ImageUrl = "img2" }
                });
                context.SaveChanges();
            }

            using (var context = new Context(_dbOptions))
            {
                // Act
                var result = await ProductApiActions.GetAllProducts(context);
                var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<List<Product>>;
                Assert.IsNotNull(okResult, "Result should be Ok with a list of products");
                var products = okResult.Value;
                Assert.AreEqual(2, products.Count, "Should return all seeded products");
                Assert.IsTrue(products.Any(p => p.Name == "Test1"));
                Assert.IsTrue(products.Any(p => p.Name == "Test2"));
            }
        }

        [TestMethod]
        public async Task GetProductById_ReturnsCorrectProductOrNotFound()
        {
            using (var context = new Context(_dbOptions))
            {
                context.Product.Add(new Product { Id = 10, Name = "Prod10", Description = "Desc10", Price = 100, ImageUrl = "img10" });
                context.SaveChanges();
            }
            using (var context = new Context(_dbOptions))
            {
                var found = await ProductApiActions.GetProductById(10, context);
                var okResult = found as Microsoft.AspNetCore.Http.HttpResults.Ok<Product>;
                Assert.IsNotNull(okResult);
                Assert.AreEqual("Prod10", okResult.Value.Name);

                var notFound = await ProductApiActions.GetProductById(999, context);
                Assert.IsInstanceOfType(notFound, typeof(Microsoft.AspNetCore.Http.HttpResults.NotFound));
            }
        }

        [TestMethod]
        public async Task CreateProduct_AddsProductToDatabase()
        {
            using (var context = new Context(_dbOptions))
            {
                var newProduct = new Product { Id = 20, Name = "NewProd", Description = "NewDesc", Price = 200, ImageUrl = "img20" };
                var result = await ProductApiActions.CreateProduct(newProduct, context);
                var created = result as Microsoft.AspNetCore.Http.HttpResults.Created<Product>;
                Assert.IsNotNull(created);
                Assert.AreEqual("/api/Product/20", created.Location);
                Assert.AreEqual("NewProd", created.Value.Name);
                Assert.AreEqual(1, context.Product.Count());
            }
        }

        [TestMethod]
        public async Task UpdateProduct_UpdatesExistingProductOrReturnsNotFound()
        {
            using (var context = new Context(_dbOptions))
            {
                context.Product.Add(new Product { Id = 30, Name = "OldName", Description = "OldDesc", Price = 300, ImageUrl = "img30" });
                context.SaveChanges();
            }
            using (var context = new Context(_dbOptions))
            {
                var updated = new Product { Id = 30, Name = "UpdatedName", Description = "UpdatedDesc", Price = 333, ImageUrl = "img30u" };
                var result = await ProductApiActions.UpdateProduct(30, updated, context);
                Assert.IsInstanceOfType(result, typeof(Microsoft.AspNetCore.Http.HttpResults.Ok));
                var prod = context.Product.First(p => p.Id == 30);
                Assert.AreEqual("UpdatedName", prod.Name);
                Assert.AreEqual(333, prod.Price);

                var notFound = await ProductApiActions.UpdateProduct(999, updated, context);
                Assert.IsInstanceOfType(notFound, typeof(Microsoft.AspNetCore.Http.HttpResults.NotFound));
            }
        }

        [TestMethod]
        public async Task DeleteProduct_RemovesProductOrReturnsNotFound()
        {
            using (var context = new Context(_dbOptions))
            {
                context.Product.Add(new Product { Id = 40, Name = "ToDelete", Description = "DelDesc", Price = 400, ImageUrl = "img40" });
                context.SaveChanges();
            }
            using (var context = new Context(_dbOptions))
            {
                await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await ProductApiActions.DeleteProduct(40, context);
                });
            }
        }

        [TestMethod]
        public async Task SearchAllProducts_ReturnsMatchingProductsAndResponse()
        {
            using (var context = new Context(_dbOptions))
            {
                context.Product.AddRange(new List<Product>
                {
                    new Product { Id = 50, Name = "Tent", Description = "Desc", Price = 500, ImageUrl = "img50" },
                    new Product { Id = 51, Name = "Lantern", Description = "Desc", Price = 510, ImageUrl = "img51" }
                });
                context.SaveChanges();
            }
            using (var context = new Context(_dbOptions))
            {
                var result = await ProductApiActions.SearchAllProducts("Tent", context);
                var okResult = result as Microsoft.AspNetCore.Http.HttpResults.Ok<SearchEntities.SearchResponse>;
                Assert.IsNotNull(okResult);
                Assert.AreEqual(1, okResult.Value.Products.Count);
                Assert.AreEqual("Tent", okResult.Value.Products[0].Name);
                Assert.IsTrue(okResult.Value.Response.Contains("1 Products found"));

                var noResult = await ProductApiActions.SearchAllProducts("Nonexistent", context);
                var okNoResult = noResult as Microsoft.AspNetCore.Http.HttpResults.Ok<SearchEntities.SearchResponse>;
                Assert.IsNotNull(okNoResult);
                Assert.AreEqual(0, okNoResult.Value.Products.Count);
                Assert.IsTrue(okNoResult.Value.Response.Contains("No products found"));
            }
        }
    }
}
