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
    private readonly base = `${environment.apiBaseUrl}/GQM-goal`;

    constructor(private http: HttpClient) { }

    // GQM Goals
    getGqmGoals(params: PagedParams = {}): Observable<PaginatedResponse<GqmGoal>> {
        const p = new HttpParams().set('page', params.page ?? 1).set('size', params.size ?? 20);
        return this.http.get<PaginatedResponse<GqmGoal>>(`${this.base}/GqmGoal`, { params: p });
    }

    getGqmGoalById(id: string): Observable<GqmGoal> {
        return this.http.get<GqmGoal>(`${this.base}/GqmGoal/${id}`);
    }

    getGqmGoalsByGoal(goalId: string): Observable<GqmGoal[]> {
        return this.http.get<GqmGoal[]>(`${this.base}/GqmGoal/by-goal/${goalId}`);
    }

    createGqmGoal(req: GqmGoalRequest): Observable<GqmGoal> {
        return this.http.post<GqmGoal>(`${this.base}/GqmGoal`, req);
    }

    updateGqmGoal(id: string, req: GqmGoalRequest): Observable<GqmGoal> {
        return this.http.put<GqmGoal>(`${this.base}/GqmGoal/${id}`, req);
    }

    deleteGqmGoal(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/GqmGoal/${id}`);
    }

    // Questions
    getQuestions(): Observable<Question[]> {
        return this.http.get<Question[]>(`${this.base}/Question`);
    }

    getQuestionById(id: string): Observable<Question> {
        return this.http.get<Question>(`${this.base}/Question/${id}`);
    }

    getQuestionsByGqmGoal(gqmGoalId: string): Observable<Question[]> {
        return this.http.get<Question[]>(`${this.base}/Question/by-goal/${gqmGoalId}`);
    }

    createQuestion(req: QuestionRequest): Observable<Question> {
        return this.http.post<Question>(`${this.base}/Question`, req);
    }

    updateQuestion(id: string, req: QuestionRequest): Observable<Question> {
        return this.http.put<Question>(`${this.base}/Question/${id}`, req);
    }

    deleteQuestion(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/Question/${id}`);
    }

    // Targets
    getTargets(): Observable<Target[]> {
        return this.http.get<Target[]>(`${this.base}/Target`);
    }

    getTargetById(id: string): Observable<Target> {
        return this.http.get<Target>(`${this.base}/Target/${id}`);
    }

    getTargetsByQuestion(questionId: string): Observable<Target[]> {
        return this.http.get<Target[]>(`${this.base}/Target/by-question/${questionId}`);
    }

    createTarget(req: TargetRequest): Observable<Target> {
        return this.http.post<Target>(`${this.base}/Target`, req);
    }

    updateTarget(id: string, req: TargetRequest): Observable<Target> {
        return this.http.put<Target>(`${this.base}/Target/${id}`, req);
    }

    deleteTarget(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/Target/${id}`);
    }

    // Measurements
    getMeasurements(): Observable<Measurement[]> {
        return this.http.get<Measurement[]>(`${this.base}/Measurement`);
    }

    getMeasurementById(id: string): Observable<Measurement> {
        return this.http.get<Measurement>(`${this.base}/Measurement/${id}`);
    }

    getMeasurementsByTarget(targetId: string): Observable<Measurement[]> {
        return this.http.get<Measurement[]>(`${this.base}/Measurement/by-target/${targetId}`);
    }

    createMeasurement(req: MeasurementRequest): Observable<Measurement> {
        return this.http.post<Measurement>(`${this.base}/Measurement`, req);
    }

    updateMeasurement(id: string, req: MeasurementRequest): Observable<Measurement> {
        return this.http.put<Measurement>(`${this.base}/Measurement/${id}`, req);
    }

    deleteMeasurement(id: string): Observable<void> {
        return this.http.delete<void>(`${this.base}/Measurement/${id}`);
    }
}
