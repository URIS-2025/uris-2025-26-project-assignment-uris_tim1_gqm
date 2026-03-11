namespace UserService.Domain.Constants;

public static class Permissions
{
    public const string ManageOrganizations = "manage_organizations";
    public const string ManageUsers = "manage_users";
    public const string ManageRoles = "manage_roles";
    public const string ManagePermissions = "manage_permissions";
    public const string ManageUserRoles = "manage_user_roles";
    public const string ManageDepartments = "manage_departments";
    public const string ViewAllDepartments = "view_all_departments";
    public const string CreateGoals = "create_goals";
    public const string EditGoals = "edit_goals";
    public const string DeleteGoals = "delete_goals";
    public const string ViewGoals = "view_goals";
    public const string ManageGoalInfluences = "manage_goal_influences";
    public const string ManagePremises = "manage_premises";
    public const string RecordMeasurements = "record_measurements";
    public const string ManageProbabilityAssessments = "manage_probability_assessments";
    public const string ViewAnalytics = "view_analytics";

    public static bool IsSystemPermission(string name) =>
        name is ManageOrganizations
            or ManageUsers
            or ManageRoles
            or ManagePermissions
            or ManageUserRoles
            or ManageDepartments
            or ViewAllDepartments
            or CreateGoals
            or EditGoals
            or DeleteGoals
            or ViewGoals
            or ManageGoalInfluences
            or ManagePremises
            or RecordMeasurements
            or ManageProbabilityAssessments
            or ViewAnalytics;
}
