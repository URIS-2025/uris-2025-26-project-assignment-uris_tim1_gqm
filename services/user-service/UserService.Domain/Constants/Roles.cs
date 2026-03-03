namespace UserService.Domain.Constants;

public static class Roles
{
    public const string SystemAdmin = "System Admin";
    public const string OrganizationAdmin = "Organization Admin";
    public const string DepartmentManager = "Department Manager";
    public const string Analyst = "Analyst";
    public const string Viewer = "Viewer";

    public static bool IsSystemRole(string name) =>
        name is SystemAdmin
            or OrganizationAdmin
            or DepartmentManager
            or Analyst
            or Viewer;
}
