using EfCoreQueryingDemo.Data;
using EfCoreQueryingDemo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCoreQueryingDemo.Scenarios;

public static class TemporalTables
{
    const string TimeFormat = "yyyy-MM-dd HH:mm:ss.fff";

    public static async Task RunAsync()
    {
        Console.WriteLine();
        Console.WriteLine("3. Temporal tables");
        Console.WriteLine("  New employee gets promoted twice and then leaves (Employees + EmployeesHistory)");

        await using var db = new CompanyDbContext();

        var development = await db.Departments.SingleAsync(d => d.Name == "Development");
        var employee = new Employee
        {
            FirstName = "Mila",
            LastName = "Stankovic",
            Title = "Junior",
            Salary = 1_100,
            HiredOn = DateTime.Today,
            DepartmentId = development.Id
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        await Promote(db, employee, "Medior", 1_750);
        await Promote(db, employee, "Senior", 2_600);

        var history = await db.Employees
            .TemporalAll()
            .Where(e => e.Id == employee.Id)
            .OrderBy(e => EF.Property<DateTime>(e, "PeriodStart"))
            .Select(e => new
            {
                e.Title,
                e.Salary,
                ValidFrom = EF.Property<DateTime>(e, "PeriodStart"),
                ValidTo = EF.Property<DateTime>(e, "PeriodEnd")
            })
            .ToListAsync();

        Console.WriteLine();
        Console.WriteLine($"    {"Title",-10}{"Salary",10}   {"Valid from (UTC)",-26}Valid to (UTC)");
        foreach (var version in history)
        {
            var validTo = version.ValidTo.Year == 9999 ? "current" : version.ValidTo.ToString(TimeFormat);
            Console.WriteLine($"    {version.Title,-10}{version.Salary,10:N2}   {version.ValidFrom.ToString(TimeFormat),-26}{validTo}");
        }

        var pointInTime = history[1].ValidFrom;
        var asOf = await db.Employees
            .TemporalAsOf(pointInTime)
            .AsNoTracking()
            .SingleAsync(e => e.Id == employee.Id);

        Console.WriteLine();
        Console.WriteLine($"    TemporalAsOf({pointInTime.ToString(TimeFormat)}): {asOf.Title}, {asOf.Salary:N2}");

        db.Employees.Remove(employee);
        await db.SaveChangesAsync();

        var current = await db.Employees.CountAsync(e => e.Id == employee.Id);
        var versions = await db.Employees.TemporalAll().CountAsync(e => e.Id == employee.Id);

        Console.WriteLine($"    After Remove + SaveChanges: {current} rows in Employees, {versions} versions in history");
    }

    static async Task Promote(CompanyDbContext db, Employee employee, string title, decimal salary)
    {
        await Task.Delay(200);

        employee.Title = title;
        employee.Salary = salary;
        await db.SaveChangesAsync();
    }
}
