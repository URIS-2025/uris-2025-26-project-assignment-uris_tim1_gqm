import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { parseApiError, getErrorMessage, ErrorTypes } from '../models/api-error.model';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const router = inject(Router);
    const toast = inject(ToastService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            // Try to parse the standardized error response
            const apiError = parseApiError(error.error);

            if (apiError) {
                // Handle based on error type
                switch (apiError.type) {
                    case ErrorTypes.UNAUTHORIZED:
                        // 401 is handled by auth interceptor, no toast needed
                        break;

                    case ErrorTypes.FORBIDDEN:
                        toast.showError('You do not have permission to perform this action.');
                        router.navigate(['/auth/forbidden']);
                        break;

                    case ErrorTypes.NOT_FOUND:
                        toast.showWarning(apiError.title);
                        break;

                    case ErrorTypes.VALIDATION_ERROR:
                        toast.showError(getErrorMessage(apiError));
                        break;

                    case ErrorTypes.CONFLICT:
                        toast.showWarning(apiError.title);
                        break;

                    case ErrorTypes.BAD_REQUEST:
                    case ErrorTypes.UNPROCESSABLE_ENTITY:
                        toast.showError(getErrorMessage(apiError));
                        break;

                    case ErrorTypes.INTERNAL_SERVER_ERROR:
                        toast.showError('An unexpected error occurred. Please try again later.');
                        break;

                    default:
                        toast.showError(apiError.title || 'An error occurred.');
                        break;
                }
            } else {
                // Fallback for non-standardized errors (e.g., network errors)
                switch (error.status) {
                    case 0:
                        toast.showError('Unable to connect to the server. Please check your connection.');
                        break;
                    case 403:
                        toast.showError('You do not have permission to perform this action.');
                        router.navigate(['/auth/forbidden']);
                        break;
                    case 500:
                    case 502:
                    case 503:
                        toast.showError('A server error occurred. Please try again later.');
                        break;
                }
            }

            return throwError(() => error);
        }),
    );
};
