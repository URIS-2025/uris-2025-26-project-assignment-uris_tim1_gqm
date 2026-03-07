import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { FormBuilder, FormGroup, FormArray, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatStepperModule } from '@angular/material/stepper';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSliderModule } from '@angular/material/slider';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { GoalApiService } from '../../../core/api/goal-api.service';
import { PremiseApiService } from '../../../core/api/premise-api.service';
import { AssessmentApiService } from '../../../core/api/assessment-api.service';
import { GqmApiService } from '../../../core/api/gqm-api.service';
import { DepartmentApiService } from '../../../core/api/department-api.service';
import { AuthService } from '../../../core/auth/auth.service';
import { PermissionService } from '../../../core/permissions/permission.service';
import { Department, Goal, MeasurementUnit } from '../../../core/api/api.models';
import { firstValueFrom } from 'rxjs';

@Component({
    selector: 'app-goal-new',
    standalone: true,
    imports: [
        ReactiveFormsModule,
        MatStepperModule, MatFormFieldModule, MatInputModule, MatSelectModule,
        MatButtonModule, MatIconModule, MatSliderModule, MatProgressSpinnerModule,
        MatDatepickerModule, MatNativeDateModule,
        PageHeaderComponent,
    ],
    templateUrl: './goal-new.component.html',
    styleUrl: './goal-new.component.css',
})
export class GoalNewComponent implements OnInit {
    departments: Department[] = [];
    submitting = false;
    submitError = '';

    readonly UNIT_GROUPS = [
        { group: 'Dimensionless', units: ['None', 'Percentage', 'Ratio', 'Index', 'Score', 'Rating', 'Grade', 'Multiplier', 'Points', 'Count'] },
        { group: 'Time', units: ['Milliseconds', 'Seconds', 'Minutes', 'Hours', 'Days', 'Weeks', 'Months', 'Quarters', 'Years'] },
        { group: 'Length', units: ['Millimeters', 'Centimeters', 'Meters', 'Kilometers', 'Inches', 'Feet', 'Yards', 'Miles'] },
        { group: 'Area', units: ['SquareMeters', 'SquareKilometers', 'SquareFeet', 'Hectares', 'Acres'] },
        { group: 'Volume', units: ['Milliliters', 'Liters', 'CubicMeters', 'CubicFeet', 'Gallons'] },
        { group: 'Mass / Weight', units: ['Milligrams', 'Grams', 'Kilograms', 'Tons', 'Pounds', 'Ounces'] },
        { group: 'Temperature', units: ['Celsius', 'Fahrenheit', 'Kelvin'] },
        { group: 'Speed', units: ['MetersPerSecond', 'KilometersPerHour', 'MilesPerHour'] },
        { group: 'Energy / Power', units: ['Joules', 'KilowattHours', 'Watts', 'Kilowatts'] },
        { group: 'Financial', units: ['Currency', 'CurrencyPerHour', 'CurrencyPerDay', 'CurrencyPerMonth', 'CurrencyPerYear', 'CostPerUnit', 'RevenuePerUnit', 'BudgetVariance'] },
        { group: 'Quality & Defects', units: ['DefectCount', 'DefectsPerUnit', 'DefectsPerMillion', 'ErrorRate', 'FailureRate', 'AvailabilityPercentage', 'DowntimeHours', 'UptimeHours'] },
        { group: 'Performance', units: ['ResponseTimeMilliseconds', 'ThroughputPerSecond', 'ThroughputPerMinute', 'RequestsPerSecond', 'TransactionsPerSecond', 'LatencyMilliseconds'] },
        { group: 'Productivity', units: ['TasksCompleted', 'TasksPerHour', 'OutputPerEmployee', 'VelocityPoints', 'StoryPoints', 'BurndownRate'] },
        { group: 'Human / HR', units: ['Employees', 'EmployeesPerManager', 'TrainingHours', 'SatisfactionScore', 'EngagementScore', 'AttritionRate'] },
        { group: 'Customer / Business', units: ['Customers', 'NewCustomers', 'CustomerRetentionRate', 'ChurnRate', 'NetPromoterScore', 'ConversionRate', 'MarketSharePercentage'] },
        { group: 'Technical / Software', units: ['LinesOfCode', 'CodeCoveragePercentage', 'BuildDurationMinutes', 'DeploymentFrequency', 'LeadTimeDays', 'CycleTimeDays'] },
        { group: 'Risk & Compliance', units: ['RiskScore', 'RiskExposureCurrency', 'CompliancePercentage', 'AuditFindingsCount'] },
        { group: 'Environmental / Sustainability', units: ['CO2EmissionsTons', 'CH4EmissionsTons', 'EnergyConsumptionKWh', 'WaterUsageLiters'] },
        { group: 'Custom / Fallback', units: ['Custom', 'Other'] }
    ];
    readonly GOAL_STATUSES = ['Draft', 'Active', 'OnHold', 'Completed', 'Cancelled'];
    readonly REFINEMENT_TYPES = ['AND', 'OR'];
    readonly PREMISE_TYPES = ['Assumption', 'Context'];

    private fb = inject(FormBuilder);

    // Step forms
    step1 = this.fb.group({ departmentId: ['', Validators.required] });
    step2 = this.fb.group({
        focus: ['', Validators.required],
        object: ['', Validators.required],
        magnitude: ['', Validators.required],
        constraints: [''],
        status: ['Draft', Validators.required],
        activeFrom: ['', Validators.required],
        activeTo: ['', Validators.required],
        baselineProbability: [0.5, [Validators.required, Validators.min(0), Validators.max(1)]],
    });
    step3 = this.fb.group({
        premises: this.fb.array([this._newPremise()])
    });
    step4 = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        refinementType: ['AND', Validators.required],
        originStrategyId: [''],
    });
    step5 = this.fb.group({
        probability: [0.5, [Validators.required, Validators.min(0), Validators.max(1)]],
        notes: [''],
    });
    step6 = this.fb.group({
        gqmDescription: ['', Validators.required],
        questions: this.fb.array([this._newQuestion()])
    });

    constructor(
        private router: Router,
        private goalApi: GoalApiService,
        private premiseApi: PremiseApiService,
        private assessmentApi: AssessmentApiService,
        private gqmApi: GqmApiService,
        private deptApi: DepartmentApiService,
        private auth: AuthService,
        private permissions: PermissionService,
    ) { }

    ngOnInit(): void {
        this.deptApi.getDepartments({ page: 1, size: 100 }).subscribe({
            next: res => {
                const all = res.items ?? [];
                // Admin gets all, otherwise managers get a filtered restricted view (simulated via first 2)
                if (this.permissions.has('view_all_departments')) {
                    this.departments = all;
                } else {
                    this.departments = all.slice(0, 2);
                }
            },
            error: () => { }
        });
    }

    get premises(): FormArray { return this.step3.get('premises') as FormArray; }
    get questions(): FormArray { return this.step6.get('questions') as FormArray; }

    private _newPremise(): FormGroup {
        return this.fb.group({ description: ['', Validators.required], type: ['Assumption', Validators.required] });
    }

    private _newQuestion(): FormGroup {
        return this.fb.group({
            text: ['', Validators.required],
            targets: this.fb.array([this._newTarget()])
        });
    }

    private _newTarget(): FormGroup {
        return this.fb.group({ name: ['', Validators.required], description: [''], unit: ['None', Validators.required] });
    }

    addPremise(): void { this.premises.push(this._newPremise()); }
    removePremise(i: number): void { if (this.premises.length > 1) this.premises.removeAt(i); }

    addQuestion(): void { this.questions.push(this._newQuestion()); }
    removeQuestion(i: number): void { if (this.questions.length > 1) this.questions.removeAt(i); }

    getTargets(qi: number): FormArray {
        return (this.questions.at(qi) as FormGroup).get('targets') as FormArray;
    }

    addTarget(qi: number): void { this.getTargets(qi).push(this._newTarget()); }
    removeTarget(qi: number, ti: number): void {
        const t = this.getTargets(qi);
        if (t.length > 1) t.removeAt(ti);
    }

    getBaseline(): number { return Math.round((this.step2.get('baselineProbability')?.value ?? 0.5) * 100); }
    getProbability(): number { return Math.round((this.step5.get('probability')?.value ?? 0.5) * 100); }
    getProbabilityLevel(): string {
        const p = this.step5.get('probability')?.value ?? 0.5;
        if (p < 0.33) return 'Low';
        if (p < 0.66) return 'Medium';
        return 'High';
    }

    saveDraft(): void {
        this.step2.patchValue({ status: 'Draft' });
        this.submit();
    }

    async submit(): Promise<void> {
        this.submitting = true;
        this.submitError = '';

        try {
            // 1. Create Goal
            const goal: Goal = await firstValueFrom(this.goalApi.create({
                ...this.step2.value,
                departmentId: this.step1.value.departmentId!,
                activeFrom: new Date(this.step2.value.activeFrom!).toISOString(),
                activeTo: new Date(this.step2.value.activeTo!).toISOString(),
                baselineProbability: this.step2.value.baselineProbability!,
                status: this.step2.value.status as any,
                focus: this.step2.value.focus!,
                object: this.step2.value.object!,
                magnitude: this.step2.value.magnitude!,
                constraints: this.step2.value.constraints ?? '',
            }));

            // 2. Create Premises
            for (const p of this.premises.value) {
                await firstValueFrom(this.premiseApi.create({ ...p, goalId: goal.id }));
            }

            // 3. Create Strategy
            await firstValueFrom(this.goalApi.createStrategy({
                ...this.step4.value,
                goalId: goal.id,
                name: this.step4.value.name!,
                description: this.step4.value.description ?? '',
                refinementType: this.step4.value.refinementType as any,
                originStrategyId: this.step4.value.originStrategyId || undefined,
            }));

            // 4. Create Assessment
            await firstValueFrom(this.assessmentApi.create({
                probability: this.step5.value.probability!,
                notes: this.step5.value.notes ?? '',
                goalId: goal.id,
            }));

            // 5. Create GQM Goal + Questions + Targets
            const gqmGoal = await firstValueFrom(this.gqmApi.createGqmGoal({
                description: this.step6.value.gqmDescription!,
                goalId: goal.id,
            }));

            for (const q of this.questions.value) {
                const question = await firstValueFrom(this.gqmApi.createQuestion({
                    text: q.text,
                    gqmGoalId: gqmGoal.id,
                }));
                for (const t of q.targets) {
                    await firstValueFrom(this.gqmApi.createTarget({
                        name: t.name,
                        description: t.description || '',
                        unit: t.unit,
                        questionId: question.id
                    }));
                }
            }

            this.router.navigate(['/goals', goal.id]);
        } catch (err) {
            this.submitError = 'Failed to create goal. Please check your inputs and try again.';
            console.error(err);
        } finally {
            this.submitting = false;
        }
    }
}
