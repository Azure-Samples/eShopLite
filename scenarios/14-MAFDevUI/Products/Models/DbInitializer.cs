using DataEntities;

namespace Products.Models
{
    public static class DbInitializer
    {
        public static void Initialize(Context context)
        {
            InitializeProducts(context);
            InitializeLocations(context);
            InitializeCustomers(context);
            InitializeDiscounts(context);
            InitializeProductsByLocation(context);
        }

        private static void InitializeProducts(Context context)
        {
            if (context.Product.Any())
                return;

            var products = new List<Product>
            {
                new Product { Name = "Solar Powered Flashlight", Description = "A fantastic product for outdoor enthusiasts", Price = 19.99m, ImageUrl = "product1.png", IsDefault = true },
                new Product { Name = "Hiking Poles", Description = "Ideal for camping and hiking trips", Price = 24.99m, ImageUrl = "product2.png" },
                new Product { Name = "Outdoor Rain Jacket", Description = "This product will keep you warm and dry in all weathers", Price = 49.99m, ImageUrl = "product3.png" },
                new Product { Name = "Survival Kit", Description = "A must-have for any outdoor adventurer", Price = 99.99m, ImageUrl = "product4.png" },
                new Product { Name = "Outdoor Backpack", Description = "This backpack is perfect for carrying all your outdoor essentials", Price = 39.99m, ImageUrl = "product5.png" },
                new Product { Name = "Camping Cookware", Description = "This cookware set is ideal for cooking outdoors", Price = 29.99m, ImageUrl = "product6.png" },
                new Product { Name = "Camping Stove", Description = "This stove is perfect for cooking outdoors", Price = 49.99m, ImageUrl = "product7.png" },
                new Product { Name = "Camping Lantern", Description = "This lantern is perfect for lighting up your campsite", Price = 19.99m, ImageUrl = "product8.png" },
                new Product { Name = "Camping Tent", Description = "This tent is perfect for camping trips", Price = 99.99m, ImageUrl = "product9.png" },
            };

            context.AddRange(products);
            context.SaveChanges();
        }

        private static void InitializeLocations(Context context)
        {
            if (context.Location.Any())
                return;

            var locations = new List<Location>
            {
                new Location { Name = "Downtown Store", Address = "123 Main Street", City = "Seattle", State = "WA", Country = "USA", PostalCode = "98101" },
                new Location { Name = "Mall Location", Address = "456 Shopping Ave", City = "Bellevue", State = "WA", Country = "USA", PostalCode = "98004" },
                new Location { Name = "Warehouse Outlet", Address = "789 Industrial Blvd", City = "Redmond", State = "WA", Country = "USA", PostalCode = "98052" },
            };

            context.AddRange(locations);
            context.SaveChanges();
        }

        private static void InitializeCustomers(Context context)
        {
            if (context.Customer.Any())
                return;

            var customers = new List<Customer>
            {
                new Customer { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@example.com", Phone = "555-0101", MembershipTier = MembershipTier.Gold },
                new Customer { FirstName = "Bob", LastName = "Smith", Email = "bob.smith@example.com", Phone = "555-0102", MembershipTier = MembershipTier.Silver },
                new Customer { FirstName = "Carol", LastName = "Williams", Email = "carol.williams@example.com", Phone = "555-0103", MembershipTier = MembershipTier.Normal },
                new Customer { FirstName = "David", LastName = "Brown", Email = "david.brown@example.com", Phone = "555-0104", MembershipTier = MembershipTier.Normal },
            };

            context.AddRange(customers);
            context.SaveChanges();
        }

        private static void InitializeDiscounts(Context context)
        {
            if (context.Discount.Any())
                return;

            var discounts = new List<Discount>
            {
                new Discount { Name = "Gold Member Discount", Description = "Special discount for Gold members", DiscountPercentage = 20m, MembershipTier = MembershipTier.Gold },
                new Discount { Name = "Silver Member Discount", Description = "Special discount for Silver members", DiscountPercentage = 10m, MembershipTier = MembershipTier.Silver },
                new Discount { Name = "Regular Member Discount", Description = "Standard discount for all members", DiscountPercentage = 0m, MembershipTier = MembershipTier.Normal },
            };

            context.AddRange(discounts);
            context.SaveChanges();
        }

        private static void InitializeProductsByLocation(Context context)
        {
            if (context.ProductsByLocation.Any())
                return;

            var products = context.Product.ToList();
            var locations = context.Location.ToList();

            var productsByLocation = new List<ProductsByLocation>();

            // Add each product to each location with varying quantities
            foreach (var product in products)
            {
                foreach (var location in locations)
                {
                    productsByLocation.Add(new ProductsByLocation
                    {
                        ProductId = product.Id,
                        LocationId = location.Id,
                        Quantity = Random.Shared.Next(5, 50)
                    });
                }
            }

            context.AddRange(productsByLocation);
            context.SaveChanges();
        }

        private static List<Product> GetProductsToAdd(int count, List<Product> baseProducts)
        {
            var productsToAdd = new List<Product>();
            for (int i = 1; i < count; i++)
            {
                foreach (var product in baseProducts)
                {
                    var newproduct = new Product
                    {
                        Name = $"{product.Name}-{i}",
                        Description = product.Description,
                        ImageUrl = product.ImageUrl,
                        Price = product.Price
                    };
                    productsToAdd.Add(newproduct);
                }
            }
            return productsToAdd;
        }
    }
}
