import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
    const auth = inject(AuthService);
    const router = inject(Router);

    if (auth.currentState.isAuthenticated) {
        return true;
    }

    return router.createUrlTree(['/auth/login'], {
        queryParams: { returnUrl: state.url }
    });
};
