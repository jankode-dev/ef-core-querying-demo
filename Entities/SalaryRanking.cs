namespace EfCoreQueryingDemo.Entities;

public class SalaryRanking
{
    public int EmployeeId { get; set; }
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public string Title { get; set; } = "";
    public decimal Salary { get; set; }
    public long SalaryRank { get; set; }
    public int DepartmentSize { get; set; }
}
