import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../permissions/permission.service';

export const permissionGuard: CanActivateFn = (route) => {
    const permissions = inject(PermissionService);
    const router = inject(Router);

    const required: string[] = route.data?.['permissions'] ?? [];
    const anyRequired: string[] = route.data?.['anyPermissions'] ?? [];

    const hasNoRequirements = required.length === 0 && anyRequired.length === 0;
    const meetsAllReqs = required.length > 0 && permissions.hasAll(required);
    const meetsAnyReqs = anyRequired.length > 0 && permissions.hasAny(anyRequired);

    if (hasNoRequirements || meetsAllReqs || meetsAnyReqs) {
        return true;
    }

    return router.createUrlTree(['/auth/forbidden']);
};
