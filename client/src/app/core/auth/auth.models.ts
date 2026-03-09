export interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    organizationId: string | null;
    organization: { id: string; name: string } | null;
    organizations: { id: string; name: string }[];
    isSystemAdmin: boolean;
    permissions: string[];
    managedDepartmentIds: string[];
}

export interface AuthState {
    user: User | null;
    accessToken: string | null;
    refreshToken: string | null;
    isAuthenticated: boolean;
}

export interface LoginRequest {
    email: string;
    password: string;
}

export interface LoginResponse {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
}

export interface RefreshResponse {
    accessToken: string;
    refreshToken: string;
    expiresAt: string;
}

// Aligned with backend UserService.Domain.Constants.Permissions
export const ALL_PERMISSIONS = [
    'manage_organizations',
    'manage_users',
    'manage_roles',
    'manage_departments',
    'view_all_departments',
    'create_goals',
    'edit_goals',
    'delete_goals',
    'view_goals',
    'manage_goal_influences',
    'record_measurements',
    'manage_probability_assessments',
    'view_analytics',
] as const;

export type Permission = typeof ALL_PERMISSIONS[number];
