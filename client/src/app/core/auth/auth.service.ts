import { Injectable, signal, computed } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AuthState, LoginRequest, User, ALL_PERMISSIONS } from './auth.models';

const STORAGE_KEY = 'gqm_auth';

// -------------------------------------------------------
// Mock admin user — swap with real API when access-service
// implements JWT endpoints.
// -------------------------------------------------------
const MOCK_ADMIN_USER: User = {
    id: '00000000-0000-0000-0000-000000000001',
    email: 'admin@gqm.local',
    firstName: 'Admin',
    lastName: 'User',
    organizationId: '00000000-0000-0000-0000-000000000010',
    permissions: [...ALL_PERMISSIONS],
    managedDepartmentIds: [],
};

const MOCK_VIEWER_USER: User = {
    id: '00000000-0000-0000-0000-000000000002',
    email: 'viewer@gqm.local',
    firstName: 'Viewer',
    lastName: 'User',
    organizationId: '00000000-0000-0000-0000-000000000010',
    permissions: ['view_goals', 'view_premises', 'view_assessments', 'view_gqm', 'view_all_departments'],
    managedDepartmentIds: [],
};

const INITIAL_STATE: AuthState = {
    user: null,
    accessToken: null,
    refreshToken: null,
    isAuthenticated: false,
};

@Injectable({ providedIn: 'root' })
export class AuthService {
    private readonly _state$ = new BehaviorSubject<AuthState>(INITIAL_STATE);

    readonly state$ = this._state$.asObservable();
    readonly user$: Observable<User | null> = this._state$.pipe(map(s => s.user));
    readonly isAuthenticated$: Observable<boolean> = this._state$.pipe(map(s => s.isAuthenticated));
    readonly token$: Observable<string | null> = this._state$.pipe(map(s => s.accessToken));

    constructor(private router: Router) {
        this._restoreFromStorage();
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

    get organizationId(): string | null {
        return this._state$.value.user?.organizationId ?? null;
    }

    /**
     * Mock login. Returns admin user for any credentials.
     * Viewer mode: use viewer@gqm.local to simulate limited permissions.
     */
    login(req: LoginRequest): Promise<void> {
        return new Promise(resolve => {
            setTimeout(() => {
                const user = req.email === 'viewer@gqm.local' ? MOCK_VIEWER_USER : MOCK_ADMIN_USER;
                const mockToken = `mock.jwt.${btoa(JSON.stringify({ sub: user.id, exp: Date.now() + 86400000 }))}`;

                const newState: AuthState = {
                    user,
                    accessToken: mockToken,
                    refreshToken: `mock.refresh.${user.id}`,
                    isAuthenticated: true,
                };

                this._setState(newState);
                this._persist(newState);
                resolve();
            }, 400); // Simulates network latency
        });
    }

    logout(): void {
        localStorage.removeItem(STORAGE_KEY);
        this._setState(INITIAL_STATE);
        this.router.navigate(['/auth/login']);
    }

    private _setState(state: AuthState): void {
        this._state$.next(state);
    }

    private _persist(state: AuthState): void {
        try {
            localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
        } catch {
            // Storage quota exceeded or private mode
        }
    }

    private _restoreFromStorage(): void {
        try {
            const raw = localStorage.getItem(STORAGE_KEY);
            if (raw) {
                const state: AuthState = JSON.parse(raw);
                if (state.isAuthenticated && state.accessToken) {
                    this._setState(state);
                }
            }
        } catch {
            // Invalid JSON
        }
    }
}
