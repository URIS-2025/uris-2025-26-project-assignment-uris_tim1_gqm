import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { GoalApiService } from '../../core/api/goal-api.service';
import { DepartmentApiService } from '../../core/api/department-api.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [RouterLink, MatCardModule, MatButtonModule, MatIconModule, PageHeaderComponent, HasPermissionDirective],
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.css',
})
export class DashboardComponent implements OnInit {
    stats = {
        goals: 0,
        departments: 0,
    };
    loading = true;
    userName = '';

    constructor(
        private goalApi: GoalApiService,
        private deptApi: DepartmentApiService,
        private auth: AuthService
    ) { }

    ngOnInit(): void {
        const user = this.auth.currentUser;
        this.userName = user ? user.firstName : '';

        this.goalApi.getAll({ pageNumber: 1, pageSize: 1 }).subscribe({
            next: res => { this.stats.goals = res.totalCount; },
            error: () => { },
        });

        this.deptApi.getDepartments({ page: 1, size: 1 }).subscribe({
            next: res => {
                this.stats.departments = res.totalCount;
                this.loading = false;
            },
            error: () => { this.loading = false; },
        });
    }
}
