import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Premise, PremiseRequest, PaginatedResponse, PagedParams } from './api.models';

@Injectable({ providedIn: 'root' })
export class PremiseApiService {
    private readonly base = `${environment.apiBaseUrl}/premise`;

    constructor(private http: HttpClient) { }

    getAll(params: PagedParams = {}): Observable<PaginatedResponse<Premise>> {
        const p = new HttpParams()
            .set('page', params.page ?? 1)
            .set('size', params.size ?? 20);
        return this.http.get<PaginatedResponse<Premise>>(`${this.base}/premises`, { params: p });
    }

    getById(id: string): Observable<Premise> {
        return this.http.get<Premise>(`${this.base}/premises/${id}`);
    }

    getActiveByGoal(goalId: string): Observable<Premise[]> {
        return this.http.get<Premise[]>(`${this.base}/premises/active/goal/${goalId}`);
    }

    getActiveByStrategy(strategyId: string): Observable<Premise[]> {
        return this.http.get<Premise[]>(`${this.base}/premises/active/strategy/${strategyId}`);
    }

    create(req: PremiseRequest): Observable<Premise> {
        return this.http.post<Premise>(`${this.base}/premises`, req);
    }

    /** PUT creates a new version */
    update(id: string, req: PremiseRequest): Observable<Premise> {
        return this.http.put<Premise>(`${this.base}/premises/${id}`, req);
    }

    /** DELETE is soft-delete */
    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/premises/${id}`);
    }
}
