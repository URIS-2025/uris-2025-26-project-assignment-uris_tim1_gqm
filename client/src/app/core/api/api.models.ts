// Shared pagination interfaces
export interface PaginationParams {
    pageNumber?: number;
    pageSize?: number;
    orderBy?: string;
}

export interface PagedParams {
    page?: number;
    size?: number;
}

export interface PaginatedResponse<T> {
    items: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
    totalPages: number;
}

// Goal models
export interface Goal {
    id: string;
    focus: string;
    object: string;
    magnitude: string;
    constraints: string;
    status: GoalStatus;
    activeFrom: string;
    activeTo: string;
    baselineProbability: number;
    departmentId: string;
    createdAt?: string;
    updatedAt?: string;
}

export type GoalStatus = 'Active' | 'OnHold' | 'Completed' | 'Cancelled';

export interface GoalDetails extends Goal {
    strategy?: Strategy;
    premises?: Premise[];
    assessments?: Assessment[];
    gqmGoals?: GqmGoal[];
}

export interface GoalRequest {
    focus: string;
    object: string;
    magnitude: string;
    constraints: string;
    status: GoalStatus;
    activeFrom: string;
    activeTo: string;
    baselineProbability: number;
    departmentId: string;
}

// Strategy models
export interface Strategy {
    id: string;
    name: string;
    description: string;
    refinementType: 'AND' | 'OR';
    goalId: string;
    originStrategyId?: string;
}

export interface StrategyRequest {
    name: string;
    description: string;
    refinementType: 'AND' | 'OR';
    goalId: string;
    originStrategyId?: string;
}

// GoalInfluence models
export interface GoalInfluence {
    id: string;
    goalId: string;
    strategyId: string;
    description: string;
}

export interface GoalInfluenceRequest {
    goalId: string;
    strategyId: string;
    description: string;
}

// Department models
export interface Department {
    id: string;
    name: string;
    description?: string;
    organizationId: string;
}

export interface DepartmentRequest {
    name: string;
    description?: string;
    organizationId: string;
}

// Organization models
export interface Organization {
    id: string;
    name: string;
    description?: string;
}

export interface OrganizationRequest {
    name: string;
    description?: string;
}

// Premise models
export type PremiseType = 'Assumption' | 'Context';

export interface Premise {
    id: string;
    description: string;
    type: PremiseType;
    goalId: string;
    strategyId?: string;
    isActive: boolean;
    version: number;
    createdAt?: string;
}

export interface PremiseRequest {
    description: string;
    type: PremiseType;
    goalId: string;
    strategyId?: string;
}

// Assessment models
export interface Assessment {
    id: string;
    probability: number;
    notes: string;
    goalId: string;
    assessedAt?: string;
}

export interface AssessmentRequest {
    probability: number;
    notes: string;
    goalId: string;
}

// GQM models
export interface GqmGoal {
    id: string;
    description: string;
    goalId: string;
}

export interface GqmGoalRequest {
    description: string;
    goalId: string;
}

export interface Question {
    id: string;
    text: string;
    gqmGoalId: string;
}

export interface QuestionRequest {
    text: string;
    gqmGoalId: string;
}

export type MeasurementUnit =
    | 'dimensionless' | 'time' | 'length' | 'area' | 'volume' | 'mass' | 'temperature'
    | 'speed' | 'energy' | 'financial' | 'quality' | 'performance' | 'productivity'
    | 'HR' | 'business' | 'technical' | 'risk' | 'environmental' | 'custom';

export interface Target {
    id: string;
    value: string;
    unit: MeasurementUnit;
    questionId: string;
}

export interface TargetRequest {
    value: string;
    unit: MeasurementUnit;
    questionId: string;
}

export interface Measurement {
    id: string;
    value: string;
    measuredAt: string;
    targetId: string;
}

export interface MeasurementRequest {
    value: string;
    measuredAt: string;
    targetId: string;
}
