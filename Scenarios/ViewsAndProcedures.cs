using EfCoreQueryingDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace EfCoreQueryingDemo.Scenarios;

public static class ViewsAndProcedures
{
    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("4. Views, stored procedures and raw SQL");

        await using var db = new CompanyDbContext();

        var bestPaid = await db.SalaryRankings
            .Where(r => r.SalaryRank == 1)
            .OrderBy(r => r.Department)
            .ToListAsync();

        Console.WriteLine();
        Console.WriteLine("  View SalaryRankings (ROW_NUMBER and COUNT OVER PARTITION BY): best paid per department");
        foreach (var r in bestPaid)
            Console.WriteLine($"    {r.Department,-14}{r.FullName,-22}{r.Title,-8}{r.Salary,10:N2}   1 of {r.DepartmentSize:N0}");

        var development = await db.Departments.SingleAsync(d => d.Name == "Development");
        var topEarners = await db.Database
            .SqlQuery<TopEarner>($"EXEC dbo.GetTopEarners @DepartmentId = {development.Id}, @Count = 3")
            .ToListAsync();

        Console.WriteLine();
        Console.WriteLine($"  Stored procedure GetTopEarners @DepartmentId = {development.Id} (Development), @Count = 3");
        foreach (var e in topEarners)
        {
            var name = $"{e.FirstName} {e.LastName}";
            Console.WriteLine($"    {name,-22}{e.Title,-8}{e.Salary,10:N2}");
        }

        var since = new DateTime(DateTime.Today.Year - 4, 1, 1);
        var hires = await db.Database
            .SqlQuery<HiresPerYear>($"SELECT YEAR(HiredOn) AS [Year], COUNT(*) AS Hires FROM dbo.Employees WHERE HiredOn >= {since} GROUP BY YEAR(HiredOn)")
            .OrderBy(h => h.Year)
            .ToListAsync();

        Console.WriteLine();
        Console.WriteLine($"  Raw SQL with a parameter, OrderBy composed on top: hires since {since:yyyy-MM-dd}");
        foreach (var h in hires)
            Console.WriteLine($"    {h.Year}{h.Hires,8:N0}");
    }
}

public record TopEarner(string FirstName, string LastName, string Title, decimal Salary);

public record HiresPerYear(int Year, int Hires);
