import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    Goal, GoalDetails, GoalRequest,
    Strategy, StrategyRequest,
    GoalInfluence, GoalInfluenceRequest,
    PaginationParams, PaginatedResponse,
    GoalTreeNode, GoalAnalytics
} from './api.models';

@Injectable({ providedIn: 'root' })
export class GoalApiService {
    private readonly base = environment.apiBaseUrl;

    constructor(private http: HttpClient) { }

    // Goals
    getAll(params: PaginationParams = {}): Observable<PaginatedResponse<Goal>> {
        const p = new HttpParams()
            .set('pageNumber', params.pageNumber ?? 1)
            .set('pageSize', params.pageSize ?? 20)
            .set('orderBy', params.orderBy ?? 'createdAt');
        return this.http.get<PaginatedResponse<Goal>>(`${this.base}/goal`, { params: p });
    }

    getById(id: string): Observable<Goal> {
        return this.http.get<Goal>(`${this.base}/goal/${id}`);
    }

    getDetails(id: string): Observable<GoalDetails> {
        return this.http.get<GoalDetails>(`${this.base}/goal/${id}/details`);
    }

    getByDepartment(departmentId: string): Observable<Goal[]> {
        return this.http.get<Goal[]>(`${this.base}/goal/department/${departmentId}`);
    }

    create(req: GoalRequest): Observable<Goal> {
        return this.http.post<Goal>(`${this.base}/goal`, req);
    }

    update(id: string, req: GoalRequest): Observable<Goal> {
        return this.http.put<Goal>(`${this.base}/goal/${id}`, req);
    }

    delete(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/goal/${id}`);
    }

    // Strategy
    getStrategyByGoal(goalId: string): Observable<Strategy> {
        return this.http.get<Strategy>(`${this.base}/strategy/goal/${goalId}`);
    }

    getStrategyById(id: string): Observable<Strategy> {
        return this.http.get<Strategy>(`${this.base}/strategy/${id}`);
    }

    createStrategy(req: StrategyRequest): Observable<Strategy> {
        return this.http.post<Strategy>(`${this.base}/strategy`, req);
    }

    updateStrategy(id: string, req: StrategyRequest): Observable<Strategy> {
        return this.http.put<Strategy>(`${this.base}/strategy/${id}`, req);
    }

    deleteStrategy(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/strategy/${id}`);
    }

    getStrategiesByDepartment(departmentId: string): Observable<Strategy[]> {
        return this.http.get<Strategy[]>(`${this.base}/strategy/department/${departmentId}`);
    }

    // GoalInfluence
    getInfluencesByGoal(goalId: string): Observable<GoalInfluence[]> {
        return this.http.get<GoalInfluence[]>(`${this.base}/goalinfluence/goal/${goalId}`);
    }

    getInfluencesByStrategy(strategyId: string): Observable<GoalInfluence[]> {
        return this.http.get<GoalInfluence[]>(`${this.base}/goalinfluence/strategy/${strategyId}`);
    }

    createInfluence(req: GoalInfluenceRequest): Observable<GoalInfluence> {
        return this.http.post<GoalInfluence>(`${this.base}/goalinfluence`, req);
    }

    deleteInfluence(goalId: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/goalinfluence/${goalId}`);
    }

    // Analytics
    getRootGoalsByDepartment(departmentId: string): Observable<Goal[]> {
        return this.http.get<Goal[]>(`${this.base}/goal/department/${departmentId}/roots`);
    }

    getGoalTree(goalId: string): Observable<GoalTreeNode> {
        return this.http.get<GoalTreeNode>(`${this.base}/goal/${goalId}/tree`);
    }

    getAnalytics(departmentId?: string, rootGoalId?: string): Observable<GoalAnalytics> {
        let params = new HttpParams();
        if (departmentId) {
            params = params.set('departmentId', departmentId);
        }
        if (rootGoalId) {
            params = params.set('rootGoalId', rootGoalId);
        }
        return this.http.get<GoalAnalytics>(`${this.base}/goal/analytics`, { params });
    }
}
