using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniversityLostAndFound.Models;

namespace UniversityLostAndFound.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            // Ensure database schema exists
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "User" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Default Admin User
            var adminEmail = configuration["AdminSeed:Email"] ?? "admin@university.edu";
            var adminPassword = configuration["AdminSeed:Password"] ?? "Admin123!";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Campus Lost & Found Office",
                    Department = "Campus Safety & Student Affairs",
                    StudentOrStaffId = "STAFF-1001",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Default Student User
            var studentEmail = configuration["StudentSeed:Email"] ?? "student@university.edu";
            var studentPassword = configuration["StudentSeed:Password"] ?? "Student123!";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);
            if (studentUser == null)
            {
                studentUser = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    EmailConfirmed = true,
                    FullName = "Alex Johnson",
                    Department = "Computer Science",
                    StudentOrStaffId = "STU-2024-8891",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(studentUser, studentPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "User");
                }
            }

            // 4. Seed Categories if empty
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Electronics & Gadgets", IconClass = "bi-laptop", Description = "Laptops, phones, chargers, headphones, tablets" },
                    new Category { Name = "Textbooks & Notebooks", IconClass = "bi-book", Description = "Coursebooks, lecture notebooks, binders, planners" },
                    new Category { Name = "Keys & Lanyards", IconClass = "bi-key", Description = "Dorm keys, car key fobs, keychains, university lanyards" },
                    new Category { Name = "Student IDs & Wallet Cards", IconClass = "bi-card-heading", Description = "Campus ID cards, driver licenses, credit cards" },
                    new Category { Name = "Apparel & Eyewear", IconClass = "bi-backpack", Description = "Jackets, hoodies, prescription glasses, sunglasses, caps" },
                    new Category { Name = "Wallets & Purses", IconClass = "bi-wallet2", Description = "Leather wallets, cardholders, pouches, coin purses" },
                    new Category { Name = "Other Personal Items", IconClass = "bi-box-seam", Description = "Water bottles, umbrellas, sporting gear, calculators" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // Remove legacy accessory categories and keep their items searchable.
            var accessoryCategories = (await context.Categories.ToListAsync())
                .Where(c => c.Name.Contains("accessor", StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (accessoryCategories.Count > 0)
            {
                var fallbackCategory = await context.Categories
                    .FirstOrDefaultAsync(c => c.Name == "Other Personal Items");

                if (fallbackCategory != null)
                {
                    var accessoryCategoryIds = accessoryCategories.Select(c => c.Id).ToList();
                    var affectedItems = await context.Items
                        .Where(i => accessoryCategoryIds.Contains(i.CategoryId))
                        .ToListAsync();

                    foreach (var item in affectedItems)
                    {
                        item.CategoryId = fallbackCategory.Id;
                    }
                }

                context.Categories.RemoveRange(accessoryCategories);
                await context.SaveChangesAsync();
            }

            // 5. Seed Locations if empty
            if (!await context.Locations.AnyAsync())
            {
                var locations = new List<Location>
                {
                    new Location { Name = "Central University Library", BuildingCode = "LIB-MAIN", Description = "Ground level study hall and upper floor reading rooms" },
                    new Location { Name = "Student Union Center", BuildingCode = "SUC-01", Description = "Cafeteria, information desk, and lounge areas" },
                    new Location { Name = "Science & Engineering Complex", BuildingCode = "SEC-300", Description = "Lecture halls, lab corridors, and outdoor plaza" },
                    new Location { Name = "Campus Recreation & Gym", BuildingCode = "GYM-A", Description = "Locker rooms, basketball court, and fitness center" },
                    new Location { Name = "Arts & Humanities Building", BuildingCode = "AHB-102", Description = "Music practice rooms and studio art wing" },
                    new Location { Name = "Campus Shuttle & Bus Depot", BuildingCode = "BUS-STOP", Description = "North campus bus shelter and shuttle stops" }
                };
                await context.Locations.AddRangeAsync(locations);
                await context.SaveChangesAsync();
            }

            // 6. Seed Sample Items if empty
            if (!await context.Items.AnyAsync())
            {
                var defaultUser = studentUser ?? adminUser;
                var electronics = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Electronics"));
                var keysCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Keys"));
                var idCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("IDs"));
                var library = await context.Locations.FirstOrDefaultAsync(l => l.Name.Contains("Library"));
                var union = await context.Locations.FirstOrDefaultAsync(l => l.Name.Contains("Student Union"));

                if (defaultUser != null && electronics != null && keysCat != null && idCat != null && library != null && union != null)
                {
                    var items = new List<Item>
                    {
                        new Item
                        {
                            Title = "Silver MacBook Air M2 in Space Gray Sleeve",
                            Description = "Left on table 14 near the 2nd floor silent reading zone. Has a sticker of a university CS logo on the bottom lid.",
                            ItemType = ItemType.Found,
                            CategoryId = electronics.Id,
                            LocationId = library.Id,
                            DateLostOrFound = DateTime.Today.AddDays(-1),
                            Status = ItemStatus.Reported,
                            ContactPhone = "555-019-2834",
                            ContactEmail = "lostfound-office@university.edu",
                            UserId = defaultUser.Id,
                            CreatedAt = DateTime.UtcNow.AddDays(-1)
                        },
                        new Item
                        {
                            Title = "Set of Dorm Keys with Blue Leather Lanyard",
                            Description = "Lost somewhere between the Student Union Cafeteria and the Science Building. 3 silver keys and 1 electronic fob.",
                            ItemType = ItemType.Lost,
                            CategoryId = keysCat.Id,
                            LocationId = union.Id,
                            DateLostOrFound = DateTime.Today.AddDays(-2),
                            Status = ItemStatus.Reported,
                            ContactPhone = "555-014-9921",
                            ContactEmail = "student@university.edu",
                            RewardDetails = "$20 Coffee Card Reward!",
                            UserId = defaultUser.Id,
                            CreatedAt = DateTime.UtcNow.AddDays(-2)
                        },
                        new Item
                        {
                            Title = "University Student ID Card (Alex Johnson)",
                            Description = "Found near the main entrance check-in counter at the Central Library. Handed to security guard desk.",
                            ItemType = ItemType.Found,
                            CategoryId = idCat.Id,
                            LocationId = library.Id,
                            DateLostOrFound = DateTime.Today,
                            Status = ItemStatus.Reported,
                            ContactEmail = "lostfound-office@university.edu",
                            UserId = adminUser?.Id ?? defaultUser.Id,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    await context.Items.AddRangeAsync(items);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
