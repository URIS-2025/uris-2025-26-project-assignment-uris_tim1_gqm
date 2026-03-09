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
    total: number;
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

export type GoalStatus = 'Active' | 'OnHold' | 'Completed' | 'Cancelled' | 'Draft';

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
    managerId?: string;
}

export interface DepartmentRequest {
    name: string;
    description?: string;
    organizationId: string;
    managerId?: string;
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
    | 'None' | 'Percentage' | 'Ratio' | 'Index' | 'Score' | 'Rating' | 'Grade' | 'Multiplier' | 'Points' | 'Count'
    | 'Milliseconds' | 'Seconds' | 'Minutes' | 'Hours' | 'Days' | 'Weeks' | 'Months' | 'Quarters' | 'Years'
    | 'Millimeters' | 'Centimeters' | 'Meters' | 'Kilometers' | 'Inches' | 'Feet' | 'Yards' | 'Miles'
    | 'SquareMeters' | 'SquareKilometers' | 'SquareFeet' | 'Hectares' | 'Acres'
    | 'Milliliters' | 'Liters' | 'CubicMeters' | 'CubicFeet' | 'Gallons'
    | 'Milligrams' | 'Grams' | 'Kilograms' | 'Tons' | 'Pounds' | 'Ounces'
    | 'Celsius' | 'Fahrenheit' | 'Kelvin'
    | 'MetersPerSecond' | 'KilometersPerHour' | 'MilesPerHour'
    | 'Joules' | 'KilowattHours' | 'Watts' | 'Kilowatts'
    | 'Currency' | 'CurrencyPerHour' | 'CurrencyPerDay' | 'CurrencyPerMonth' | 'CurrencyPerYear' | 'CostPerUnit' | 'RevenuePerUnit' | 'BudgetVariance'
    | 'DefectCount' | 'DefectsPerUnit' | 'DefectsPerMillion' | 'ErrorRate' | 'FailureRate' | 'AvailabilityPercentage' | 'DowntimeHours' | 'UptimeHours'
    | 'ResponseTimeMilliseconds' | 'ThroughputPerSecond' | 'ThroughputPerMinute' | 'RequestsPerSecond' | 'TransactionsPerSecond' | 'LatencyMilliseconds'
    | 'TasksCompleted' | 'TasksPerHour' | 'OutputPerEmployee' | 'VelocityPoints' | 'StoryPoints' | 'BurndownRate'
    | 'Employees' | 'EmployeesPerManager' | 'TrainingHours' | 'SatisfactionScore' | 'EngagementScore' | 'AttritionRate'
    | 'Customers' | 'NewCustomers' | 'CustomerRetentionRate' | 'ChurnRate' | 'NetPromoterScore' | 'ConversionRate' | 'MarketSharePercentage'
    | 'LinesOfCode' | 'CodeCoveragePercentage' | 'BuildDurationMinutes' | 'DeploymentFrequency' | 'LeadTimeDays' | 'CycleTimeDays'
    | 'RiskScore' | 'RiskExposureCurrency' | 'CompliancePercentage' | 'AuditFindingsCount'
    | 'CO2EmissionsTons' | 'CH4EmissionsTons' | 'EnergyConsumptionKWh' | 'WaterUsageLiters'
    | 'Custom' | 'Other';

export interface Target {
    id: string;
    name: string;
    description: string;
    unit: MeasurementUnit;
    questionId: string;
}

export interface TargetRequest {
    name: string;
    description: string;
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

// User models
export interface User {
    id: string;
    firstName: string;
    lastName: string;
    email: string;
    isActive: boolean;
    roles: string[];
    createdAt?: string;
    updatedAt?: string;
}

export interface UserRequest {
    firstName: string;
    lastName: string;
    email: string;
    password?: string;
    organizationId?: string | null;
}

export interface UpdateProfileRequest {
    firstName: string;
    lastName: string;
}

// Role models
export interface Role {
    id: string;
    name: string;
    description: string;
}

export interface AssignRoleRequest {
    userId: string;
    roleId: string;
}
