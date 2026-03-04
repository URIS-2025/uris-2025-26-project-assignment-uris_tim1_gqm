import { Injectable } from '@angular/core';
import { AuthService } from '../auth/auth.service';

@Injectable({ providedIn: 'root' })
export class PermissionService {
    constructor(private auth: AuthService) { }

    has(permission: string): boolean {
        const user = this.auth.currentUser;
        if (!user) return false;
        return user.permissions.includes(permission) || user.permissions.includes('admin');
    }

    hasAny(permissions: string[]): boolean {
        return permissions.some(p => this.has(p));
    }

    hasAll(permissions: string[]): boolean {
        return permissions.every(p => this.has(p));
    }
}
