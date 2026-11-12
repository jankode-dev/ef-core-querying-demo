using EfCoreQueryingDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace EfCoreQueryingDemo.Scenarios;

public static class Paging
{
    const int PageSize = 25;

    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("5. Paging");
        Console.WriteLine($"  Employees sorted by last name, {PageSize} per page");
        Console.WriteLine();
        Console.WriteLine($"    {"Approach",-34}{"Rows",8}{"Time",12}{"Allocated",12}");

        await using var db = new CompanyDbContext();

        var lastPage = (await db.Employees.CountAsync() - 1) / PageSize;

        Print("Everything, no paging", await Measure.RunAsync(() => Sorted(db).ToListAsync()));
        Print("Page 1 (Skip 0)", await Measure.RunAsync(() => Page(db, 0)));
        Print($"Page {lastPage + 1:N0} (Skip {lastPage * PageSize:N0})", await Measure.RunAsync(() => Page(db, lastPage)));

        Console.WriteLine();
        Console.WriteLine("  SQL for one page:");
        foreach (var line in Sorted(db).Skip(lastPage * PageSize).Take(PageSize).ToQueryString().Split('\n'))
            Console.WriteLine($"    {line.TrimEnd()}");
    }

    static IQueryable<EmployeeRow> Sorted(CompanyDbContext db) =>
        db.Employees
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.Id)
            .Select(e => new EmployeeRow(e.Id, e.FirstName, e.LastName, e.Title));

    static Task<List<EmployeeRow>> Page(CompanyDbContext db, int pageIndex) =>
        Sorted(db).Skip(pageIndex * PageSize).Take(PageSize).ToListAsync();

    static void Print(string name, Measurement<List<EmployeeRow>> m) =>
        Console.WriteLine($"    {name,-34}{m.Result.Count,8:N0}{m.Milliseconds,9:N1} ms{m.Megabytes,9:N2} MB");
}

public record EmployeeRow(int Id, string FirstName, string LastName, string Title);
