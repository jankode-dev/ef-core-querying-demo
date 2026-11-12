using System.Diagnostics;
using EfCoreQueryingDemo.Data;
using Microsoft.EntityFrameworkCore;

namespace EfCoreQueryingDemo.Scenarios;

public static class AsyncQueries
{
    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("2. Async queries");
        Console.WriteLine("  Monthly payroll of all employees (AsNoTracking)");
        Console.WriteLine();
        Console.WriteLine($"    {"Approach",-34}{"First row",12}{"All rows",12}  Result");

        await using var db = new CompanyDbContext();

        await Print("ToListAsync", () => WithToListAsync(db));
        await Print("AsAsyncEnumerable + await foreach", () => WithAsyncEnumerable(db));
    }

    static async Task Print(string name, Func<Task<Timing>> run)
    {
        await run();
        var timing = await run();

        Console.WriteLine($"    {name,-34}{timing.FirstRow.TotalMilliseconds,9:N1} ms{timing.AllRows.TotalMilliseconds,9:N1} ms  payroll {timing.Payroll:N2}");
    }

    static async Task<Timing> WithToListAsync(CompanyDbContext db)
    {
        var stopwatch = Stopwatch.StartNew();
        var employees = await db.Employees.AsNoTracking().ToListAsync();
        var firstRow = stopwatch.Elapsed;

        var payroll = 0m;
        foreach (var employee in employees)
            payroll += employee.Salary;

        return new Timing(firstRow, stopwatch.Elapsed, payroll);
    }

    static async Task<Timing> WithAsyncEnumerable(CompanyDbContext db)
    {
        var stopwatch = Stopwatch.StartNew();
        TimeSpan? firstRow = null;

        var payroll = 0m;
        await foreach (var employee in db.Employees.AsNoTracking().AsAsyncEnumerable())
        {
            firstRow ??= stopwatch.Elapsed;
            payroll += employee.Salary;
        }

        return new Timing(firstRow ?? stopwatch.Elapsed, stopwatch.Elapsed, payroll);
    }

    record Timing(TimeSpan FirstRow, TimeSpan AllRows, decimal Payroll);
}
