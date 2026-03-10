import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface SagaWorkflowResponse {
    id: string;
    goalId: string;
    status: string;
    currentStep: string;
    createdAt: string;
    updatedAt: string;
}

@Injectable({ providedIn: 'root' })
export class OrchestrationApiService {
    private readonly base = environment.apiBaseUrl;

    constructor(private http: HttpClient) { }

    /**
     * Triggers the compensation sequence for a specific workflow
     * @param goalId The unique identifier of the goal tracking the workflow
     */
    cancelWorkflow(goalId: string): Observable<SagaWorkflowResponse> {
        return this.http.post<SagaWorkflowResponse>(`${this.base}/workflow/${goalId}/cancel`, {});
    }
}
