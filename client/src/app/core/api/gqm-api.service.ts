import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
    GqmGoal, GqmGoalRequest,
    Question, QuestionRequest,
    Target, TargetRequest,
    Measurement, MeasurementRequest,
    PaginatedResponse, PagedParams
} from './api.models';

@Injectable({ providedIn: 'root' })
export class GqmApiService {
    private readonly base = environment.apiBaseUrl;

    constructor(private http: HttpClient) { }

    // GQM Goals
    getGqmGoals(params: PagedParams = {}): Observable<PaginatedResponse<GqmGoal>> {
        const p = new HttpParams().set('page', params.page ?? 1).set('size', params.size ?? 20);
        return this.http.get<PaginatedResponse<GqmGoal>>(`${this.base}/gqmgoal`, { params: p });
    }

    getGqmGoalById(id: string): Observable<GqmGoal> {
        return this.http.get<GqmGoal>(`${this.base}/gqmgoal/${id}`);
    }

    getGqmGoalsByGoal(goalId: string): Observable<GqmGoal[]> {
        return this.http.get<GqmGoal[]>(`${this.base}/gqmgoal/by-goal/${goalId}`);
    }

    createGqmGoal(req: GqmGoalRequest): Observable<GqmGoal> {
        return this.http.post<GqmGoal>(`${this.base}/gqmgoal`, req);
    }

    updateGqmGoal(id: string, req: GqmGoalRequest): Observable<GqmGoal> {
        return this.http.put<GqmGoal>(`${this.base}/gqmgoal/${id}`, req);
    }

    deleteGqmGoal(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/gqmgoal/${id}`);
    }

    // Questions
    getQuestions(): Observable<Question[]> {
        return this.http.get<Question[]>(`${this.base}/question`);
    }

    getQuestionById(id: string): Observable<Question> {
        return this.http.get<Question>(`${this.base}/question/${id}`);
    }

    getQuestionsByGqmGoal(gqmGoalId: string): Observable<Question[]> {
        return this.http.get<Question[]>(`${this.base}/question/by-goal/${gqmGoalId}`);
    }

    createQuestion(req: QuestionRequest): Observable<Question> {
        return this.http.post<Question>(`${this.base}/question`, req);
    }

    updateQuestion(id: string, req: QuestionRequest): Observable<Question> {
        return this.http.put<Question>(`${this.base}/question/${id}`, req);
    }

    deleteQuestion(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/question/${id}`);
    }

    // Targets
    getTargets(): Observable<Target[]> {
        return this.http.get<Target[]>(`${this.base}/target`);
    }

    getTargetById(id: string): Observable<Target> {
        return this.http.get<Target>(`${this.base}/target/${id}`);
    }

    getTargetsByQuestion(questionId: string): Observable<Target[]> {
        return this.http.get<Target[]>(`${this.base}/target/by-question/${questionId}`);
    }

    createTarget(req: TargetRequest): Observable<Target> {
        return this.http.post<Target>(`${this.base}/target`, req);
    }

    updateTarget(id: string, req: TargetRequest): Observable<Target> {
        return this.http.put<Target>(`${this.base}/target/${id}`, req);
    }

    deleteTarget(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/target/${id}`);
    }

    // Measurements
    getMeasurements(): Observable<Measurement[]> {
        return this.http.get<Measurement[]>(`${this.base}/measurement`);
    }

    getMeasurementById(id: string): Observable<Measurement> {
        return this.http.get<Measurement>(`${this.base}/measurement/${id}`);
    }

    getMeasurementsByTarget(targetId: string): Observable<Measurement[]> {
        return this.http.get<Measurement[]>(`${this.base}/measurement/by-target/${targetId}`);
    }

    createMeasurement(req: MeasurementRequest): Observable<Measurement> {
        return this.http.post<Measurement>(`${this.base}/measurement`, req);
    }

    updateMeasurement(id: string, req: MeasurementRequest): Observable<Measurement> {
        return this.http.put<Measurement>(`${this.base}/measurement/${id}`, req);
    }

    deleteMeasurement(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/measurement/${id}`);
    }
}
