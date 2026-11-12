using Microsoft.EntityFrameworkCore;

namespace EfCoreQueryingDemo.Data;

public static class DatabaseObjects
{
    public static async Task CreateAsync(CompanyDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE OR ALTER VIEW dbo.SalaryRankings AS
            SELECT e.Id AS EmployeeId,
                   e.FirstName + ' ' + e.LastName AS FullName,
                   d.Name AS Department,
                   e.Title,
                   e.Salary,
                   ROW_NUMBER() OVER (PARTITION BY e.DepartmentId ORDER BY e.Salary DESC, e.Id) AS SalaryRank,
                   COUNT(*) OVER (PARTITION BY e.DepartmentId) AS DepartmentSize
            FROM dbo.Employees e
            JOIN dbo.Departments d ON d.Id = e.DepartmentId
            """);

        await db.Database.ExecuteSqlRawAsync("""
            CREATE OR ALTER PROCEDURE dbo.GetTopEarners
                @DepartmentId int,
                @Count int
            AS
            BEGIN
                SET NOCOUNT ON;

                SELECT TOP (@Count) e.FirstName, e.LastName, e.Title, e.Salary
                FROM dbo.Employees e
                WHERE e.DepartmentId = @DepartmentId
                ORDER BY e.Salary DESC, e.Id;
            END
            """);
    }
}
