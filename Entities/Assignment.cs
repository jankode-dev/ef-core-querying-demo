namespace EfCoreQueryingDemo.Entities;

public class Assignment
{
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public string Role { get; set; } = "";

    public Employee Employee { get; set; } = null!;
    public Project Project { get; set; } = null!;
}
