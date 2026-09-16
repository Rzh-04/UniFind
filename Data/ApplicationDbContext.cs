using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniversityLostAndFound.Models;

namespace UniversityLostAndFound.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Item> Items => Set<Item>();
        public DbSet<Claim> Claims => Set<Claim>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Item -> ApplicationUser relationship
            builder.Entity<Item>()
                .HasOne(i => i.User)
                .WithMany(u => u.ReportedItems)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Claim -> ApplicationUser relationship
            builder.Entity<Claim>()
                .HasOne(c => c.ClaimerUser)
                .WithMany(u => u.ClaimsSubmitted)
                .HasForeignKey(c => c.ClaimerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Claim -> Item relationship
            builder.Entity<Claim>()
                .HasOne(c => c.Item)
                .WithMany(i => i.Claims)
                .HasForeignKey(c => c.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
