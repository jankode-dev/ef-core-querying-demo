namespace EfCoreQueryingDemo.Entities;

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";

    public List<Employee> Employees { get; set; } = [];
}
