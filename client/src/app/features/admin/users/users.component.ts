import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';
import { UserApiService } from '../../../core/api/user-api.service';
import { DepartmentApiService } from '../../../core/api/department-api.service';
import { Department, User } from '../../../core/api/api.models';
import { AuthService } from '../../../core/auth/auth.service';
import { UserDialogComponent } from './user-dialog.component';
import { forkJoin, map, switchMap, catchError, of, Observable, take } from 'rxjs';

interface UserRow {
  id: string;
  name: string;
  initials: string;
  email: string;
  role: string;
  roleClass: string;
  departments: string[];
  isActive: boolean;
}

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDialogModule, PageHeaderComponent, HasPermissionDirective],
  styles: [`
    .avatar {
      width: 32px;
      height: 32px;
      border-radius: 50%;
      background-color: var(--primary-100);
      color: var(--primary-700);
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-weight: 600;
      font-size: 13px;
      margin-right: 12px;
    }
    .user-name-cell {
      display: flex;
      align-items: center;
    }
    .chip-role {
      font-size: 12px;
      padding: 2px 8px;
      border-radius: 4px;
      font-weight: 500;
      display: inline-block;
      margin-right: 4px;
    }
    .role-org-admin { background: #F3E8FF; color: #7E22CE; }
    .role-dept-manager { background: #DBEAFE; color: #1D4ED8; }
    .role-viewer { background: #F3F4F6; color: #4B5563; }
    .role-analyst { background: #D1FAE5; color: #047857; }
    .role-system-admin { background: #FCE7F3; color: #BE185D; }
    .role-default { background: #F3F4F6; color: #4B5563; }
    
    .dept-badge {
      font-size: 12px;
      padding: 2px 6px;
      border-radius: 4px;
      background: var(--surface-1);
      border: 1px solid var(--border-color);
      color: var(--text-secondary);
      margin-right: 4px;
      display: inline-block;
      margin-top: 2px;
    }
    .user-inactive { opacity: 0.5; }
  `],
  template: `
    <div class="page-enter" style="padding: 24px;">
      <app-page-header title="Users" subtitle="Manage user accounts and permissions">
        <ng-template #actions>
          <button mat-flat-button color="primary" [hasPermission]="'manage_users'" permissionMode="disable" (click)="openAddUserDialog()">
            <mat-icon>add</mat-icon> Add User
          </button>
        </ng-template>
      </app-page-header>

      <div class="table-card" style="margin-top: 16px;">
        @if (loading()) {
            <div style="display: flex; justify-content: center; padding: 48px;">
                <mat-spinner diameter="40"></mat-spinner>
            </div>
        } @else {
            <table mat-table [dataSource]="users()">
            <!-- Name Column -->
            <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Name</th>
                <td mat-cell *matCellDef="let user" class="user-name-cell" [class.user-inactive]="!user.isActive">
                <div class="avatar">{{ user.initials }}</div>
                <strong>{{ user.name }}</strong>
                </td>
            </ng-container>

            <!-- Email Column -->
            <ng-container matColumnDef="email">
                <th mat-header-cell *matHeaderCellDef>Email</th>
                <td mat-cell *matCellDef="let user" [class.user-inactive]="!user.isActive">{{ user.email }}</td>
            </ng-container>

            <!-- Role Column -->
            <ng-container matColumnDef="role">
                <th mat-header-cell *matHeaderCellDef>Role</th>
                <td mat-cell *matCellDef="let user" [class.user-inactive]="!user.isActive">
                <span class="chip-role" [ngClass]="user.roleClass">{{ user.role }}</span>
                </td>
            </ng-container>

            <!-- Departments Column -->
            <ng-container matColumnDef="departments">
                <th mat-header-cell *matHeaderCellDef>Managed Departments</th>
                <td mat-cell *matCellDef="let user" [class.user-inactive]="!user.isActive">
                @if (user.departments.length) {
                    @for (d of user.departments; track d) {
                    <span class="dept-badge">{{ d }}</span>
                    }
                } @else {
                    <span style="color: var(--text-muted)">—</span>
                }
                </td>
            </ng-container>

            <!-- Actions Column -->
            <ng-container matColumnDef="actions">
                <th mat-header-cell *matHeaderCellDef style="text-align: right;">Actions</th>
                <td mat-cell *matCellDef="let user" style="text-align: right;">
                <button mat-icon-button [hasPermission]="'manage_users'" permissionMode="hide" style="color: var(--text-secondary)">
                    <mat-icon>edit</mat-icon>
                </button>
                <button mat-icon-button [hasPermission]="'manage_users'" permissionMode="hide" [style.color]="user.isActive ? 'var(--error)' : 'var(--primary-main)'">
                    <mat-icon>{{ user.isActive ? 'block' : 'restore' }}</mat-icon>
                </button>
                </td>
            </ng-container>

            <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
            <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
            </table>

            @if (users().length === 0) {
                <div style="text-align: center; padding: 48px; color: var(--text-secondary);">
                    No users found.
                </div>
            }
        }
      </div>
    </div>
  `
})
export class UsersComponent implements OnInit {
  displayedColumns = ['name', 'email', 'role', 'departments', 'actions'];
  users = signal<UserRow[]>([]);
  loading = signal<boolean>(true);

  constructor(
    private userService: UserApiService,
    private departmentService: DepartmentApiService,
    private authService: AuthService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  openAddUserDialog(): void {
    const dialogRef = this.dialog.open(UserDialogComponent, {
      width: '500px',
      disableClose: true
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        // Wait briefly for backend Database logic (roles, managers) to finish committing
        setTimeout(() => this.loadData(), 300);
      }
    });
  }

  private loadData(): void {
    this.loading.set(true);

    const deptReq$: Observable<Department[]> = this.authService.user$.pipe(
      take(1),
      switchMap(currentUser => {
        if (!currentUser) return of([]);

        // Org admins and system admins can see departments. 
        // However, our user-api service returns all users the requestor is allowed to see.
        // We'll try fetching all departments the requestor can see, to cross reference managerId
        return this.departmentService.getDepartments({ size: 1000 }).pipe(
          map(res => res.items),
          catchError(() => of([])) // fallback if permission denied to view departments
        );
      })
    );

    forkJoin({
      usersRes: this.userService.getUsers({ size: 100 }).pipe(catchError(() => of({ items: [] as User[] }))),
      departments: deptReq$
    }).subscribe(({ usersRes, departments }) => {
      const users = usersRes.items || [];

      const rows = users.map(u => this.mapUserToRow(u, departments));
      this.users.set(rows);
      this.loading.set(false);
    });
  }

  private mapUserToRow(user: User, allDepartments: Department[]): UserRow {
    // Find if this user manages any departments
    const managedDepts = allDepartments
      .filter(d => d.managerId && d.managerId.toLowerCase() === user.id.toLowerCase())
      .map(d => d.name);

    const primaryRole = user.roles && user.roles.length > 0 ? user.roles[0] : 'Unknown';

    return {
      id: user.id,
      name: `${user.firstName} ${user.lastName}`,
      initials: `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`.toUpperCase(),
      email: user.email,
      role: primaryRole,
      roleClass: this.getRoleBadgeClass(primaryRole),
      departments: managedDepts,
      isActive: user.isActive
    };
  }

  private getRoleBadgeClass(roleName: string): string {
    const lower = roleName.toLowerCase();
    if (lower.includes('system admin')) return 'role-system-admin';
    if (lower.includes('organization admin') || lower.includes('org admin')) return 'role-org-admin';
    if (lower.includes('department manager')) return 'role-dept-manager';
    if (lower.includes('analyst')) return 'role-analyst';
    if (lower.includes('viewer')) return 'role-viewer';
    return 'role-default';
  }
}
