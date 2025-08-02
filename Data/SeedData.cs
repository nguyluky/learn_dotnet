using Microsoft.EntityFrameworkCore;
using test_api.Models.Entities;

namespace test_api.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(StoreContext context)
        {
            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Check if data already exists
            if (context.Roles.Any())
            {
                return; // Database has been seeded
            }

            // Seed Rules (Roles)
            var adminRule = new Rule
            {
                Name = "Admin",
                Description = "Administrator with full access",
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var userRule = new Rule
            {
                Name = "User",
                Description = "Regular user",
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var sellerRule = new Rule
            {
                Name = "Seller",
                Description = "Seller can manage products and orders",
                IsAdmin = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Roles.AddRange(adminRule, userRule, sellerRule);
            await context.SaveChangesAsync();

            // Seed Action Permissions - will be updated by Routes controller
            var permissions = new List<ActionPermission>
            {
                new ActionPermission
                {
                    Name = "View Users",
                    Description = "Can view user list",
                    Path = "/api/users",
                    Method = "GET",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ActionPermission
                {
                    Name = "Create Products",
                    Description = "Can create products",
                    Path = "/api/products",
                    Method = "POST",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ActionPermission
                {
                    Name = "Update Products",
                    Description = "Can update products",
                    Path = "/api/products",
                    Method = "PUT",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ActionPermission
                {
                    Name = "Delete Products",
                    Description = "Can delete products",
                    Path = "/api/products",
                    Method = "DELETE",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ActionPermission
                {
                    Name = "Create Categories",
                    Description = "Can create categories",
                    Path = "/api/categories",
                    Method = "POST",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ActionPermission
                {
                    Name = "Update Categories",
                    Description = "Can update categories",
                    Path = "/api/categories",
                    Method = "PUT",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ActionPermission
                {
                    Name = "Delete Categories",
                    Description = "Can delete categories",
                    Path = "/api/categories",
                    Method = "DELETE",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new ActionPermission
                {
                    Name = "Update Orders",
                    Description = "Can update order status",
                    Path = "/api/orders",
                    Method = "PUT",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.ActionPermissions.AddRange(permissions);
            await context.SaveChangesAsync();

            // Seed Users
            var adminUser = new User
            {
                Name = "Admin User",
                Email = "admin@example.com",
                Phone = "0123456789",
                Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                RuleId = adminRule.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var regularUser = new User
            {
                Name = "John Doe",
                Email = "john@example.com",
                Phone = "0987654321",
                Password = BCrypt.Net.BCrypt.HashPassword("User123!"),
                RuleId = userRule.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var sellerUser = new User
            {
                Name = "Jane Smith",
                Email = "jane@example.com",
                Phone = "0555666777",
                Password = BCrypt.Net.BCrypt.HashPassword("Seller123!"),
                RuleId = sellerRule.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var customer1 = new User
            {
                Name = "Alice Johnson",
                Email = "alice@example.com",
                Phone = "0111222333",
                Password = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                RuleId = userRule.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var customer2 = new User
            {
                Name = "Bob Wilson",
                Email = "bob@example.com",
                Phone = "0444555666",
                Password = BCrypt.Net.BCrypt.HashPassword("Customer123!"),
                RuleId = userRule.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Users.AddRange(adminUser, regularUser, sellerUser, customer1, customer2);
            await context.SaveChangesAsync();

            // Seed Stores
            var techStore = new Store
            {
                Name = "TechWorld Store",
                Location = "123 Tech Street, Silicon Valley",
                Description = "Leading technology and electronics store",
                OwnerId = sellerUser.Id,
                avtar = "https://images.unsplash.com/photo-1560472354-b33ff0c44a43?w=200",
                banner = "https://images.unsplash.com/photo-1441986300917-64674bd600d8?w=800",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var fashionStore = new Store
            {
                Name = "Fashion Hub",
                Location = "456 Fashion Ave, New York",
                Description = "Trendy fashion and accessories",
                OwnerId = adminUser.Id,
                avtar = "https://images.unsplash.com/photo-1441984904996-e0b6ba687e04?w=200",
                banner = "https://images.unsplash.com/photo-1445205170230-053b83016050?w=800",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Store.AddRange(techStore, fashionStore);
            await context.SaveChangesAsync();

            // Seed Categories
            var electronics = new Category
            {
                Name = "Electronics",
                Description = "Electronic devices and gadgets",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var clothing = new Category
            {
                Name = "Clothing",
                Description = "Fashion and apparel",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var books = new Category
            {
                Name = "Books",
                Description = "Books and educational materials",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var sports = new Category
            {
                Name = "Sports & Outdoors",
                Description = "Sports equipment and outdoor gear",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var home = new Category
            {
                Name = "Home & Garden",
                Description = "Home improvement and garden supplies",
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Categories.AddRange(electronics, clothing, books, sports, home);
            await context.SaveChangesAsync();

            // Seed Products
            var products = new List<Product>
            {
                // Electronics
                new Product
                {
                    Name = "iPhone 15 Pro",
                    Price = 999.99m,
                    Description = "Latest iPhone with A17 Pro chip, titanium design, and advanced camera system. Features include 48MP main camera, USB-C connectivity, and all-day battery life.",
                    ShortDescription = "Latest iPhone with titanium design",
                    ImageUrl = "https://images.unsplash.com/photo-1592750475338-74b7b21085ab?w=500",
                    CategoryId = electronics.Id,
                    StoreId = techStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "MacBook Air M3",
                    Price = 1299.99m,
                    Description = "13-inch MacBook Air with M3 chip delivers exceptional performance and up to 18 hours of battery life. Perfect for productivity and creative work.",
                    ShortDescription = "Ultra-light laptop with M3 chip",
                    ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=500",
                    CategoryId = electronics.Id,
                    StoreId = techStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Samsung Galaxy S24 Ultra",
                    Price = 1199.99m,
                    Description = "Premium Android smartphone with S Pen, 200MP camera, and AI-powered features. Built for productivity and creativity.",
                    ShortDescription = "Premium Android with S Pen",
                    ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500",
                    CategoryId = electronics.Id,
                    StoreId = techStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "iPad Pro 12.9\"",
                    Price = 1099.99m,
                    Description = "Most advanced iPad with M2 chip, Liquid Retina XDR display, and Apple Pencil support. Perfect for professional work and creativity.",
                    ShortDescription = "Professional tablet with M2 chip",
                    ImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=500",
                    CategoryId = electronics.Id,
                    StoreId = techStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Sony WH-1000XM5",
                    Price = 399.99m,
                    Description = "Industry-leading noise canceling wireless headphones with premium sound quality and 30-hour battery life.",
                    ShortDescription = "Premium noise-canceling headphones",
                    ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500",
                    CategoryId = electronics.Id,
                    StoreId = techStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Clothing
                new Product
                {
                    Name = "Classic Denim Jacket",
                    Price = 89.99m,
                    Description = "Timeless denim jacket made from premium cotton. Features classic fit, button closure, and versatile styling for any occasion.",
                    ShortDescription = "Timeless denim jacket",
                    ImageUrl = "https://images.unsplash.com/photo-1544966503-7cc5ac882d5f?w=500",
                    CategoryId = clothing.Id,
                    StoreId = fashionStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Organic Cotton T-Shirt",
                    Price = 29.99m,
                    Description = "Soft and comfortable t-shirt made from 100% organic cotton. Available in multiple colors with a perfect fit.",
                    ShortDescription = "Comfortable organic cotton tee",
                    ImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=500",
                    CategoryId = clothing.Id,
                    StoreId = fashionStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Running Sneakers",
                    Price = 129.99m,
                    Description = "High-performance running shoes with advanced cushioning and breathable mesh upper. Designed for comfort and durability.",
                    ShortDescription = "Performance running shoes",
                    ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500",
                    CategoryId = clothing.Id,
                    StoreId = fashionStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Books
                new Product
                {
                    Name = "Clean Code",
                    Price = 44.99m,
                    Description = "A handbook of agile software craftsmanship by Robert Martin. Essential reading for any serious programmer.",
                    ShortDescription = "Programming best practices guide",
                    ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=500",
                    CategoryId = books.Id,
                    StoreId = techStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "The Lean Startup",
                    Price = 16.99m,
                    Description = "How today's entrepreneurs use continuous innovation to create radically successful businesses by Eric Ries.",
                    ShortDescription = "Entrepreneurship methodology book",
                    ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=500",
                    CategoryId = books.Id,
                    StoreId = techStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Sports
                new Product
                {
                    Name = "Yoga Mat Premium",
                    Price = 49.99m,
                    Description = "High-quality yoga mat with superior grip and cushioning. Perfect for yoga, pilates, and general fitness.",
                    ShortDescription = "Premium yoga mat",
                    ImageUrl = "https://images.unsplash.com/photo-1544367567-0f2fcb009e0b?w=500",
                    CategoryId = sports.Id,
                    StoreId = fashionStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },

                // Home & Garden
                new Product
                {
                    Name = "Smart LED Bulb Set",
                    Price = 79.99m,
                    Description = "Set of 4 smart LED bulbs with WiFi connectivity, color changing, and voice control compatibility.",
                    ShortDescription = "Smart home lighting solution",
                    ImageUrl = "https://images.unsplash.com/photo-1558618666-fcd25c85cd64?w=500",
                    CategoryId = home.Id,
                    StoreId = techStore.Id,
                    IsAvailable = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            // Seed Features
            var features = new List<Feature>
            {
                new Feature
                {
                    Name = "Product Reviews",
                    Description = "Allow customers to review and rate products",
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Feature
                {
                    Name = "Wishlist",
                    Description = "Allow customers to save products to wishlist",
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Feature
                {
                    Name = "Live Chat",
                    Description = "Real-time customer support chat",
                    IsEnabled = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new Feature
                {
                    Name = "Email Notifications",
                    Description = "Send email notifications for orders and updates",
                    IsEnabled = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };

            context.Features.AddRange(features);
            await context.SaveChangesAsync();

            // Seed Sample Orders
            var sampleOrder = new Order
            {
                UserId = customer1.Id,
                CustomerName = customer1.Name,
                CustomerEmail = customer1.Email,
                OrderDate = DateTime.UtcNow.AddDays(-5),
                Status = "Delivered",
                TotalAmount = 1429.98m,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = products[0].Id, // iPhone 15 Pro
                        Quantity = 1,
                        Price = 999.99m
                    },
                    new OrderItem
                    {
                        ProductId = products[4].Id, // Sony WH-1000XM5
                        Quantity = 1,
                        Price = 399.99m
                    },
                    new OrderItem
                    {
                        ProductId = products[6].Id, // Organic Cotton T-Shirt
                        Quantity = 1,
                        Price = 29.99m
                    }
                }
            };

            var sampleOrder2 = new Order
            {
                UserId = customer2.Id,
                CustomerName = customer2.Name,
                CustomerEmail = customer2.Email,
                OrderDate = DateTime.UtcNow.AddDays(-2),
                Status = "Processing",
                TotalAmount = 1389.98m,
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = products[1].Id, // MacBook Air M3
                        Quantity = 1,
                        Price = 1299.99m
                    },
                    new OrderItem
                    {
                        ProductId = products[5].Id, // Classic Denim Jacket
                        Quantity = 1,
                        Price = 89.99m
                    }
                }
            };

            context.Orders.AddRange(sampleOrder, sampleOrder2);
            await context.SaveChangesAsync();

            // Seed Sample Comments/Reviews
            var reviews = new List<Comment>
            {
                new Comment
                {
                    Content = "Amazing phone! The camera quality is outstanding and the battery life is excellent. Highly recommend!",
                    Rating = 5,
                    UserId = customer1.Id,
                    ProductId = products[0].Id, // iPhone 15 Pro
                    OrderItemId = sampleOrder.Items.First().Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-3),
                    UpdatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new Comment
                {
                    Content = "Great headphones with excellent noise cancellation. Perfect for work and travel.",
                    Rating = 5,
                    UserId = customer1.Id,
                    ProductId = products[4].Id, // Sony WH-1000XM5
                    OrderItemId = sampleOrder.Items.Last().Id,
                    CreatedAt = DateTime.UtcNow.AddDays(-2),
                    UpdatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            context.Comments.AddRange(reviews);
            await context.SaveChangesAsync();

            // Seed Product Likes
            var likes = new List<LikeProduct>
            {
                new LikeProduct
                {
                    UserId = customer1.Id,
                    ProductId = products[1].Id, // MacBook Air M3
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new LikeProduct
                {
                    UserId = customer2.Id,
                    ProductId = products[0].Id, // iPhone 15 Pro
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new LikeProduct
                {
                    UserId = regularUser.Id,
                    ProductId = products[2].Id, // Samsung Galaxy S24 Ultra
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.LiveProduct.AddRange(likes);
            await context.SaveChangesAsync();
        }
    }
}
