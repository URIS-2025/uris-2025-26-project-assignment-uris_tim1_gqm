import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';

@Component({
    selector: 'app-users',
    standalone: true,
    imports: [CommonModule, MatTableModule, MatButtonModule, MatIconModule, PageHeaderComponent, HasPermissionDirective],
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
    }
    .role-org-admin { background: #F3E8FF; color: #7E22CE; }
    .role-dept-manager { background: #DBEAFE; color: #1D4ED8; }
    .role-viewer { background: #F3F4F6; color: #4B5563; }
    .role-analyst { background: #D1FAE5; color: #047857; }
    .role-system-admin { background: #FCE7F3; color: #BE185D; }
    
    .dept-badge {
      font-size: 12px;
      padding: 2px 6px;
      border-radius: 4px;
      background: var(--surface-1);
      border: 1px solid var(--border-color);
      color: var(--text-secondary);
      margin-right: 4px;
    }
  `],
    template: `
    <div class="page-enter" style="padding: 24px;">
      <app-page-header title="Users" subtitle="Manage user accounts and permissions">
        <ng-template #actions>
          <button mat-flat-button color="primary" [hasPermission]="'manage_users'" permissionMode="disable">
            <mat-icon>add</mat-icon> Add User
          </button>
        </ng-template>
      </app-page-header>

      <div class="table-card" style="margin-top: 16px;">
        <table mat-table [dataSource]="users">
          <!-- Name Column -->
          <ng-container matColumnDef="name">
            <th mat-header-cell *matHeaderCellDef>Name</th>
            <td mat-cell *matCellDef="let user" class="user-name-cell">
              <div class="avatar">{{ user.initials }}</div>
              <strong>{{ user.name }}</strong>
            </td>
          </ng-container>

          <!-- Email Column -->
          <ng-container matColumnDef="email">
            <th mat-header-cell *matHeaderCellDef>Email</th>
            <td mat-cell *matCellDef="let user">{{ user.email }}</td>
          </ng-container>

          <!-- Role Column -->
          <ng-container matColumnDef="role">
            <th mat-header-cell *matHeaderCellDef>Role</th>
            <td mat-cell *matCellDef="let user">
              <span class="chip-role" [ngClass]="user.roleClass">{{ user.role }}</span>
            </td>
          </ng-container>

          <!-- Departments Column -->
          <ng-container matColumnDef="departments">
            <th mat-header-cell *matHeaderCellDef>Managed Departments</th>
            <td mat-cell *matCellDef="let user">
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
              <button mat-icon-button [hasPermission]="'manage_users'" permissionMode="hide" style="color: var(--text-secondary)"><mat-icon>edit</mat-icon></button>
              <button mat-icon-button [hasPermission]="'manage_users'" permissionMode="hide" style="color: var(--error)"><mat-icon>delete_outline</mat-icon></button>
            </td>
          </ng-container>

          <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
          <tr mat-row *matRowDef="let row; columns: displayedColumns;"></tr>
        </table>
      </div>
    </div>
  `
})
export class UsersComponent {
    displayedColumns = ['name', 'email', 'role', 'departments', 'actions'];

    users = [
        { name: 'Alice Johnson', initials: 'AJ', email: 'alice@acme.com', role: 'Org Admin', roleClass: 'role-org-admin', departments: [] },
        { name: 'Bob Smith', initials: 'BS', email: 'bob@acme.com', role: 'Dept Manager', roleClass: 'role-dept-manager', departments: ['Engineering'] },
        { name: 'Carol Davis', initials: 'CD', email: 'carol@acme.com', role: 'Dept Manager', roleClass: 'role-dept-manager', departments: ['Product'] },
        { name: 'David Wilson', initials: 'DW', email: 'david@acme.com', role: 'Dept Manager', roleClass: 'role-dept-manager', departments: ['Sales'] },
        { name: 'Emma Brown', initials: 'EB', email: 'emma@acme.com', role: 'Dept Manager', roleClass: 'role-dept-manager', departments: ['Marketing'] },
        { name: 'Frank Miller', initials: 'FM', email: 'frank@acme.com', role: 'Viewer', roleClass: 'role-viewer', departments: [] },
        { name: 'Grace Chen', initials: 'GC', email: 'grace@acme.com', role: 'Analyst', roleClass: 'role-analyst', departments: [] },
        { name: 'Henry Park', initials: 'HP', email: 'henry@acme.com', role: 'System Admin', roleClass: 'role-system-admin', departments: [] },
    ];
}
