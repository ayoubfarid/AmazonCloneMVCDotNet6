using Microsoft.EntityFrameworkCore;

namespace AmazonCloneMVC.Models
{
    public class EcomDbContext:DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<User> Users { get; set; }
        // Define other DbSet properties for your models here

        public EcomDbContext(DbContextOptions<EcomDbContext> context) : base(context)
        {
            
        }
    }
}
