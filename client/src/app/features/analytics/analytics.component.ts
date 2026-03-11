import { Component, OnInit, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GoalHierarchyGraphComponent } from './components/goal-hierarchy-graph.component';
import { AnalyticsDashboardComponent } from './components/analytics-dashboard.component';
import { DetailDrawerComponent } from './components/detail-drawer.component';
import { GoalApiService } from '../../core/api/goal-api.service';
import { DepartmentApiService } from '../../core/api/department-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { Department, GoalTreeNode, StrategyTreeNode, GoalAnalytics, Goal } from '../../core/api/api.models';
import { finalize } from 'rxjs/operators';

@Component({
    selector: 'app-analytics',
    standalone: true,
    imports: [
        CommonModule,
        FormsModule,
        PageHeaderComponent,
        MatCardModule,
        MatIconModule,
        MatSelectModule,
        MatFormFieldModule,
        MatButtonModule,
        MatProgressSpinnerModule,
        GoalHierarchyGraphComponent,
        AnalyticsDashboardComponent,
        DetailDrawerComponent
    ],
    templateUrl: './analytics.component.html',
    styleUrl: './analytics.component.scss'
})
export class AnalyticsComponent implements OnInit {
    private goalApi = inject(GoalApiService);
    private deptApi = inject(DepartmentApiService);
    private auth = inject(AuthService);
    private destroyRef = inject(DestroyRef);

    departments: Department[] = [];
    rootGoals: Goal[] = [];
    goalTree: GoalTreeNode | null = null;
    analytics: GoalAnalytics | null = null;

    selectedDepartmentId: string | null = null;
    selectedRootGoalId: string | null = null;

    isLoading = false;

    // Drawer state
    drawerOpen = false;
    selectedNodeType: 'goal' | 'strategy' | null = null;
    selectedGoal: GoalTreeNode | null = null;
    selectedStrategy: StrategyTreeNode | null = null;

    ngOnInit(): void {
        // Reload departments whenever organization changes
        this.auth.organizationId$.pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe(() => {
            this.resetFilters();
            this.loadDepartments();
            this.loadAnalytics();
        });
    }

    private loadDepartments(): void {
        const orgId = this.auth.organizationId;
        const request$ = orgId
            ? this.deptApi.getDepartmentsByOrg(orgId, { page: 1, size: 100 })
            : this.deptApi.getDepartments({ page: 1, size: 100 });

        request$.subscribe({
            next: (res) => {
                this.departments = res.items;
            },
            error: (err) => console.error('Failed to load departments', err)
        });
    }

    onDepartmentChange(): void {
        this.rootGoals = [];
        this.selectedRootGoalId = null;
        this.goalTree = null;

        if (this.selectedDepartmentId) {
            this.loadRootGoals();
        }
        this.loadAnalytics();
    }

    private loadRootGoals(): void {
        if (!this.selectedDepartmentId) return;

        this.goalApi.getRootGoalsByDepartment(this.selectedDepartmentId).subscribe({
            next: (goals) => {
                this.rootGoals = goals;
            },
            error: (err) => console.error('Failed to load root goals', err)
        });
    }

    onRootGoalChange(): void {
        this.goalTree = null;

        if (this.selectedRootGoalId) {
            this.loadGoalTree();
        }
        this.loadAnalytics();
    }

    private loadGoalTree(): void {
        if (!this.selectedRootGoalId) return;

        this.isLoading = true;
        this.goalApi.getGoalTree(this.selectedRootGoalId)
            .pipe(finalize(() => this.isLoading = false))
            .subscribe({
                next: (tree) => {
                    this.goalTree = tree;
                },
                error: (err) => console.error('Failed to load goal tree', err)
            });
    }

    private loadAnalytics(): void {
        this.isLoading = true;
        this.goalApi.getAnalytics(this.selectedDepartmentId || undefined, this.selectedRootGoalId || undefined)
            .pipe(finalize(() => this.isLoading = false))
            .subscribe({
                next: (data) => {
                    this.analytics = data;
                },
                error: (err) => console.error('Failed to load analytics', err)
            });
    }

    resetFilters(): void {
        this.selectedDepartmentId = null;
        this.selectedRootGoalId = null;
        this.rootGoals = [];
        this.goalTree = null;
        this.loadAnalytics();
    }

    onNodeSelected(event: { type: 'goal' | 'strategy'; data: GoalTreeNode | StrategyTreeNode }): void {
        this.selectedNodeType = event.type;
        if (event.type === 'goal') {
            this.selectedGoal = event.data as GoalTreeNode;
            this.selectedStrategy = null;
        } else {
            this.selectedStrategy = event.data as StrategyTreeNode;
            this.selectedGoal = null;
        }
        this.drawerOpen = true;
    }

    closeDrawer(): void {
        this.drawerOpen = false;
        this.selectedNodeType = null;
        this.selectedGoal = null;
        this.selectedStrategy = null;
    }
}
