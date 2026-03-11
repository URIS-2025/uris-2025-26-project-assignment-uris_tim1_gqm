import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Assessment, AssessmentRequest, PaginatedResponse, PagedParams } from './api.models';

@Injectable({ providedIn: 'root' })
export class AssessmentApiService {
    private readonly base = `${environment.apiBaseUrl}/assessment`;

    constructor(private http: HttpClient) { }

    getAll(params: PagedParams = {}): Observable<PaginatedResponse<Assessment>> {
        const p = new HttpParams()
            .set('page', params.page ?? 1)
            .set('size', params.size ?? 20);
        return this.http.get<PaginatedResponse<Assessment>>(`${this.base}`, { params: p });
    }

    getById(id: string): Observable<Assessment> {
        return this.http.get<Assessment>(`${this.base}/${id}`);
    }

    getByGoal(goalId: string): Observable<Assessment[]> {
        return this.http.get<Assessment[]>(`${this.base}/goal/${goalId}`);
    }

    create(req: AssessmentRequest): Observable<Assessment> {
        return this.http.post<Assessment>(`${this.base}`, req);
    }

    update(id: string, req: AssessmentRequest): Observable<Assessment> {
        return this.http.put<Assessment>(`${this.base}/${id}`, req);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/${id}`);
    }
}
