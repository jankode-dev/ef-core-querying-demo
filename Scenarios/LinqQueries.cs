using EfCoreQueryingDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace EfCoreQueryingDemo.Scenarios;

public static class LinqQueries
{
    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("1. LINQ beyond the basics");

        await using var db = new CompanyDbContext();

        await SelectMany(db);
        await GroupBy(db);
        await LeftJoin(db);
        await ClientEvaluation(db);
    }

    static async Task SelectMany(CompanyDbContext db)
    {
        var leads = await db.Projects
            .SelectMany(p => p.Assignments.Where(a => a.Role == "Lead"),
                (p, a) => new { Project = p.Name, Lead = a.Employee.FirstName + " " + a.Employee.LastName })
            .OrderBy(x => x.Project)
            .ToListAsync();

        Console.WriteLine();
        Console.WriteLine("  SelectMany: project leads (projects -> assignments flattened)");
        foreach (var lead in leads)
            Console.WriteLine($"    {lead.Project,-20}{lead.Lead}");
    }

    static async Task GroupBy(CompanyDbContext db)
    {
        var stats = await db.Employees
            .GroupBy(e => e.Department.Name)
            .Select(g => new
            {
                Department = g.Key,
                Employees = g.Count(),
                Average = g.Average(e => e.Salary),
                Min = g.Min(e => e.Salary),
                Max = g.Max(e => e.Salary)
            })
            .OrderByDescending(x => x.Average)
            .ToListAsync();

        Console.WriteLine();
        Console.WriteLine("  GroupBy: salary statistics per department");
        Console.WriteLine($"    {"Department",-14}{"Employees",10}{"Average",12}{"Min",10}{"Max",10}");
        foreach (var s in stats)
            Console.WriteLine($"    {s.Department,-14}{s.Employees,10:N0}{s.Average,12:N2}{s.Min,10:N2}{s.Max,10:N2}");
    }

    static async Task LeftJoin(CompanyDbContext db)
    {
        var groupJoin = await (
                from e in db.Employees
                join a in db.Assignments on e.Id equals a.EmployeeId into assignments
                from a in assignments.DefaultIfEmpty()
                where a == null
                select e.Id)
            .CountAsync();

        var leftJoin = await db.Employees
            .LeftJoin(db.Assignments, e => e.Id, a => a.EmployeeId, (e, a) => new { e.Id, Assignment = a })
            .Where(x => x.Assignment == null)
            .CountAsync();

        Console.WriteLine();
        Console.WriteLine("  Left join: employees without any project");
        Console.WriteLine($"    {"GroupJoin + DefaultIfEmpty",-36}{groupJoin,6:N0}");
        Console.WriteLine($"    {"LeftJoin (new in EF Core 10)",-36}{leftJoin,6:N0}");
    }

    static async Task ClientEvaluation(CompanyDbContext db)
    {
        Console.WriteLine();
        Console.WriteLine("  Client vs server evaluation: senior and lead employees");

        try
        {
            await db.Employees.CountAsync(e => IsSenior(e.Title));
        }
        catch (InvalidOperationException)
        {
            Console.WriteLine($"    {"CountAsync(e => IsSenior(e.Title))",-36}could not be translated to SQL");
        }

        var server = await Measure.RunAsync(() => db.Employees.CountAsync(e => e.Title == "Senior" || e.Title == "Lead"));
        var client = await Measure.RunAsync(async () => (await db.Employees.AsNoTracking().ToListAsync()).Count(e => IsSenior(e.Title)));

        Console.WriteLine($"    {"Rewritten, counted by SQL Server",-36}{server.Result,6:N0}{server.Milliseconds,10:N1} ms{server.Megabytes,9:N2} MB");
        Console.WriteLine($"    {"ToListAsync, counted on the client",-36}{client.Result,6:N0}{client.Milliseconds,10:N1} ms{client.Megabytes,9:N2} MB");
    }

    static bool IsSenior(string title) => title is "Senior" or "Lead";
}
