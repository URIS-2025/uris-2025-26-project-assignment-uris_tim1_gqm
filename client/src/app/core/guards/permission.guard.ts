import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../permissions/permission.service';

export const permissionGuard: CanActivateFn = (route) => {
    const permissions = inject(PermissionService);
    const router = inject(Router);

    const required: string[] = route.data?.['permissions'] ?? [];

    if (required.length === 0 || permissions.hasAll(required)) {
        return true;
    }

    return router.createUrlTree(['/auth/forbidden']);
};
