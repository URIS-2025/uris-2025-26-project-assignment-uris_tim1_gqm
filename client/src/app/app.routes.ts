import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

export const routes: Routes = [
    // Redirect root to dashboard
    { path: '', redirectTo: '/dashboard', pathMatch: 'full' },

    // Auth routes (no layout shell)
    {
        path: 'auth',
        children: [
            {
                path: 'login',
                loadComponent: () =>
                    import('./features/auth/login/login.component').then(m => m.LoginComponent),
            },
            {
                path: 'forbidden',
                loadComponent: () =>
                    import('./features/auth/forbidden/forbidden.component').then(m => m.ForbiddenComponent),
            },
            { path: '', redirectTo: 'login', pathMatch: 'full' },
        ]
    },

    // Protected routes (with layout shell)
    {
        path: '',
        canActivate: [authGuard],
        loadComponent: () =>
            import('./layout/layout.component').then(m => m.LayoutComponent),
        children: [
            {
                path: 'dashboard',
                loadComponent: () =>
                    import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
            },

            // Goals
            {
                path: 'goals',
                canActivate: [permissionGuard],
                data: { permissions: ['view_goals'] },
                children: [
                    {
                        path: '',
                        loadComponent: () =>
                            import('./features/goals/goals-list.component').then(m => m.GoalsListComponent),
                    },
                    {
                        path: 'new',
                        canActivate: [permissionGuard],
                        data: { permissions: ['create_goals'] },
                        loadComponent: () =>
                            import('./features/goals/goal-new/goal-new.component').then(m => m.GoalNewComponent),
                    },
                    {
                        path: ':id',
                        loadComponent: () =>
                            import('./features/goals/goal-detail.component').then(m => m.GoalDetailComponent),
                    },
                ]
            },

            // Analytics
            {
                path: 'analytics',
                canActivate: [permissionGuard],
                data: { permissions: ['view_analytics'] },
                children: [
                    {
                        path: '',
                        loadComponent: () =>
                            import('./features/analytics/analytics.component').then(m => m.AnalyticsComponent),
                    }
                ]
            },

            // Admin
            {
                path: 'admin',
                canActivate: [permissionGuard],
                data: { anyPermissions: ['manage_organizations', 'manage_users', 'manage_roles', 'manage_departments'] },
                children: [
                    {
                        path: 'organizations',
                        canActivate: [permissionGuard],
                        data: { permissions: ['manage_organizations'] },
                        loadComponent: () =>
                            import('./features/admin/organizations.component').then(m => m.OrganizationsComponent),
                    },
                    {
                        path: 'departments',
                        canActivate: [permissionGuard],
                        data: { permissions: ['manage_departments'] },
                        loadComponent: () =>
                            import('./features/admin/departments.component').then(m => m.DepartmentsComponent),
                    },
                    {
                        path: 'users',
                        canActivate: [permissionGuard],
                        data: { permissions: ['manage_users'] },
                        loadComponent: () =>
                            import('./features/admin/users/users.component').then(m => m.UsersComponent),
                    },
                    {
                        path: 'roles',
                        canActivate: [permissionGuard],
                        data: { permissions: ['manage_roles'] },
                        loadComponent: () =>
                            import('./features/admin/roles/roles.component').then(m => m.RolesComponent),
                    },
                    { path: '', redirectTo: 'organizations', pathMatch: 'full' },
                ]
            },
        ]
    },

    // Catch-all
    { path: '**', redirectTo: '/dashboard' },
];
