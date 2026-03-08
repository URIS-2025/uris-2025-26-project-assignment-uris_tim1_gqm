import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    User, UserRequest, UpdateProfileRequest,
    Role, AssignRoleRequest,
    PaginatedResponse, PagedParams
} from './api.models';

@Injectable({ providedIn: 'root' })
export class UserApiService {
    private readonly base = environment.apiBaseUrl;

    constructor(private http: HttpClient) { }

    getUsers(params: PagedParams = {}): Observable<PaginatedResponse<User>> {
        const p = new HttpParams()
            .set('page', params.page ?? 1)
            .set('size', params.size ?? 50);
        return this.http.get<PaginatedResponse<User>>(`${this.base}/user`, { params: p });
    }

    getUserById(id: string): Observable<User> {
        return this.http.get<User>(`${this.base}/user/${id}`);
    }

    createUser(req: UserRequest): Observable<User> {
        return this.http.post<User>(`${this.base}/user`, req);
    }

    updateProfile(id: string, req: UpdateProfileRequest): Observable<User> {
        return this.http.put<User>(`${this.base}/user/${id}`, req);
    }

    toggleActive(id: string): Observable<User> {
        return this.http.put<User>(`${this.base}/user/${id}/toggle-active`, {});
    }

    deleteUser(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/user/${id}`);
    }

    // Roles & Organization Mapping
    getRoles(): Observable<Role[]> {
        return this.http.get<Role[]>(`${this.base}/role`);
    }

    assignRole(req: AssignRoleRequest): Observable<any> {
        return this.http.post<any>(`${this.base}/userorganizationrole`, req);
    }
}
