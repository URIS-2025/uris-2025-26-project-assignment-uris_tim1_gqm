import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { GoalApiService } from '../../core/api/goal-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { Goal } from '../../core/api/api.models';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-goals-list',
    standalone: true,
    imports: [
        RouterLink,
        MatTableModule, MatButtonModule, MatIconModule, MatChipsModule,
        MatProgressSpinnerModule, MatPaginatorModule,
        PageHeaderComponent, HasPermissionDirective, CommonModule,
    ],
    templateUrl: './goals-list.component.html',
    styleUrl: './goals-list.component.css',
})
export class GoalsListComponent implements OnInit {
    goals: Goal[] = [];
    totalCount = 0;
    pageSize = 10;
    pageNumber = 1;
    loading = true;

    displayedColumns = ['focus', 'object', 'status', 'activeFrom', 'activeTo', 'actions'];

    statusColors: Record<string, string> = {
        'Active': 'chip-success',
        'OnHold': 'chip-warning',
        'Completed': 'chip-info',
        'Cancelled': 'chip-default',
    };

    private auth = inject(AuthService);
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
        this.goalApi.getAll({ pageNumber: this.pageNumber, pageSize: this.pageSize }).subscribe({
            next: res => {
                this.goals = res.items ?? [];
                this.totalCount = res.totalCount ?? 0;
                this.loading = false;
            },
            error: () => { this.loading = false; }
        });
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
}
