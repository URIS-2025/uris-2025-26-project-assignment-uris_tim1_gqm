/**
 * Standardized API error response matching the backend format.
 * This interface should be used to parse error responses from all backend services.
 */
export interface ApiErrorResponse {
    type: string;
    title: string;
    errors: Record<string, string[]>;
}

/**
 * Known error types returned by the backend.
 */
export const ErrorTypes = {
    VALIDATION_ERROR: 'validation_error',
    BAD_REQUEST: 'bad_request',
    NOT_FOUND: 'not_found',
    UNAUTHORIZED: 'unauthorized',
    FORBIDDEN: 'forbidden',
    CONFLICT: 'conflict',
    UNPROCESSABLE_ENTITY: 'unprocessable_entity',
    INTERNAL_SERVER_ERROR: 'internal_server_error',
} as const;

/**
 * Attempts to parse an HttpErrorResponse body into an ApiErrorResponse.
 * Returns null if the body doesn't match the expected format.
 */
export function parseApiError(errorBody: unknown): ApiErrorResponse | null {
    if (
        errorBody &&
        typeof errorBody === 'object' &&
        'type' in errorBody &&
        'title' in errorBody
    ) {
        const raw = errorBody as Record<string, unknown>;
        return {
            type: String(raw['type'] ?? ''),
            title: String(raw['title'] ?? ''),
            errors: (raw['errors'] as Record<string, string[]>) ?? {},
        };
    }
    return null;
}

/**
 * Extracts a human-readable message from an ApiErrorResponse.
 * Combines the title with field-level error details when available.
 */
export function getErrorMessage(apiError: ApiErrorResponse): string {
    const fieldErrors = Object.entries(apiError.errors)
        .flatMap(([, messages]) => messages);

    if (fieldErrors.length > 0) {
        return `${apiError.title}: ${fieldErrors.join(', ')}`;
    }

    return apiError.title;
}
