import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, firstValueFrom } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import {
    AuthState,
    LoginRequest,
    LoginResponse,
    RefreshResponse,
    User,
} from './auth.models';

const ACCESS_TOKEN_KEY = 'gqm_access_token';
const REFRESH_TOKEN_KEY = 'gqm_refresh_token';

const INITIAL_STATE: AuthState = {
    user: null,
    accessToken: null,
    refreshToken: null,
    isAuthenticated: false,
};

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly _state$ = new BehaviorSubject<AuthState>(INITIAL_STATE);
    private readonly apiUrl = `${environment.apiBaseUrl}/user/auth`;

    readonly state$ = this._state$.asObservable();
    readonly user$: Observable<User | null> = this._state$.pipe(map(s => s.user));
    readonly isAuthenticated$: Observable<boolean> = this._state$.pipe(map(s => s.isAuthenticated));
    readonly token$: Observable<string | null> = this._state$.pipe(map(s => s.accessToken));

    constructor(
        private http: HttpClient,
        private router: Router,
    ) {
        this._restoreTokensFromStorage();
    }

    get currentState(): AuthState {
        return this._state$.value;
    }

    get currentUser(): User | null {
        return this._state$.value.user;
    }

    get accessToken(): string | null {
        return this._state$.value.accessToken;
    }

    get refreshToken(): string | null {
        return this._state$.value.refreshToken;
    }

    get organizationId(): string | null {
        return this._state$.value.user?.organizationId ?? null;
    }

    /**
     * Login flow:
     * 1. POST /auth/login → get tokens
     * 2. Store tokens
     * 3. GET /auth/me → load user context
     * 4. Store user in state
     */
    async login(req: LoginRequest): Promise<void> {
        const response = await firstValueFrom(
            this.http.post<LoginResponse>(`${this.apiUrl}/login`, req),
        );

        this._storeTokens(response.accessToken, response.refreshToken);
        this._updateState({ accessToken: response.accessToken, refreshToken: response.refreshToken });

        await this.loadUser();
    }

    /**
     * Refresh flow:
     * POST /auth/refresh → get new access token + rotated refresh token
     */
    async refresh(): Promise<void> {
        const currentRefreshToken = this.refreshToken;
        if (!currentRefreshToken) {
            throw new Error('No refresh token available');
        }

        const response = await firstValueFrom(
            this.http.post<RefreshResponse>(`${this.apiUrl}/refresh`, {
                refreshToken: currentRefreshToken,
            }),
        );

        this._storeTokens(response.accessToken, response.refreshToken);
        this._updateState({ accessToken: response.accessToken, refreshToken: response.refreshToken });
    }

    /**
     * Logout flow:
     * POST /auth/logout → invalidate refresh token server-side
     * Clear all local state
     */
    logout(): void {
        const token = this.accessToken;
        if (token) {
            // Fire-and-forget: server-side cleanup
            this.http
                .post(`${this.apiUrl}/logout`, {})
                .subscribe({ error: () => {} });
        }

        this._clearAll();
        this.router.navigate(['/auth/login']);
    }

    /**
     * Load user context from backend.
     * Called on app init (if tokens exist) and after login.
     */
    async loadUser(): Promise<void> {
        try {
            const user = await firstValueFrom(
                this.http.get<User>(`${this.apiUrl}/me`),
            );

            this._setState({
                user,
                accessToken: this._state$.value.accessToken,
                refreshToken: this._state$.value.refreshToken,
                isAuthenticated: true,
            });
        } catch {
            // Token is invalid or expired — clean up
            this._clearAll();
        }
    }

    private _updateState(partial: Partial<AuthState>): void {
        this._setState({ ...this._state$.value, ...partial });
    }

    private _setState(state: AuthState): void {
        this._state$.next(state);
    }

    private _storeTokens(accessToken: string, refreshToken: string): void {
        try {
            localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
            localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
        } catch {
            // Storage quota exceeded or private mode
        }
    }

    private _restoreTokensFromStorage(): void {
        try {
            const accessToken = localStorage.getItem(ACCESS_TOKEN_KEY);
            const refreshToken = localStorage.getItem(REFRESH_TOKEN_KEY);
            if (accessToken && refreshToken) {
                this._updateState({ accessToken, refreshToken });
            }
        } catch {
            // Invalid storage state
        }
    }

    private _clearAll(): void {
        localStorage.removeItem(ACCESS_TOKEN_KEY);
        localStorage.removeItem(REFRESH_TOKEN_KEY);
        this._setState(INITIAL_STATE);
    }
}
