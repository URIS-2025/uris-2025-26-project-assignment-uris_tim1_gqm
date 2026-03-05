import {
    HttpInterceptorFn,
    HttpRequest,
    HttpHandlerFn,
    HttpErrorResponse,
    HttpEvent,
} from '@angular/common/http';
import { inject } from '@angular/core';
import { Observable, throwError, BehaviorSubject, filter, take, switchMap, catchError } from 'rxjs';
import { AuthService } from '../auth/auth.service';

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (
    req: HttpRequest<unknown>,
    next: HttpHandlerFn,
): Observable<HttpEvent<unknown>> => {
    const auth = inject(AuthService);

    // Only intercept API requests
    if (!req.url.startsWith('/api')) {
        return next(req);
    }

    // Don't add auth headers to auth endpoints that don't need them
    const isAuthRequest =
        req.url.includes('/auth/login') ||
        req.url.includes('/auth/refresh');

    const clonedReq = isAuthRequest ? req : addAuthHeaders(req, auth);

    return next(clonedReq).pipe(
        catchError((error: HttpErrorResponse) => {
            // Only attempt refresh on 401 for non-auth requests
            if (error.status === 401 && !isAuthRequest) {
                return handle401Error(req, next, auth);
            }
            return throwError(() => error);
        }),
    );
};

function addAuthHeaders(req: HttpRequest<unknown>, auth: AuthService): HttpRequest<unknown> {
    const token = auth.accessToken;
    const orgId = auth.organizationId;

    let headers = req.headers;
    if (token) {
        headers = headers.set('Authorization', `Bearer ${token}`);
    }
    if (orgId) {
        headers = headers.set('X-Organization-Id', orgId);
    }

    return req.clone({ headers });
}

function handle401Error(
    req: HttpRequest<unknown>,
    next: HttpHandlerFn,
    auth: AuthService,
): Observable<HttpEvent<unknown>> {
    if (!isRefreshing) {
        isRefreshing = true;
        refreshTokenSubject.next(null);

        return new Observable<HttpEvent<unknown>>(subscriber => {
            auth.refresh()
                .then(() => {
                    isRefreshing = false;
                    const newToken = auth.accessToken!;
                    refreshTokenSubject.next(newToken);

                    // Retry the original request with new token
                    next(addAuthHeaders(req, auth)).subscribe(subscriber);
                })
                .catch(() => {
                    isRefreshing = false;
                    refreshTokenSubject.next(null);
                    auth.logout();
                    subscriber.error(new HttpErrorResponse({ status: 401 }));
                });
        });
    }

    // If already refreshing, wait for the new token then retry
    return refreshTokenSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap(() => next(addAuthHeaders(req, auth))),
    );
}
