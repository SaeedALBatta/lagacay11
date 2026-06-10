using lagacay11.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lagacay11.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. Seed Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Admin User
            string adminEmail = "admin@worldcupstore.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Store Administrator",
                    Address = "11 Football Avenue, Zurich, Switzerland",
                    PhoneNumber = "+41 44 234 5678",
                    CreatedAt = DateTime.UtcNow
                };

                var createAdminResult = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createAdminResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Regular User
            string userEmail = "user@worldcupstore.com";
            var normalUser = await userManager.FindByEmailAsync(userEmail);
            if (normalUser == null)
            {
                normalUser = new ApplicationUser
                {
                    UserName = userEmail,
                    Email = userEmail,
                    EmailConfirmed = true,
                    FullName = "John Doe",
                    Address = "123 Main Street, New York, USA",
                    PhoneNumber = "+1 555 123 4567",
                    CreatedAt = DateTime.UtcNow
                };

                var createUserResult = await userManager.CreateAsync(normalUser, "User@123");
                if (createUserResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(normalUser, "User");
                }
            }

            // 4. Seed Categories (only on first run — deleted categories will NOT be re-added)
            var categories = new List<Category>();
            var existingCategories = context.Categories.ToList();

            if (!existingCategories.Any())
            {
                // First run: seed all categories
                var targetCategories = new List<Category>
                {
                    new Category { Name = "National Teams", Slug = "national-teams", Description = "National team jerseys from across the globe", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Club Jerseys", Slug = "club-jerseys", Description = "Club football jerseys from various leagues", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "World Cup 2022", Slug = "world-cup-2022", Description = "Jerseys worn during the Qatar 2022 World Cup", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Retro Collection", Slug = "retro-collection", Description = "Classic retro jerseys from historical tournaments", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "World Cup 2026", Slug = "world-cup-2026", Description = "Official products and jerseys for the FIFA World Cup 2026", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Jordan National Team", Slug = "jordan", Description = "Official kits and merchandise of the Jordanian national team, Al-Nashama", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Match Balls", Slug = "match-balls", Description = "Official replicas of match balls from legendary football tournaments", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "World Cup Champions", Slug = "world-cup-champions", Description = "Exclusive merchandise and memorabilia of World Cup winning nations", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "World Cup Legends", Slug = "world-cup-legends", Description = "Iconic player jerseys and historical collectibles from World Cup history", CreatedAt = DateTime.UtcNow }
                };

                context.Categories.AddRange(targetCategories);
                await context.SaveChangesAsync();
                categories.AddRange(targetCategories);
            }
            else
            {
                // Categories already exist, just use them for product mapping
                categories.AddRange(existingCategories);
            }

            // 5. Seed Products & Join Table (Mapping images to /uploads/products/)
            if (!context.Products.Any())
            {
                var catNational = categories.First(c => c.Slug == "national-teams");
                var catClub = categories.First(c => c.Slug == "club-jerseys");
                var catWC2022 = categories.First(c => c.Slug == "world-cup-2022");
                var catRetro = categories.First(c => c.Slug == "retro-collection");

                var productsSeedData = new List<(Product Prod, List<Category> Cats, string Img)>
                {
                    (
                        new Product
                        {
                            Name = "Argentina 1986 Home Jersey",
                            Description = "The iconic light blue and white stripes jersey worn by Diego Maradona during his legendary 1986 World Cup campaign in Mexico.",
                            Price = 89.99m,
                            StockQuantity = 25,
                            TeamName = "Argentina",
                            ClubName = "",
                            Year = 1986,
                            WorldCupEdition = "Mexico 1986",
                            IsAvailable = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new List<Category> { catNational, catRetro },
                        "/uploads/products/arg86.jpg"
                    ),
                    (
                        new Product
                        {
                            Name = "Brazil 2002 Home Jersey",
                            Description = "The classic yellow and green jersey worn by Ronaldo, Ronaldinho, and Rivaldo as they secured Brazil's fifth World Cup title in Korea/Japan.",
                            Price = 99.99m,
                            StockQuantity = 15,
                            TeamName = "Brazil",
                            ClubName = "",
                            Year = 2002,
                            WorldCupEdition = "Korea/Japan 2002",
                            IsAvailable = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new List<Category> { catNational, catRetro },
                        "/uploads/products/bra02.jpg"
                    ),
                    (
                        new Product
                        {
                            Name = "France 2022 Home Jersey",
                            Description = "The official home jersey worn by Mbappe and the French national team during their run to the World Cup final in Qatar.",
                            Price = 110.00m,
                            StockQuantity = 50,
                            TeamName = "France",
                            ClubName = "",
                            Year = 2022,
                            WorldCupEdition = "Qatar 2022",
                            IsAvailable = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new List<Category> { catNational, catWC2022 },
                        "/uploads/products/fra22.jpg"
                    ),
                    (
                        new Product
                        {
                            Name = "Manchester United 1999 Retro Jersey",
                            Description = "The legendary treble-winning home jersey worn during the historic Champions League final comeback in Barcelona.",
                            Price = 95.00m,
                            StockQuantity = 10,
                            TeamName = "",
                            ClubName = "Manchester United",
                            Year = 1999,
                            WorldCupEdition = "",
                            IsAvailable = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new List<Category> { catClub, catRetro },
                        "/uploads/products/mun99.jpg"
                    ),
                    (
                        new Product
                        {
                            Name = "Real Madrid 2022 Home Jersey",
                            Description = "The pristine white home kit featuring black and purple details, worn during Real Madrid's 14th Champions League triumph.",
                            Price = 120.00m,
                            StockQuantity = 30,
                            TeamName = "",
                            ClubName = "Real Madrid",
                            Year = 2022,
                            WorldCupEdition = "",
                            IsAvailable = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new List<Category> { catClub },
                        "/uploads/products/rma22.jpg"
                    ),
                    (
                        new Product
                        {
                            Name = "Italy 2006 Tribute Jersey",
                            Description = "The stylish blue jersey worn by Cannavaro, Pirlo, and the Azzurri team that conquered the World Cup in Germany.",
                            Price = 85.00m,
                            StockQuantity = 20,
                            TeamName = "Italy",
                            ClubName = "",
                            Year = 2006,
                            WorldCupEdition = "Germany 2006",
                            IsAvailable = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new List<Category> { catNational, catRetro },
                        "/uploads/products/ita06.jpg"
                    )
                };

                foreach (var data in productsSeedData)
                {
                    context.Products.Add(data.Prod);
                    await context.SaveChangesAsync(); // Saves product to generate ID

                    // Add join categories
                    foreach (var category in data.Cats)
                    {
                        context.ProductCategories.Add(new ProductCategory
                        {
                            ProductId = data.Prod.Id,
                            CategoryId = category.Id
                        });
                    }

                    // Add main product image
                    context.ProductImages.Add(new ProductImage
                    {
                        ProductId = data.Prod.Id,
                        ImagePath = data.Img,
                        IsMain = true,
                        SortOrder = 1
                    });

                    // Add standard size variants for jerseys
                    var sizes = new[] { "S", "M", "L", "XL", "XXL" };
                    int stockPerSize = data.Prod.StockQuantity / sizes.Length;
                    foreach (var sz in sizes)
                    {
                        context.ProductSizes.Add(new ProductSize
                        {
                            ProductId = data.Prod.Id,
                            Size = sz,
                            StockQuantity = stockPerSize
                        });
                    }
                }
                await context.SaveChangesAsync();
            }


            // Seed Museum Moment Products (Match Balls) — only on first run
            if (!context.Products.Any(p => p.Name == "1986 'Hand of God' Match Ball Replica"))
            {
                var museumCatNational = context.Categories.FirstOrDefault(c => c.Slug == "national-teams");
                var museumCatRetro = context.Categories.FirstOrDefault(c => c.Slug == "retro-collection");

                if (museumCatNational != null && museumCatRetro != null)
                {
                    var newProducts = new List<(string Name, string Description, decimal Price, string Team, int Year, string Edition, string Image)>
                    {
                        (
                            "1986 'Hand of God' Match Ball Replica",
                            "The historic Azteca ball from the Argentina vs England quarter-final in Mexico 1986, where Diego Maradona scored his famous 'Hand of God' goal.",
                            350.00m,
                            "Argentina",
                            1986,
                            "Mexico 1986",
                            "https://upload.wikimedia.org/wikipedia/commons/e/ec/Adidas_Azteca_Mexico_1986_Official_ball.jpg"
                        ),
                        (
                            "1958 World Cup Final Match Ball Replica",
                            "Replica of the classic leather ball used in the 1958 World Cup Final in Sweden, where Pelé made his legendary debut and won his first title.",
                            195.00m,
                            "Brazil",
                            1958,
                            "Sweden 1958",
                            "https://upload.wikimedia.org/wikipedia/commons/2/23/Top_Star-1958.jpg"
                        ),
                        (
                            "1998 'Tricolore' Match Ball Replica",
                            "The iconic Adidas Tricolore ball used in the 1998 World Cup in France, where Zinedine Zidane scored two goals to lead France to glory.",
                            220.00m,
                            "France",
                            1998,
                            "France 1998",
                            "https://upload.wikimedia.org/wikipedia/commons/e/e0/1998_-_Tricolore_%28France%29_%284170715889%29.jpg"
                        )
                    };

                    foreach (var item in newProducts)
                    {
                        if (!context.Products.Any(p => p.Name == item.Name))
                        {
                            var product = new Product
                            {
                                Name = item.Name,
                                Description = item.Description,
                                Price = item.Price,
                                StockQuantity = 10,
                                TeamName = item.Team,
                                ClubName = "",
                                Year = item.Year,
                                WorldCupEdition = item.Edition,
                                IsAvailable = true,
                                CreatedAt = DateTime.UtcNow
                            };

                            context.Products.Add(product);
                            await context.SaveChangesAsync();

                            // Add categories
                            context.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = museumCatNational.Id });
                            context.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = museumCatRetro.Id });

                            // Add image
                            context.ProductImages.Add(new ProductImage
                            {
                                ProductId = product.Id,
                                ImagePath = item.Image,
                                IsMain = true,
                                SortOrder = 1
                            });
                        }
                    }
                    await context.SaveChangesAsync();
                }
            }

            // Seed Additional Products (Match Balls + World Cup editions)
            // NOTE: Only runs once - if these products are deleted via admin, they will NOT be re-added.
            var allCategories = context.Categories.ToList();
            var additionalProducts = new List<(Product Prod, List<string> CatSlugs, string ImagePath)>
            {
                // Jordan 2026 Qualification Jersey
                (
                    new Product
                    {
                        Name = "Jordan National Team 2026 Qualification Jersey",
                        Description = "The official home jersey for the Al-Nashama team as they compete in the FIFA World Cup 2026 qualification stages.",
                        Price = 84.99m,
                        StockQuantity = 25,
                        TeamName = "Jordan",
                        ClubName = "",
                        Year = 2026,
                        WorldCupEdition = "USA/Canada/Mexico 2026",
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new List<string> { "jordan", "world-cup-2026" },
                    "/uploads/products/jordan26.png"
                ),
                // 2026 Official World Cup Match Ball Replica
                (
                    new Product
                    {
                        Name = "2026 Official World Cup Match Ball Replica",
                        Description = "Replica of the official high-performance match ball designed for the FIFA World Cup 2026 in North America.",
                        Price = 159.99m,
                        StockQuantity = 20,
                        TeamName = "",
                        ClubName = "",
                        Year = 2026,
                        WorldCupEdition = "USA/Canada/Mexico 2026",
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new List<string> { "match-balls", "world-cup-2026" },
                    "https://upload.wikimedia.org/wikipedia/commons/e/e0/1998_-_Tricolore_%28France%29_%284170715889%29.jpg"
                ),
                // 2022 World Cup Final Match Ball Replica (Al Hilm)
                (
                    new Product
                    {
                        Name = "2022 World Cup Final Match Ball Replica",
                        Description = "Replica of the Al Hilm ('The Dream') official match ball used in the dramatic final between Argentina and France in Qatar 2022.",
                        Price = 320.00m,
                        StockQuantity = 10,
                        TeamName = "Argentina",
                        ClubName = "",
                        Year = 2022,
                        WorldCupEdition = "Qatar 2022",
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new List<string> { "match-balls", "world-cup-2022", "world-cup-champions", "world-cup-legends", "national-teams" },
                    "https://upload.wikimedia.org/wikipedia/commons/8/86/Adidas_Al_Rihla_FIFA_World_Cup_2022.jpg"
                ),
                // 2010 World Cup Final Jabulani Replica
                (
                    new Product
                    {
                        Name = "2010 World Cup Final Jabulani Replica",
                        Description = "The iconic gold-painted Jo'bulani match ball from the 2010 FIFA World Cup Final in Johannesburg, where Spain won their first world title.",
                        Price = 245.00m,
                        StockQuantity = 10,
                        TeamName = "Spain",
                        ClubName = "",
                        Year = 2010,
                        WorldCupEdition = "South Africa 2010",
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new List<string> { "match-balls", "retro-collection", "world-cup-champions" },
                    "https://upload.wikimedia.org/wikipedia/commons/7/77/Adidas_Jabulani_2010.jpg"
                ),
                // 2002 World Cup Final Fevernova Replica
                (
                    new Product
                    {
                        Name = "2002 World Cup Final Fevernova Replica",
                        Description = "Replica of the Fevernova ball used during the 2002 World Cup in Korea/Japan, representing Ronaldo's legendary final brace against Germany.",
                        Price = 210.00m,
                        StockQuantity = 10,
                        TeamName = "Brazil",
                        ClubName = "",
                        Year = 2002,
                        WorldCupEdition = "Korea/Japan 2002",
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new List<string> { "match-balls", "retro-collection", "world-cup-champions", "world-cup-legends", "national-teams" },
                    "https://upload.wikimedia.org/wikipedia/commons/1/1d/Adidas_Fevernova_World_Cup_2002.jpg"
                ),
                // 2014 World Cup Final Brazuca Replica
                (
                    new Product
                    {
                        Name = "2014 World Cup Final Brazuca Replica",
                        Description = "Replica of the Brazuca Final Rio ball used in the 2014 World Cup Final, where Mario Götze scored the extra-time winner for Germany.",
                        Price = 230.00m,
                        StockQuantity = 10,
                        TeamName = "Germany",
                        ClubName = "",
                        Year = 2014,
                        WorldCupEdition = "Brazil 2014",
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new List<string> { "match-balls", "retro-collection", "world-cup-champions" },
                    "https://upload.wikimedia.org/wikipedia/commons/c/c5/Brazuca_rio.jpg"
                )
            };

            // Only seed additional products if the DB has 6 or fewer products (initial seed only).
            // This prevents deleted products from being re-added on every app restart.
            int existingProductCount = context.Products.Count();
            if (existingProductCount <= 6)
            {
                foreach (var item in additionalProducts)
                {
                    if (!context.Products.Any(p => p.Name == item.Prod.Name))
                    {
                        context.Products.Add(item.Prod);
                        await context.SaveChangesAsync();

                        // Map Categories
                        foreach (var slug in item.CatSlugs)
                        {
                            var category = allCategories.FirstOrDefault(c => c.Slug == slug);
                            if (category != null)
                            {
                                context.ProductCategories.Add(new ProductCategory
                                {
                                    ProductId = item.Prod.Id,
                                    CategoryId = category.Id
                                });
                            }
                        }

                        // Add image
                        context.ProductImages.Add(new ProductImage
                        {
                            ProductId = item.Prod.Id,
                            ImagePath = item.ImagePath,
                            IsMain = true,
                            SortOrder = 1
                        });

                        // Add size variants
                        var sizes = new[] { "S", "M", "L", "XL", "XXL" };
                        int stockPerSize = item.Prod.StockQuantity / sizes.Length;
                        if (stockPerSize == 0) stockPerSize = 2;
                        foreach (var sz in sizes)
                        {
                            context.ProductSizes.Add(new ProductSize
                            {
                                ProductId = item.Prod.Id,
                                Size = sz,
                                StockQuantity = stockPerSize
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();
            }

            // 6. Seed Testimonials and Reviews
            if (context.Products.Any() && !context.Reviews.Any() && normalUser != null)
            {
                var sampleProduct = context.Products.First();
                context.Reviews.Add(new Review
                {
                    ProductId = sampleProduct.Id,
                    UserId = normalUser.Id,
                    Comment = "Absolutely beautiful jersey! The quality is outstanding and it fits perfectly. A true retro piece.",
                    Rating = 5,
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                });

                context.Testimonials.Add(new Testimonial
                {
                    UserId = normalUser.Id,
                    Content = "Legacy Eleven has the best collection of World Cup retro jerseys. Fast shipping and the quality of the fabrics is fantastic. Will buy again!",
                    IsApproved = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
