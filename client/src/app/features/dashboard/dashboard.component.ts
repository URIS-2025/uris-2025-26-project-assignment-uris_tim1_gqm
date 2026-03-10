import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { forkJoin } from 'rxjs';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { GoalApiService } from '../../core/api/goal-api.service';
import { DepartmentApiService } from '../../core/api/department-api.service';
import { AssessmentApiService } from '../../core/api/assessment-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { Goal, Assessment, Department } from '../../core/api/api.models';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule, RouterLink, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, PageHeaderComponent, HasPermissionDirective],
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
    loading = true;
    userName = '';
    userRole = '';

    goals: Goal[] = [];
    allGoals: Goal[] = [];
    assessments: Assessment[] = [];
    departments: Department[] = [];

    // KPIs
    activeGoals = 0;
    completedGoals = 0;
    avgProbability = 0;
    highRiskGoals = 0;

    // Chart data
    statusDistribution: { status: string; count: number; color: string }[] = [];
    goalsByDept: { name: string; count: number }[] = [];
    recentAssessments: (Assessment & { goalFocus?: string })[] = [];
    activeGoalsList: (Goal & { deptName?: string; probability?: number })[] = [];

    private destroyRef = inject(DestroyRef);

    constructor(
        private goalApi: GoalApiService,
        private deptApi: DepartmentApiService,
        private assessmentApi: AssessmentApiService,
        private auth: AuthService
    ) { }

    ngOnInit(): void {
        const user = this.auth.currentUser;
        this.userName = user ? user.firstName : '';
        if (user?.isSystemAdmin) {
            this.userRole = 'SystemAdmin';
        } else if (user?.permissions?.includes('manage_organizations')) {
            this.userRole = 'OrgAdmin';
        } else if (user?.permissions?.includes('manage_departments')) {
            this.userRole = 'DeptManager';
        } else if (user?.permissions?.includes('record_measurements')) {
            this.userRole = 'Analyst';
        } else {
            this.userRole = 'Viewer';
        }

        this.auth.organizationId$.pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe(() => this.loadData());
    }

    private loadData(): void {
        this.loading = true;
        const orgId = this.auth.organizationId;

        const depts$ = orgId
            ? this.deptApi.getDepartmentsByOrg(orgId, { page: 1, size: 100 })
            : this.deptApi.getDepartments({ page: 1, size: 100 });

        forkJoin({
            goalsRes: this.goalApi.getAll({ pageNumber: 1, pageSize: 100 }),
            departmentsRes: depts$,
            assessmentsRes: this.assessmentApi.getAll({ page: 1, size: 100 }),
        }).subscribe({
            next: ({ goalsRes, departmentsRes, assessmentsRes }) => {
                const deptIds = new Set(departmentsRes.items.map(d => d.id));
                const filteredGoals = goalsRes.items.filter(g => deptIds.has(g.departmentId));

                this.allGoals = goalsRes.items;
                this.goals = filteredGoals;
                this.departments = departmentsRes.items;
                this.assessments = assessmentsRes.items;
                this.computeKPIs();
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    private computeKPIs(): void {
        this.activeGoals = this.goals.filter(g => g.status === 'Active').length;
        this.completedGoals = this.goals.filter(g => g.status === 'Completed').length;
        const draftGoals = this.goals.filter(g => g.status === 'Draft').length;
        const onHoldGoals = this.goals.filter(g => g.status === 'OnHold').length;
        const cancelledGoals = this.goals.filter(g => g.status === 'Cancelled').length;

        this.avgProbability = this.goals.length > 0
            ? this.goals.reduce((sum, g) => sum + g.baselineProbability, 0) / this.goals.length
            : 0;

        this.highRiskGoals = this.goals.filter(g =>
            g.baselineProbability < 0.6 && g.status === 'Active'
        ).length;

        // Status distribution
        this.statusDistribution = [
            { status: 'Active', count: this.activeGoals, color: '#3F51B5' },
            { status: 'Draft', count: draftGoals, color: '#64748B' },
            { status: 'On Hold', count: onHoldGoals, color: '#F59E0B' },
            { status: 'Completed', count: this.completedGoals, color: '#2CB1A1' },
            { status: 'Cancelled', count: cancelledGoals, color: '#9CA3AF' },
        ].filter(s => s.count > 0);

        // Goals by department
        const deptMap = new Map(this.departments.map(d => [d.id, d.name]));
        const deptCounts = new Map<string, number>();
        this.goals.forEach(g => {
            const name = deptMap.get(g.departmentId) || 'Unknown';
            deptCounts.set(name, (deptCounts.get(name) || 0) + 1);
        });
        this.goalsByDept = Array.from(deptCounts.entries())
            .map(([name, count]) => ({ name, count }))
            .sort((a, b) => b.count - a.count);

        // Recent assessments (last 5, sorted by date)
        const goalMap = new Map(this.allGoals.map(g => [g.id, g.focus]));
        this.recentAssessments = [...this.assessments]
            .sort((a, b) => new Date(b.assessedAt || '').getTime() - new Date(a.assessedAt || '').getTime())
            .slice(0, 5)
            .map(a => ({ ...a, goalFocus: goalMap.get(a.goalId) || 'Unknown Goal' }));

        // Active goals list with department names and latest probability
        this.activeGoalsList = this.goals
            .filter(g => g.status === 'Active')
            .map(g => {
                const goalAssessments = this.assessments.filter(a => a.goalId === g.id);
                const latest = goalAssessments.sort((a, b) =>
                    new Date(b.assessedAt || '').getTime() - new Date(a.assessedAt || '').getTime()
                )[0];
                return {
                    ...g,
                    deptName: deptMap.get(g.departmentId) || 'Unknown',
                    probability: latest?.probability ?? g.baselineProbability,
                };
            });
    }

    getMaxDeptCount(): number {
        return this.goalsByDept.length > 0 ? Math.max(...this.goalsByDept.map(d => d.count)) : 1;
    }

    getBarWidth(count: number): number {
        return Math.max(8, (count / this.getMaxDeptCount()) * 100);
    }

    getMaxStatusCount(): number {
        return this.statusDistribution.length > 0 ? Math.max(...this.statusDistribution.map(s => s.count)) : 1;
    }

    getStatusBarHeight(count: number): number {
        return Math.max(8, (count / this.getMaxStatusCount()) * 100);
    }

    getProbabilityColor(p: number): string {
        if (p >= 0.7) return '#059669';
        if (p >= 0.5) return '#D97706';
        return '#DC2626';
    }

    getProbabilityBg(p: number): string {
        if (p >= 0.7) return '#D1FAE5';
        if (p >= 0.5) return '#FEF3C7';
        return '#FEE2E2';
    }

    formatDate(dateStr: string | undefined): string {
        if (!dateStr) return '';
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    }

    formatDateTime(dateStr: string | undefined): string {
        if (!dateStr) return '';
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-US', {
            month: 'short', day: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    }

    formatShortDate(dateStr: string | undefined): string {
        if (!dateStr) return '';
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-US', { month: 'numeric', day: 'numeric', year: 'numeric' });
    }

    getRoleDisplay(): string {
        const map: Record<string, string> = {
            'SystemAdmin': 'System Admin',
            'OrgAdmin': 'Org Admin',
            'DeptManager': 'Dept Manager',
            'Analyst': 'Analyst',
            'Viewer': 'Viewer',
        };
        return map[this.userRole] || this.userRole;
    }
}
