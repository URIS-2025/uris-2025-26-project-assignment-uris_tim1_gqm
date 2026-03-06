import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Department, DepartmentRequest,
    Organization, OrganizationRequest,
    PaginatedResponse, PagedParams
} from './api.models';

@Injectable({ providedIn: 'root' })
export class DepartmentApiService {
    private readonly base = environment.apiBaseUrl;

    constructor(private http: HttpClient) { }

    // Departments
    getDepartments(params: PagedParams = {}): Observable<PaginatedResponse<Department>> {
        const p = new HttpParams()
            .set('page', params.page ?? 1)
            .set('size', params.size ?? 20);
        return this.http.get<PaginatedResponse<Department>>(`${this.base}/department`, { params: p });
    }

    getDepartmentById(id: string): Observable<Department> {
        return this.http.get<Department>(`${this.base}/department/${id}`);
    }

    getDepartmentsByOrg(orgId: string, params: PagedParams = {}): Observable<PaginatedResponse<Department>> {
        const p = new HttpParams()
            .set('page', params.page ?? 1)
            .set('size', params.size ?? 100);
        return this.http.get<PaginatedResponse<Department>>(`${this.base}/department/organization/${orgId}`, { params: p });
    }

    createDepartment(req: DepartmentRequest): Observable<Department> {
        return this.http.post<Department>(`${this.base}/department`, req);
    }

    updateDepartment(id: string, req: DepartmentRequest): Observable<Department> {
        return this.http.put<Department>(`${this.base}/department/${id}`, req);
    }

    deleteDepartment(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/department/${id}`);
    }

    // Organizations
    getOrganizations(params: PagedParams = {}): Observable<PaginatedResponse<Organization>> {
        const p = new HttpParams()
            .set('page', params.page ?? 1)
            .set('size', params.size ?? 20);
        return this.http.get<PaginatedResponse<Organization>>(`${this.base}/organization`, { params: p });
    }

    getOrganizationById(id: string): Observable<Organization> {
        return this.http.get<Organization>(`${this.base}/organization/${id}`);
    }

    createOrganization(req: OrganizationRequest): Observable<Organization> {
        return this.http.post<Organization>(`${this.base}/organization`, req);
    }

    updateOrganization(id: string, req: OrganizationRequest): Observable<Organization> {
        return this.http.put<Organization>(`${this.base}/organization/${id}`, req);
    }

    deleteOrganization(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/organization/${id}`);
    }
}
