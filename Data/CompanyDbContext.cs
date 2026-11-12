using EfCoreQueryingDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCoreQueryingDemo.Data;

public class CompanyDbContext : DbContext
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<SalaryRanking> SalaryRankings => Set<SalaryRanking>();

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=EfCoreQueryingDemo;Trusted_Connection=True");

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<string>().HaveMaxLength(100);
        configurationBuilder.Properties<decimal>().HavePrecision(10, 2);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(employee =>
        {
            employee.ToTable(t => t.IsTemporal());
            employee.HasIndex(e => e.LastName);
        });

        modelBuilder.Entity<Assignment>().HasKey(a => new { a.EmployeeId, a.ProjectId });

        modelBuilder.Entity<SalaryRanking>().HasNoKey().ToView("SalaryRankings");
    }
}
