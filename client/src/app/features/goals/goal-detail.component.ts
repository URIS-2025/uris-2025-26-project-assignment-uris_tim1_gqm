import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDividerModule } from '@angular/material/divider';
import { MatTabsModule } from '@angular/material/tabs';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { GoalApiService } from '../../core/api/goal-api.service';
import { GoalDetails } from '../../core/api/api.models';

@Component({
    selector: 'app-goal-detail',
    standalone: true,
    imports: [
        RouterLink, MatCardModule, MatButtonModule, MatIconModule,
        MatProgressSpinnerModule, MatDividerModule, MatTabsModule,
        PageHeaderComponent, HasPermissionDirective
    ],
    templateUrl: './goal-detail.component.html',
    styleUrl: './goal-detail.component.css',
})
export class GoalDetailComponent implements OnInit {
    goal: GoalDetails | null = null;
    loading = true;
    error = '';

    constructor(private route: ActivatedRoute, private goalApi: GoalApiService) { }

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id')!;
        this.goalApi.getDetails(id).subscribe({
            next: goal => { this.goal = goal; this.loading = false; },
            error: () => { this.error = 'Failed to load goal details.'; this.loading = false; }
        });
    }

    formatDate(d: string): string {
        return d ? new Date(d).toLocaleDateString() : '—';
    }
}
