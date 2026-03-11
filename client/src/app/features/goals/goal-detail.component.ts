import { Component, OnInit, inject, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatTabsModule } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatTooltipModule } from '@angular/material/tooltip';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { GoalApiService } from '../../core/api/goal-api.service';
import { DepartmentApiService } from '../../core/api/department-api.service';
import { GqmApiService } from '../../core/api/gqm-api.service';
import { PremiseApiService } from '../../core/api/premise-api.service';
import { ToastService } from '../../core/services/toast.service';
import {
    GoalDetails, GqmGoal, Question, Target, Measurement,
    MeasurementRequest, StrategyRequest, PremiseRequest,
    GqmGoalRequest, QuestionRequest, TargetRequest,
    GoalTreeNode
} from '../../core/api/api.models';

export interface TargetWithMeasurements extends Target {
    measurements: Measurement[];
    measurementsLoaded: boolean;
}

export interface QuestionWithTargets extends Question {
    targets: TargetWithMeasurements[];
    targetsLoaded: boolean;
}

export interface GqmGoalWithChildren extends GqmGoal {
    questions: QuestionWithTargets[];
    questionsLoaded: boolean;
}

export interface FlatTarget {
    target: TargetWithMeasurements;
    question: QuestionWithTargets;
    gqmGoal: GqmGoalWithChildren;
}

@Component({
    selector: 'app-goal-detail',
    standalone: true,
    imports: [
        CommonModule, RouterLink, ReactiveFormsModule,
        MatCardModule, MatButtonModule, MatIconModule,
        MatProgressSpinnerModule, MatDividerModule, MatTabsModule,
        MatFormFieldModule, MatInputModule, MatSelectModule,
        MatDatepickerModule, MatNativeDateModule, MatExpansionModule,
        MatTooltipModule, HasPermissionDirective
    ],
    templateUrl: './goal-detail.component.html',
    styleUrl: './goal-detail.component.css',
})
export class GoalDetailComponent implements OnInit {
    goal: GoalDetails | null = null;
    loading = true;
    error = '';

    departmentName = '';

    // GQM Structure
    gqmStructure: GqmGoalWithChildren[] = [];
    gqmLoading = false;
    gqmLoaded = false;
    gqmError = '';

    // Flat targets list (populated after GQM loaded)
    allTargets: FlatTarget[] = [];

    // Measurement form state
    showMeasurementForm = false;
    measurementSubmitting = false;
    filteredQuestions: QuestionWithTargets[] = [];
    filteredTargets: TargetWithMeasurements[] = [];
    selectedGqmGoalId = '';
    selectedQuestionId = '';
    selectedTargetId = '';

    // Strategy form state
    showStrategyForm = false;
    strategySubmitting = false;

    // Premise form state
    showPremiseForm = false;
    premiseSubmitting = false;

    // GQM creation form state
    showGqmGoalForm = false;
    gqmGoalSubmitting = false;
    showQuestionFormFor: string | null = null;  // gqmGoal.id
    questionSubmitting = false;
    showTargetFormFor: string | null = null;    // question.id
    targetSubmitting = false;

    // Influence tree state
    goalTree: GoalTreeNode | null = null;
    influenceLoading = false;
    influenceLoaded = false;

    private route = inject(ActivatedRoute);
    private goalApi = inject(GoalApiService);
    private deptApi = inject(DepartmentApiService);
    private gqmApi = inject(GqmApiService);
    private premiseApi = inject(PremiseApiService);
    private toast = inject(ToastService);
    private fb = inject(FormBuilder);
    private destroyRef = inject(DestroyRef);

    measurementForm = this.fb.group({
        value: ['', Validators.required],
        measuredAt: [new Date(), Validators.required],
    });

    strategyForm = this.fb.group({
        name: ['', Validators.required],
        description: ['', Validators.required],
        refinementType: ['AND' as 'AND' | 'OR', Validators.required],
    });

    premiseForm = this.fb.group({
        description: ['', Validators.required],
        type: ['Assumption' as 'Assumption' | 'Context', Validators.required],
    });

    gqmGoalForm = this.fb.group({
        description: ['', Validators.required],
    });

    questionForm = this.fb.group({
        text: ['', Validators.required],
    });

    targetForm = this.fb.group({
        name: ['', Validators.required],
        description: [''],
        unit: ['Count' as string, Validators.required],
    });

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id')!;
        this.goalApi.getDetails(id).pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
            next: goal => {
                this.goal = goal;
                this.loading = false;
                this.loadDepartmentName(goal.departmentId);
                // Pre-load GQM structure so Measurements tab is ready immediately
                this.loadGqmStructure();
            },
            error: () => { this.error = 'Failed to load goal details.'; this.loading = false; }
        });
    }

    private loadDepartmentName(departmentId: string): void {
        if (!departmentId) return;
        this.deptApi.getDepartmentById(departmentId).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(() => of(null))
        ).subscribe(dept => {
            this.departmentName = dept?.name ?? departmentId;
        });
    }

    onTabChange(index: number): void {
        // Tab index 3 = GQM Structure, Tab index 4 = Measurements
        if ((index === 3 || index === 4) && !this.gqmLoaded) {
            this.loadGqmStructure();
        }
        // Tab index 5 = Influence
        if (index === 5 && !this.influenceLoaded) {
            this.loadInfluenceTree();
        }
    }

    loadInfluenceTree(): void {
        if (!this.goal || this.influenceLoading) return;
        this.influenceLoading = true;
        this.goalApi.getGoalTree(this.goal.id).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(() => of(null))
        ).subscribe(tree => {
            this.goalTree = tree;
            this.influenceLoading = false;
            this.influenceLoaded = true;
        });
    }

    getTotalChildGoals(): number {
        if (!this.goalTree) return 0;
        return this.goalTree.strategies.reduce(
            (sum, s) => sum + s.childGoals.length, 0
        );
    }

    getInfluenceClass(type: string): string {
        switch (type) {
            case 'Positive': return 'influence-positive';
            case 'Negative': return 'influence-negative';
            default: return 'influence-neutral';
        }
    }

    loadGqmStructure(): void {
        if (!this.goal || this.gqmLoading) return;
        this.gqmLoading = true;
        this.gqmError = '';

        this.gqmApi.getGqmGoalsByGoal(this.goal.id).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(err => {
                const msg = err?.error?.message ?? err?.message ?? 'Unknown error';
                this.gqmError = `Failed to load GQM structure: ${msg}`;
                this.gqmLoading = false;
                this.gqmLoaded = true;
                this.toast.showError(this.gqmError);
                return of([]);
            })
        ).subscribe(gqmGoals => {
            this.gqmStructure = gqmGoals.map(g => ({
                ...g,
                questions: [],
                questionsLoaded: false,
            }));

            if (gqmGoals.length === 0) {
                this.gqmLoading = false;
                this.gqmLoaded = true;
                return;
            }

            const questionRequests = gqmGoals.map(g =>
                this.gqmApi.getQuestionsByGqmGoal(g.id).pipe(catchError(() => of([])))
            );

            forkJoin(questionRequests).pipe(
                takeUntilDestroyed(this.destroyRef),
                catchError(() => of([]))
            ).subscribe((questionSets: Question[][]) => {
                questionSets.forEach((questions, i) => {
                    this.gqmStructure[i].questions = questions.map(q => ({
                        ...q,
                        targets: [],
                        targetsLoaded: false,
                    }));
                    this.gqmStructure[i].questionsLoaded = true;
                });

                const allQuestions = this.gqmStructure.flatMap(g => g.questions);
                if (allQuestions.length === 0) {
                    this.gqmLoading = false;
                    this.gqmLoaded = true;
                    return;
                }

                const targetRequests = allQuestions.map(q =>
                    this.gqmApi.getTargetsByQuestion(q.id).pipe(catchError(() => of([])))
                );

                forkJoin(targetRequests).pipe(
                    takeUntilDestroyed(this.destroyRef),
                    catchError(() => of([]))
                ).subscribe((targetSets: Target[][]) => {
                    let targetIdx = 0;
                    this.gqmStructure.forEach(g => {
                        g.questions.forEach(q => {
                            q.targets = (targetSets[targetIdx++] || []).map(t => ({
                                ...t,
                                measurements: [],
                                measurementsLoaded: false,
                            }));
                            q.targetsLoaded = true;
                        });
                    });

                    const flat: FlatTarget[] = this.gqmStructure.flatMap(g =>
                        g.questions.flatMap(q => q.targets.map(t => ({ target: t, question: q, gqmGoal: g })))
                    );

                    if (flat.length === 0) {
                        this.allTargets = [];
                        this.gqmLoading = false;
                        this.gqmLoaded = true;
                        return;
                    }

                    const measurementRequests = flat.map(({ target }) =>
                        this.gqmApi.getMeasurementsByTarget(target.id).pipe(catchError(() => of([])))
                    );

                    forkJoin(measurementRequests).pipe(
                        takeUntilDestroyed(this.destroyRef),
                        catchError(() => of([]))
                    ).subscribe((measurementSets: Measurement[][]) => {
                        flat.forEach(({ target }, i) => {
                            target.measurements = measurementSets[i] || [];
                            target.measurementsLoaded = true;
                        });
                        this.allTargets = flat;
                        this.gqmLoading = false;
                        this.gqmLoaded = true;
                    });
                });
            });
        });
    }

    // Cascading selects for measurement form
    onGqmGoalChange(gqmGoalId: string): void {
        this.selectedGqmGoalId = gqmGoalId;
        const gqmGoal = this.gqmStructure.find(g => g.id === gqmGoalId);
        this.filteredQuestions = gqmGoal?.questions ?? [];
        this.filteredTargets = [];
        this.selectedQuestionId = '';
        this.selectedTargetId = '';
    }

    onQuestionChange(questionId: string): void {
        this.selectedQuestionId = questionId;
        const question = this.filteredQuestions.find(q => q.id === questionId);
        this.filteredTargets = question?.targets ?? [];
        this.selectedTargetId = '';
    }

    onTargetChange(targetId: string): void {
        this.selectedTargetId = targetId;
    }

    get measurementFormValid(): boolean {
        return !!this.selectedGqmGoalId && !!this.selectedQuestionId &&
               !!this.selectedTargetId && this.measurementForm.valid;
    }

    submitMeasurement(): void {
        if (!this.measurementFormValid) return;
        const v = this.measurementForm.value;
        const req: MeasurementRequest = {
            value: v.value!,
            measuredAt: new Date(v.measuredAt!).toISOString(),
            targetId: this.selectedTargetId,
        };
        this.measurementSubmitting = true;
        this.gqmApi.createMeasurement(req).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(err => { this.toast.showError('Failed to save measurement.'); this.measurementSubmitting = false; throw err; })
        ).subscribe(result => {
            const flat = this.allTargets.find(t => t.target.id === req.targetId);
            if (flat) flat.target.measurements.unshift(result);
            this.resetMeasurementForm();
            this.showMeasurementForm = false;
            this.measurementSubmitting = false;
            this.toast.showSuccess('Measurement saved successfully.');
        });
    }

    cancelMeasurement(): void {
        this.showMeasurementForm = false;
        this.resetMeasurementForm();
    }

    private resetMeasurementForm(): void {
        this.measurementForm.reset({ value: '', measuredAt: new Date() });
        this.selectedGqmGoalId = '';
        this.selectedQuestionId = '';
        this.selectedTargetId = '';
        this.filteredQuestions = [];
        this.filteredTargets = [];
    }

    submitStrategy(): void {
        if (this.strategyForm.invalid) return;
        const v = this.strategyForm.value;
        const req: StrategyRequest = {
            name: v.name!,
            description: v.description!,
            refinementType: v.refinementType as 'AND' | 'OR',
            goalId: this.goal!.id,
        };
        this.strategySubmitting = true;
        this.goalApi.createStrategy(req).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(err => { this.toast.showError('Failed to create strategy.'); this.strategySubmitting = false; throw err; })
        ).subscribe(result => {
            if (!this.goal!.strategies) this.goal!.strategies = [];
            this.goal!.strategies.push(result);
            this.strategyForm.reset({ name: '', description: '', refinementType: 'AND' });
            this.showStrategyForm = false;
            this.strategySubmitting = false;
            this.toast.showSuccess('Strategy created successfully.');
        });
    }

    cancelStrategy(): void {
        this.showStrategyForm = false;
        this.strategyForm.reset({ name: '', description: '', refinementType: 'AND' });
    }

    submitPremise(): void {
        if (this.premiseForm.invalid) return;
        const v = this.premiseForm.value;
        const req: PremiseRequest = {
            description: v.description!,
            type: v.type as 'Assumption' | 'Context',
            goalId: this.goal!.id,
        };
        this.premiseSubmitting = true;
        this.premiseApi.create(req).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(err => { this.toast.showError('Failed to add premise.'); this.premiseSubmitting = false; throw err; })
        ).subscribe(result => {
            if (!this.goal!.premises) this.goal!.premises = [];
            this.goal!.premises.push(result);
            this.premiseForm.reset({ description: '', type: 'Assumption' });
            this.showPremiseForm = false;
            this.premiseSubmitting = false;
            this.toast.showSuccess('Premise added successfully.');
        });
    }

    cancelPremise(): void {
        this.showPremiseForm = false;
        this.premiseForm.reset({ description: '', type: 'Assumption' });
    }

    submitGqmGoal(): void {
        if (this.gqmGoalForm.invalid) return;
        const req: GqmGoalRequest = {
            description: this.gqmGoalForm.value.description!,
            goalId: this.goal!.id,
        };
        this.gqmGoalSubmitting = true;
        this.gqmApi.createGqmGoal(req).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(err => { this.toast.showError('Failed to create GQM Goal.'); this.gqmGoalSubmitting = false; throw err; })
        ).subscribe(result => {
            this.gqmStructure.push({ ...result, questions: [], questionsLoaded: true });
            this.gqmGoalForm.reset({ description: '' });
            this.showGqmGoalForm = false;
            this.gqmGoalSubmitting = false;
            this.gqmLoaded = true;
            this.toast.showSuccess('GQM Goal created.');
        });
    }

    cancelGqmGoal(): void {
        this.showGqmGoalForm = false;
        this.gqmGoalForm.reset({ description: '' });
    }

    submitQuestion(gqmGoalId: string): void {
        if (this.questionForm.invalid) return;
        const req: QuestionRequest = {
            text: this.questionForm.value.text!,
            gqmGoalId,
        };
        this.questionSubmitting = true;
        this.gqmApi.createQuestion(req).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(err => { this.toast.showError('Failed to create Question.'); this.questionSubmitting = false; throw err; })
        ).subscribe(result => {
            const gqmGoal = this.gqmStructure.find(g => g.id === gqmGoalId);
            if (gqmGoal) gqmGoal.questions.push({ ...result, targets: [], targetsLoaded: true });
            this.questionForm.reset({ text: '' });
            this.showQuestionFormFor = null;
            this.questionSubmitting = false;
            this.toast.showSuccess('Question added.');
        });
    }

    cancelQuestion(): void {
        this.showQuestionFormFor = null;
        this.questionForm.reset({ text: '' });
    }

    submitTarget(questionId: string): void {
        if (this.targetForm.invalid) return;
        const v = this.targetForm.value;
        const req: TargetRequest = {
            name: v.name!,
            description: v.description ?? '',
            unit: v.unit! as any,
            questionId,
        };
        this.targetSubmitting = true;
        this.gqmApi.createTarget(req).pipe(
            takeUntilDestroyed(this.destroyRef),
            catchError(err => { this.toast.showError('Failed to create Target.'); this.targetSubmitting = false; throw err; })
        ).subscribe(result => {
            for (const g of this.gqmStructure) {
                const q = g.questions.find(q => q.id === questionId);
                if (q) { q.targets.push({ ...result, measurements: [], measurementsLoaded: true }); break; }
            }
            // Rebuild flat targets list
            this.allTargets = this.gqmStructure.flatMap(g =>
                g.questions.flatMap(q => q.targets.map(t => ({ target: t, question: q, gqmGoal: g })))
            );
            this.targetForm.reset({ name: '', description: '', unit: 'Count' });
            this.showTargetFormFor = null;
            this.targetSubmitting = false;
            this.toast.showSuccess('Target added.');
        });
    }

    cancelTarget(): void {
        this.showTargetFormFor = null;
        this.targetForm.reset({ name: '', description: '', unit: 'Count' });
    }

    readonly commonUnits = [
        'Percentage', 'Count', 'Score', 'Rating', 'Index', 'Ratio',
        'Days', 'Hours', 'Minutes', 'Seconds', 'Milliseconds',
        'Currency', 'Customers', 'Employees', 'Points', 'StoryPoints',
        'ResponseTimeMilliseconds', 'ThroughputPerSecond', 'ErrorRate',
    ];

    getSelectedTargetUnit(): string {
        if (!this.selectedTargetId) return '';
        return this.filteredTargets.find(t => t.id === this.selectedTargetId)?.unit ?? '';
    }

    getTotalMeasurements(): number {
        return this.allTargets.reduce((sum, t) => sum + t.target.measurements.length, 0);
    }

    formatDate(d: string | Date): string {
        if (!d) return '—';
        return new Date(d).toLocaleDateString('en-US', {
            year: 'numeric', month: 'numeric', day: 'numeric',
            hour: 'numeric', minute: '2-digit'
        });
    }

    getDepartmentName(): string {
        return this.departmentName || (this.goal?.departmentId ? '…' : '—');
    }

    getTimeProgress(): number {
        if (!this.goal) return 0;
        const start = new Date(this.goal.activeFrom).getTime();
        const end = new Date(this.goal.activeTo).getTime();
        const now = Date.now();
        if (now <= start) return 0;
        if (now >= end) return 100;
        return Math.round(((now - start) / (end - start)) * 100);
    }

    getDaysRemaining(): string {
        if (!this.goal) return '';
        const end = new Date(this.goal.activeTo).getTime();
        const diff = Math.ceil((end - Date.now()) / (1000 * 60 * 60 * 24));
        if (diff < 0) return 'Expired';
        return `${diff} days remaining`;
    }
}
