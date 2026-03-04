import { HttpInterceptorFn, HttpRequest, HttpHandlerFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (
    req: HttpRequest<unknown>,
    next: HttpHandlerFn
) => {
    const auth = inject(AuthService);

    // Only add headers to API requests
    if (!req.url.startsWith('/api')) {
        return next(req);
    }

    const token = auth.accessToken;
    const orgId = auth.organizationId;

    let headers = req.headers;
    if (token) {
        headers = headers.set('Authorization', `Bearer ${token}`);
    }
    if (orgId) {
        headers = headers.set('X-Organization-Id', orgId);
    }

    return next(req.clone({ headers }));
};
