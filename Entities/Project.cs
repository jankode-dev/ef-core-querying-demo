namespace EfCoreQueryingDemo.Entities;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public DateTime StartedOn { get; set; }

    public List<Assignment> Assignments { get; set; } = [];
}
