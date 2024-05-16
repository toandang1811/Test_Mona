using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace WebApplication1.Data
{
    public class EmployeeManagementContext : DbContext
    {
        public EmployeeManagementContext() { }
        public EmployeeManagementContext(DbContextOptions<EmployeeManagementContext> option) : base(option) 
        {

        }

        public DbSet<Employee>? Employees { get; set; }
        public DbSet<Key>? Keys { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            new DbInitializer(modelBuilder).Seed();
        }

        public class DbInitializer
        {
            private readonly ModelBuilder modelBuilder;

            public DbInitializer(ModelBuilder modelBuilder)
            {
                this.modelBuilder = modelBuilder;
            }

            public void Seed()
            {
                modelBuilder.Entity<Key>().HasData(
                       new Key() { TableName = "Employees", LastKey = 1, KeyName = "NV" }
                );
            }
        }
    }
}
