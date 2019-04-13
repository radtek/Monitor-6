using Microsoft.EntityFrameworkCore;
using Kakegurui.Core;
using Microsoft.Extensions.Configuration;

namespace JabamiYumeko
{
    public class MySQLContext : DbContext
    {
       
        private MySQLContext(DbContextOptions<MySQLContext> options)
            : base(options)
        {
        }

        public static MySQLContext CreateContext()
        {
            var optionBuilder = new DbContextOptionsBuilder<MySQLContext>();
            optionBuilder.UseMySQL(AppConfig.Config.GetValue<string>("mysql"));
            return new MySQLContext(optionBuilder.Options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Service>().HasKey(t => new { t.Ip, t.Name });
        }


        public DbSet<Host> Hosts { get; set; }

        public DbSet<Service> Services { get; set; }
    }
}
