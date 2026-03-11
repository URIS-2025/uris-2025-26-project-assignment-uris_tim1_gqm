import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatDialog } from '@angular/material/dialog';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { GoalApiService } from '../../core/api/goal-api.service';
import { DepartmentApiService } from '../../core/api/department-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { Goal } from '../../core/api/api.models';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-goals-list',
    standalone: true,
    imports: [
        RouterLink, FormsModule, CommonModule,
        MatButtonModule, MatIconModule,
        MatProgressSpinnerModule, MatPaginatorModule,
        MatFormFieldModule, MatSelectModule, MatInputModule,
        PageHeaderComponent, HasPermissionDirective,
    ],
    templateUrl: './goals-list.component.html',
    styleUrl: './goals-list.component.css',
})
export class GoalsListComponent implements OnInit {
    allGoals: Goal[] = [];
    goals: Goal[] = [];
    totalCount = 0;
    pageSize = 10;
    pageNumber = 1;
    loading = true;

    searchQuery = '';
    statusFilter = '';

    statusColors: Record<string, string> = {
        'Active': 'chip-success',
        'OnHold': 'chip-warning',
        'Completed': 'chip-info',
        'Cancelled': 'chip-default',
    };

    private auth = inject(AuthService);
    private deptApi = inject(DepartmentApiService);
    private destroyRef = inject(DestroyRef);

    constructor(private goalApi: GoalApiService, private dialog: MatDialog) { }

    ngOnInit(): void {
        this.auth.organizationId$.pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe(() => {
            this.pageNumber = 1;
            this.loadGoals();
        });
    }

    loadGoals(): void {
        this.loading = true;
        const orgId = this.auth.organizationId;

        const depts$ = orgId
            ? this.deptApi.getDepartmentsByOrg(orgId, { page: 1, size: 100 })
            : this.deptApi.getDepartments({ page: 1, size: 100 });

        import('rxjs').then(({ forkJoin }) => {
            forkJoin({
                goalsRes: this.goalApi.getAll({ pageNumber: 1, pageSize: 1000 }), // fetch large to filter client side
                departmentsRes: depts$
            }).subscribe({
                next: ({ goalsRes, departmentsRes }) => {
                    const deptIds = new Set(departmentsRes.items.map((d: any) => d.id));
                    const filteredGoals = (goalsRes.items ?? []).filter(g => deptIds.has(g.departmentId));

                    this.allGoals = filteredGoals;
                    this.loading = false;
                    this.applyFilters();
                },
                error: () => { this.loading = false; }
            });
        });
    }

    applyFilters(): void {
        let filtered = [...this.allGoals];

        // Status filter
        if (this.statusFilter) {
            filtered = filtered.filter(g => g.status === this.statusFilter);
        }

        // Search filter (client-side)
        if (this.searchQuery.trim()) {
            const q = this.searchQuery.toLowerCase();
            filtered = filtered.filter(g =>
                g.focus.toLowerCase().includes(q) ||
                g.object.toLowerCase().includes(q) ||
                g.magnitude.toLowerCase().includes(q)
            );
        }

        this.totalCount = filtered.length;

        // Client-side Pagination
        const start = (this.pageNumber - 1) * this.pageSize;
        this.goals = filtered.slice(start, start + this.pageSize);
    }

    onSearch(): void {
        this.applyFilters();
    }

    onStatusChange(): void {
        this.applyFilters();
    }

    onPage(e: PageEvent): void {
        this.pageNumber = e.pageIndex + 1;
        this.pageSize = e.pageSize;
        this.loadGoals();
    }

    deleteGoal(goal: Goal): void {
        const ref = this.dialog.open(ConfirmDialogComponent, {
            data: { title: 'Delete Goal', message: `Delete "${goal.focus}"? This cannot be undone.`, danger: true, confirmLabel: 'Delete' }
        });
        ref.afterClosed().subscribe(confirmed => {
            if (confirmed) {
                this.goalApi.delete(goal.id).subscribe(() => this.loadGoals());
            }
        });
    }

    formatDate(d: string): string {
        return d ? new Date(d).toLocaleDateString() : '—';
    }

    getDaysRemaining(activeTo: string): string {
        if (!activeTo) return '';
        const now = new Date();
        const end = new Date(activeTo);
        const diff = Math.ceil((end.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
        if (diff < 0) return 'Expired';
        return `${diff} days remaining`;
    }
}
