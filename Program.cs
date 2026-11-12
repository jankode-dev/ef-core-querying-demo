using System.Diagnostics;
using EfCoreQueryingDemo.Data;
using EfCoreQueryingDemo.Scenarios;
using Microsoft.EntityFrameworkCore;

await using (var db = new CompanyDbContext())
{
    if (args.Contains("--reset"))
        await db.Database.EnsureDeletedAsync();

    await db.Database.EnsureCreatedAsync();
    await DatabaseObjects.CreateAsync(db);

    if (!await db.Employees.AnyAsync())
    {
        Console.Write("Seeding EfCoreQueryingDemo database... ");
        var stopwatch = Stopwatch.StartNew();
        await Seeder.SeedAsync(db);
        Console.WriteLine($"done in {stopwatch.Elapsed.TotalSeconds:N1} s");
    }

    var departments = await db.Departments.CountAsync();
    var employees = await db.Employees.CountAsync();
    var projects = await db.Projects.CountAsync();
    var assignments = await db.Assignments.CountAsync();

    Console.WriteLine($"EfCoreQueryingDemo on (localdb)\\MSSQLLocalDB: {departments} departments, {employees:N0} employees, {projects} projects, {assignments:N0} assignments");
}

await LinqQueries.RunAsync();
await AsyncQueries.RunAsync();
await TemporalTables.RunAsync();
await ViewsAndProcedures.RunAsync();
await Paging.RunAsync();
