namespace EfCoreQueryingDemo.Entities;

public class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Title { get; set; } = "";
    public decimal Salary { get; set; }
    public DateTime HiredOn { get; set; }
    public int DepartmentId { get; set; }

    public Department Department { get; set; } = null!;
    public List<Assignment> Assignments { get; set; } = [];
}
