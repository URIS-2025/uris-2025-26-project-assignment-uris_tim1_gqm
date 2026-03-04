export interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    organizationId: string;
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
    user: User;
}

export const ALL_PERMISSIONS = [
    'view_goals',
    'create_goals',
    'edit_goals',
    'delete_goals',
    'view_all_departments',
    'manage_departments',
    'manage_organizations',
    'view_premises',
    'create_premises',
    'edit_premises',
    'view_assessments',
    'create_assessments',
    'view_gqm',
    'create_gqm',
    'admin',
] as const;

export type Permission = typeof ALL_PERMISSIONS[number];
