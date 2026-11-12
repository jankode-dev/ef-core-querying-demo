using EfCoreQueryingDemo.Entities;

namespace EfCoreQueryingDemo.Data;

public static class Seeder
{
    static readonly string[] FirstNames = ["Marko", "Nikola", "Stefan", "Luka", "Milos", "Nemanja", "Filip", "Vuk", "Jelena", "Ana", "Milica", "Jovana", "Teodora", "Sara", "Katarina", "Ivana"];
    static readonly string[] LastNames = ["Petrovic", "Jovanovic", "Nikolic", "Markovic", "Djordjevic", "Stojanovic", "Ilic", "Pavlovic", "Kovacevic", "Popovic", "Todorovic", "Lazic", "Savic", "Ristic", "Zivkovic", "Bozic"];
    static readonly string[] Projects = ["Webshop redesign", "Mobile banking", "ERP migration", "Data warehouse", "CRM integration", "Payment gateway", "Booking platform", "IoT dashboard", "HR portal", "Reporting API", "Support chatbot", "Inventory system"];
    static readonly string[] Roles = ["Developer", "Tester", "Analyst", "Designer"];

    static readonly (string Name, string City)[] Departments =
    [
        ("Development", "Beograd"), ("QA", "Novi Sad"), ("DevOps", "Beograd"), ("Data", "Novi Sad"),
        ("Sales", "Nis"), ("Marketing", "Beograd"), ("Finance", "Kragujevac"), ("HR", "Beograd")
    ];

    static readonly (string Title, int MinSalary, int MaxSalary)[] Titles =
    [
        ("Junior", 900, 1_400), ("Medior", 1_400, 2_200), ("Senior", 2_200, 3_500), ("Lead", 3_000, 4_500)
    ];

    public static async Task SeedAsync(CompanyDbContext db, int employeeCount = 10_000)
    {
        var random = new Random(7);

        var departments = Departments.Select(d => new Department { Name = d.Name, City = d.City }).ToList();
        var projects = Projects.Select(name => new Project { Name = name, StartedOn = DateTime.Today.AddDays(-random.Next(1_000)) }).ToList();

        var employees = new List<Employee>();
        for (var i = 0; i < employeeCount; i++)
        {
            var roll = random.Next(100);
            var (title, minSalary, maxSalary) = roll < 35 ? Titles[0] : roll < 70 ? Titles[1] : roll < 92 ? Titles[2] : Titles[3];

            employees.Add(new Employee
            {
                FirstName = FirstNames[random.Next(FirstNames.Length)],
                LastName = LastNames[random.Next(LastNames.Length)],
                Title = title,
                Salary = random.Next(minSalary / 10, maxSalary / 10) * 10,
                HiredOn = DateTime.Today.AddDays(-random.Next(3_650)),
                Department = departments[random.Next(departments.Count)]
            });
        }

        var leads = employees.Where(e => e.Title == "Lead").OrderBy(_ => random.Next()).Take(projects.Count).ToList();
        for (var i = 0; i < projects.Count; i++)
            leads[i].Assignments.Add(new Assignment { Project = projects[i], Role = "Lead" });

        foreach (var employee in employees.Except(leads))
        {
            foreach (var project in projects.OrderBy(_ => random.Next()).Take(random.Next(3)))
                employee.Assignments.Add(new Assignment { Project = project, Role = Roles[random.Next(Roles.Length)] });
        }

        db.Departments.AddRange(departments);
        db.Projects.AddRange(projects);
        db.Employees.AddRange(employees);
        await db.SaveChangesAsync();
    }
}
