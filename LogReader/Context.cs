using Microsoft.EntityFrameworkCore;

namespace LogReader
{
    /*
     * I've only ever used CodeFirst in an API application and I tried to implement how I understand the process
     * any feedback will be highly apreciated 
     */

    internal class Context : DbContext
    {
        public DbSet<LogsModel>? Logs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var connectionString = "server=localhost;user=root;database=mydatabase;port=3306;password=your_password";
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 27));

            optionsBuilder.UseMySql(connectionString, serverVersion);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<LogsModel>(entity =>
            entity.HasKey(e => e.Id));
        }
    }
}
