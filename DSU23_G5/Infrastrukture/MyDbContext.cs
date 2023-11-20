using DSU23_G5.Models;
using DSU23_G5.Modelsusing;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System.Diagnostics;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DSU23_G5.Infrastrukture
{
    public class MyDbContext : DbContext
    {
        public DbSet<SpotPrice> SpotPrice { get; set; }
        public DbSet<PriceArea> PriceAreas { get; set; }

        private string? _connectionString;
        /// <summary>
        /// Connects to the database with connectionstring.
        /// </summary>
        public MyDbContext()
        {
            var config = new ConfigurationBuilder().AddUserSecrets<MyDbContext>()
                        .Build();
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(_connectionString);
        }
        /// <summary>
        /// This method is called OnModelCreating and it is used to
        /// configure the model that will be used by the Entity Framework (EF) in the database. It overrides the base method OnModelCreating and it is part of the DbContext class.
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SpotPrice>().HasKey(p => p.Id);
            modelBuilder.Entity<PriceArea>().HasKey(p => p.Id);
            modelBuilder.Entity<SpotPrice>().ToTable("SpotPrice");
            modelBuilder.Entity<PriceArea>().ToTable("PriceArea");
        }
        /// <summary>
        /// Loops the jsonfile and imports the data into the database.
        /// </summary>
        public void ImportJsonFile()
        {
            string json = System.IO.File.ReadAllText("Models/data.json");
            var data = JsonConvert.DeserializeObject<List<SpotPrice>>(json);
            try
            {
                using (var db = new MyDbContext())
                {
                    if (data == null)
                    {
                        return;
                    }
                    foreach (var item in data)
                    {
                        item.Date = item.Date.ToUniversalTime();
                        db.SpotPrice.Add(item);
                    }
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ett fel uppstod: " + ex.Message);
            }
        }

        public PriceArea[] GetHourlySpotPriceFromDb(DateTime inputDateFrom, string? homePriceArea)
        {
            var inputDateTo = new DateTime(inputDateFrom.Year, inputDateFrom.Month, DateTime.DaysInMonth(inputDateFrom.Year, inputDateFrom.Month));
            inputDateTo = inputDateFrom.AddHours(23).AddMinutes(0).AddSeconds(0);
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                StringBuilder sql = new StringBuilder("SELECT \"Date\", \"Value\"");
                sql.AppendLine("FROM \"SpotPrice\"");
                sql.AppendLine("JOIN \"PriceArea\"");
                sql.AppendLine("ON \"SpotPrice\".\"Id\" = \"PriceArea\".\"SpotPriceId\"");
                sql.AppendLine($"WHERE \"PriceArea\".\"Name\" = '{homePriceArea}'");
                sql.AppendLine($"AND \"SpotPrice\".\"Date\" BETWEEN '{inputDateFrom}' AND '{inputDateTo}'");


                using var command = new NpgsqlCommand(sql.ToString(), conn);
                command.Transaction = transaction;

                List<PriceArea> priceAreas = new List<PriceArea>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        priceAreas.Add(new PriceArea()
                        {
                            Date = reader.GetDateTime(0).ToLocalTime(),
                            Value = reader.GetDouble(1),

                        });
                    }
                }
                transaction.Commit();
                return priceAreas.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public PriceArea[] GetYearlySpotPricesFromDb(DateTime inputDateFrom, string? homePriceArea)
        {
            var inputDateTo = new DateTime(inputDateFrom.Year, inputDateFrom.Month, DateTime.DaysInMonth(inputDateFrom.Year, inputDateFrom.Month));
            inputDateTo = inputDateFrom.AddMonths(12).AddDays(-1).AddHours(23).AddMinutes(0).AddSeconds(0);
            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                StringBuilder sql = new StringBuilder("SELECT DATE_TRUNC('month', \"Date\") as month, AVG(\"Value\")");
                sql.AppendLine("FROM \"SpotPrice\"");
                sql.AppendLine("JOIN \"PriceArea\"");
                sql.AppendLine("ON \"SpotPrice\".\"Id\" = \"PriceArea\".\"SpotPriceId\"");
                sql.AppendLine($"WHERE \"PriceArea\".\"Name\" = '{homePriceArea}'");
                sql.AppendLine($"AND \"SpotPrice\".\"Date\" BETWEEN '{inputDateFrom}' AND '{inputDateTo}'");
                sql.AppendLine("GROUP BY Month");
                sql.AppendLine("ORDER BY Month ASC;");

                using var command = new NpgsqlCommand(sql.ToString(), conn);
                command.Transaction = transaction;

                List<PriceArea> priceAreas = new List<PriceArea>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        priceAreas.Add(new PriceArea()
                        {
                            Date = reader.GetDateTime(0).ToLocalTime(),
                            Value = reader.GetDouble(1),
                            
                        });
                    }
                }
                transaction.Commit();
                return priceAreas.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public PriceArea[] GetMonthlySpotPricesFromDb(DateTime inputDateFrom, string? homePriceArea)
        {
            var inputDateTo = new DateTime(inputDateFrom.Year, inputDateFrom.Month, DateTime.DaysInMonth(inputDateFrom.Year, inputDateFrom.Month));
            inputDateTo = inputDateFrom.AddMonths(1).AddDays(-1).AddHours(23).AddMinutes(0).AddSeconds(0);

            try
            {
                using var conn = new NpgsqlConnection(_connectionString);
                conn.Open();
                using var transaction = conn.BeginTransaction();

                StringBuilder sql = new StringBuilder("SELECT DATE(\"SpotPrice\".\"Date\"), AVG(\"Value\")");
                sql.AppendLine("FROM \"SpotPrice\"");
                sql.AppendLine("JOIN \"PriceArea\"");
                sql.AppendLine("ON \"SpotPrice\".\"Id\" = \"PriceArea\".\"SpotPriceId\"");

                sql.AppendLine($"WHERE \"PriceArea\".\"Name\" = '{homePriceArea}'");
                sql.AppendLine($"AND \"SpotPrice\".\"Date\" BETWEEN '{inputDateFrom}' AND '{inputDateTo}'");
                sql.AppendLine("GROUP BY DATE(\"SpotPrice\".\"Date\")");
                sql.AppendLine("ORDER BY DATE(\"SpotPrice\".\"Date\") ASC");

                using var command = new NpgsqlCommand(sql.ToString(), conn);
                command.Transaction = transaction;

                List<PriceArea> priceAreas = new List<PriceArea>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        priceAreas.Add(new PriceArea()
                        {
                            Date = reader.GetDateTime(0).ToLocalTime(),
                            Value = reader.GetDouble(1),
                        });
                    }
                    
                }
                transaction.Commit();
                return priceAreas.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }
        
    }
}
