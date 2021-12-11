using ExternalServer.Common.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExternalServer.DataAccess.Database {
    public class DatabaseContext : DbContext {

        // DbSet names must be lower case. Else there are problems on linux

        public DbSet<User> users { get; set; }

        public DbSet<BasestationIP> basestationIPs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder dbContextOptionsBuilder) {
            //string connectionString = "Data Source=localhost;Initial Catalog=SmartGardening;User ID=root;Password=";
            string connectionString = "Data Source=localhost;Initial Catalog=smartgardening;User ID=root;Password=hBSoZl1u5v6k";
            dbContextOptionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
        }
    }
}
