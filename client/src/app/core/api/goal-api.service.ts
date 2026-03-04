import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Goal, GoalDetails, GoalRequest,
    Strategy, StrategyRequest,
    GoalInfluence, GoalInfluenceRequest,
    PaginationParams, PaginatedResponse
} from './api.models';

@Injectable({ providedIn: 'root' })
export class GoalApiService {
    private readonly base = `${environment.apiBaseUrl}/goal`;

    constructor(private http: HttpClient) { }

    // Goals
    getAll(params: PaginationParams = {}): Observable<PaginatedResponse<Goal>> {
        const p = new HttpParams()
            .set('pageNumber', params.pageNumber ?? 1)
            .set('pageSize', params.pageSize ?? 20)
            .set('orderBy', params.orderBy ?? 'createdAt');
        return this.http.get<PaginatedResponse<Goal>>(`${this.base}/api/Goal`, { params: p });
    }

    getById(id: string): Observable<Goal> {
        return this.http.get<Goal>(`${this.base}/api/Goal/${id}`);
    }

    getDetails(id: string): Observable<GoalDetails> {
        return this.http.get<GoalDetails>(`${this.base}/api/Goal/${id}/details`);
    }

    getByDepartment(departmentId: string): Observable<Goal[]> {
        return this.http.get<Goal[]>(`${this.base}/api/Goal/department/${departmentId}`);
    }

    create(req: GoalRequest): Observable<Goal> {
        return this.http.post<Goal>(`${this.base}/api/Goal`, req);
    }

    update(id: string, req: GoalRequest): Observable<Goal> {
        return this.http.put<Goal>(`${this.base}/api/Goal/${id}`, req);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/api/Goal/${id}`);
    }

    // Strategy
    getStrategyByGoal(goalId: string): Observable<Strategy> {
        return this.http.get<Strategy>(`${this.base}/api/Strategy/goal/${goalId}`);
    }

    getStrategyById(id: string): Observable<Strategy> {
        return this.http.get<Strategy>(`${this.base}/api/Strategy/${id}`);
    }

    createStrategy(req: StrategyRequest): Observable<Strategy> {
        return this.http.post<Strategy>(`${this.base}/api/Strategy`, req);
    }

    updateStrategy(id: string, req: StrategyRequest): Observable<Strategy> {
        return this.http.put<Strategy>(`${this.base}/api/Strategy/${id}`, req);
    }

    deleteStrategy(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/api/Strategy/${id}`);
    }

    // GoalInfluence
    getInfluencesByGoal(goalId: string): Observable<GoalInfluence[]> {
        return this.http.get<GoalInfluence[]>(`${this.base}/api/GoalInfluence/goal/${goalId}`);
    }

    getInfluencesByStrategy(strategyId: string): Observable<GoalInfluence[]> {
        return this.http.get<GoalInfluence[]>(`${this.base}/api/GoalInfluence/strategy/${strategyId}`);
    }

    createInfluence(req: GoalInfluenceRequest): Observable<GoalInfluence> {
        return this.http.post<GoalInfluence>(`${this.base}/api/GoalInfluence`, req);
    }

    deleteInfluence(goalId: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/api/GoalInfluence/${goalId}`);
    }
}
