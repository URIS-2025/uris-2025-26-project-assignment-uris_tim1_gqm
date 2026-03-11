namespace DepartmentService.Domain.Entities;

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Department> Departments { get; set; } = new List<Department>();
}
