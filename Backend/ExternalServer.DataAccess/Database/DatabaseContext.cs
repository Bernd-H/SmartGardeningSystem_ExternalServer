using ExternalServer.Common.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExternalServer.DataAccess.Database {

    /// <summary>
    /// Inherits from DbContext. Class that contains the connection string for the mysql database and multiple tables as DbSet instance.
    /// </summary>
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
