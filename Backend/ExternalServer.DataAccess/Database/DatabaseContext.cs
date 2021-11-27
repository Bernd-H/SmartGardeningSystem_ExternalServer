using ExternalServer.Common.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExternalServer.DataAccess.Database {
    public class DatabaseContext : DbContext {

        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder) {
            string connectionString = "Data Source=localhost;Initial Catalog=SmartGardening;User ID=root;Password=";
            //string connectionString = "Data Source=localhost;Initial Catalog=smartgardening;User ID=root;Password=hBSoZl1u5v6k";
            dbContextOptionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}
