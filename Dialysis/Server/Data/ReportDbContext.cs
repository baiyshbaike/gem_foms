using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Dialysis.Server.Data {
    public class SqlDataConnectionDescription : DataConnection { }
    //public class JsonDataConnectionDescription : DataConnection { }
    public abstract class DataConnection {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string ConnectionString { get; set; }
    }

    public class ReportItem {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? DisplayName { get; set; }
        public byte[] LayoutData { get; set; }
    }

    public class ReportDbContext : DbContext {
        //public DbSet<JsonDataConnectionDescription> JsonDataConnections { get; set; }
        public DbSet<SqlDataConnectionDescription> SqlDataConnections { get; set; }
        public DbSet<ReportItem> Reports { get; set; }
        public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options) {
        }
        public void InitializeDatabase() {
            Database.EnsureCreated();
            

            var reportsSqlDataConnectionName = "ReportsConnectionString";
            if (!SqlDataConnections.Any(x => x.Name == reportsSqlDataConnectionName))
            {
                var newData = new SqlDataConnectionDescription
                {
                    Name = reportsSqlDataConnectionName,
                    DisplayName = "Report Data Connection",
                    ConnectionString = "XpoProvider=Postgres;Server=213.145.153.176;User ID=postgres;Password=postQwerty321!;Database=postgres;Encoding=UNICODE"
                };
                SqlDataConnections.Add(newData);
            }

            var reportsDataConnectionName = "ReportsDataSqlite";
            if(!SqlDataConnections.Any(x => x.Name == reportsDataConnectionName)) {
                var newData = new SqlDataConnectionDescription {
                    Name = reportsDataConnectionName,
                    DisplayName = "Reports Data",
                    ConnectionString = "XpoProvider=SQLite;Data Source=|DataDirectory|/Data/reportsData.db"
                };
                SqlDataConnections.Add(newData);
            }
            SaveChanges();
        }
    }
}