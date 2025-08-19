using DataEntities;
using Microsoft.Extensions.AI;

namespace Products.Models;

public static class DbInitializer
{
    public static async Task Initialize(Context context, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, int dimensions = 1536, ILogger logger = null)
    {
        if (context.Product.Any())
            return;

        var products = new List<Product>
        {
            new Product { Name = "Solar Powered Flashlight", Description = "A fantastic product for outdoor enthusiasts", Price = 19.99m, ImageUrl = "product1.png" },
            new Product { Name = "Hiking Poles", Description = "Ideal for camping and hiking trips", Price = 24.99m, ImageUrl = "product2.png" },
            new Product { Name = "Outdoor Rain Jacket", Description = "This product will keep you warm and dry in all weathers", Price = 49.99m, ImageUrl = "product3.png" },
            new Product { Name = "Survival Kit", Description = "A must-have for any outdoor adventurer", Price = 99.99m, ImageUrl = "product4.png" },
            new Product { Name = "Outdoor Backpack", Description = "This backpack is perfect for carrying all your outdoor essentials", Price = 39.99m, ImageUrl = "product5.png" },
            new Product { Name = "Camping Cookware", Description = "This cookware set is ideal for cooking outdoors", Price = 29.99m, ImageUrl = "product6.png" },
            new Product { Name = "Camping Stove", Description = "This stove is perfect for cooking outdoors", Price = 49.99m, ImageUrl = "product7.png" },
            new Product { Name = "Camping Lantern", Description = "This lantern is perfect for lighting up your campsite", Price = 19.99m, ImageUrl = "product8.png" },
            new Product { Name = "Camping Tent", Description = "This tent is perfect for camping trips", Price = 99.99m, ImageUrl = "product9.png" },
        };

        // add embeddings
        foreach (var product in products)
        {
            var productInformation = $"Name = {product.Name} - Description = {product.Description} - Price = {product.Price}";
            var productInformationEmbedding = await embeddingGenerator.GenerateVectorAsync(productInformation, new() { Dimensions = dimensions });
            product.Embedding = productInformationEmbedding.ToArray();
            logger.LogInformation("Added embedding for product: {productName}, Dimensions: {dimensions}", product.Name, productInformationEmbedding.Length);
        }

        // add products to context
        context.AddRange(products);

        context.SaveChanges();
    }
}
