import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { PageHeaderComponent } from '../../../shared/components/page-header.component';
import { HasPermissionDirective } from '../../../core/permissions/has-permission.directive';

interface RolePerms {
    name: string;
    badgeClass: string;
    desc: string;
    perms: { name: string; has: boolean }[];
}

@Component({
    selector: 'app-roles',
    standalone: true,
    imports: [CommonModule, MatCardModule, MatIconModule, PageHeaderComponent, HasPermissionDirective],
    styles: [`
    .roles-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(380px, 1fr));
      gap: 24px;
      margin-top: 16px;
    }
    .role-card {
      border: 1px solid var(--border-color);
      box-shadow: var(--shadow-sm);
      height: 100%;
    }
    .role-header {
      display: flex;
      align-items: center;
      gap: 12px;
      margin-bottom: 12px;
    }
    .role-icon-wrap {
      width: 40px;
      height: 40px;
      border-radius: var(--radius-md);
      background: var(--background);
      display: flex;
      align-items: center;
      justify-content: center;
      color: var(--primary-400);
    }
    .chip-role {
      font-size: 11px;
      padding: 2px 8px;
      border-radius: 4px;
      font-weight: 500;
      display: inline-block;
      margin-bottom: 12px;
    }
    .role-desc {
      font-size: 13px;
      color: var(--text-secondary);
      margin-bottom: 24px;
      line-height: 1.4;
    }
    .perm-list {
      display: flex;
      flex-direction: column;
      gap: 8px;
    }
    .perm-item {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 13px;
    }
    .perm-item mat-icon.yes { color: var(--success); font-size: 16px; width: 16px; height: 16px; }
    .perm-item mat-icon.no { color: var(--text-muted); font-size: 16px; width: 16px; height: 16px; }
    .perm-item span.no-text { color: var(--text-muted); text-decoration: line-through; }
    
    .role-org-admin { background: #F3E8FF; color: #7E22CE; }
    .role-dept-manager { background: #DBEAFE; color: #1D4ED8; }
    .role-viewer { background: #F3F4F6; color: #4B5563; }
    .role-analyst { background: #D1FAE5; color: #047857; }
    .role-system-admin { background: #FCE7F3; color: #BE185D; }
  `],
    template: `
    <div class="page-enter" style="padding: 24px;">
      <app-page-header title="Roles & Permissions" subtitle="Permission-based access control system"></app-page-header>

      <div class="roles-grid" [hasPermission]="'manage_users'" permissionMode="disable" style="opacity: 1;">
        @for (r of roles; track r.name) {
          <mat-card class="role-card">
            <mat-card-content style="padding: 24px;">
              <div class="role-header">
                <div class="role-icon-wrap"><mat-icon>shield_outline</mat-icon></div>
                <h3 style="margin: 0; font-size: 18px; font-weight: 600;">{{ r.name }}</h3>
              </div>
              
              <span class="chip-role" [ngClass]="r.badgeClass">{{ r.name }}</span>
              <div class="role-desc">{{ r.desc }}</div>
              
              <div style="font-weight: 600; font-size: 13px; margin-bottom: 12px;">Permissions</div>
              <div class="perm-list">
                @for (p of r.perms; track p.name) {
                  <div class="perm-item">
                    @if (p.has) {
                      <mat-icon class="yes">check</mat-icon>
                      <span>{{ p.name }}</span>
                    } @else {
                      <mat-icon class="no">close</mat-icon>
                      <span class="no-text">{{ p.name }}</span>
                    }
                  </div>
                }
              </div>
            </mat-card-content>
          </mat-card>
        }
      </div>
    </div>
  `
})
export class RolesComponent {

    public roles: RolePerms[] = [
        {
            name: 'System Admin', badgeClass: 'role-system-admin', desc: 'Complete system access including multi-tenant management',
            perms: [
                { name: 'Manage Organizations', has: true }, { name: 'Manage Users', has: true }, { name: 'Manage Roles', has: true },
                { name: 'Manage Departments', has: true }, { name: 'View All Departments', has: true }, { name: 'Create Goals', has: true },
                { name: 'Edit Goals', has: true }, { name: 'Delete Goals', has: true }, { name: 'View Goals', has: true },
                { name: 'Manage Goal Influences', has: true }, { name: 'Record Measurements', has: true }, { name: 'Manage Probability Assessments', has: true },
                { name: 'View Analytics', has: true }
            ]
        },
        {
            name: 'Organization Admin', badgeClass: 'role-org-admin', desc: 'Full access to organizational features and settings',
            perms: [
                { name: 'Manage Organizations', has: false }, { name: 'Manage Users', has: true }, { name: 'Manage Roles', has: true },
                { name: 'Manage Departments', has: true }, { name: 'View All Departments', has: true }, { name: 'Create Goals', has: true },
                { name: 'Edit Goals', has: true }, { name: 'Delete Goals', has: true }, { name: 'View Goals', has: true },
                { name: 'Manage Goal Influences', has: true }, { name: 'Record Measurements', has: true }, { name: 'Manage Probability Assessments', has: true },
                { name: 'View Analytics', has: true }
            ]
        },
        {
            name: 'Department Manager', badgeClass: 'role-dept-manager', desc: 'Manage goals and teams within assigned departments',
            perms: [
                { name: 'Manage Organizations', has: false }, { name: 'Manage Users', has: false }, { name: 'Manage Roles', has: false },
                { name: 'Manage Departments', has: false }, { name: 'View All Departments', has: false }, { name: 'Create Goals', has: true },
                { name: 'Edit Goals', has: true }, { name: 'Delete Goals', has: false }, { name: 'View Goals', has: true },
                { name: 'Manage Goal Influences', has: true }, { name: 'Record Measurements', has: true }, { name: 'Manage Probability Assessments', has: true },
                { name: 'View Analytics', has: true }
            ]
        },
        {
            name: 'Analyst', badgeClass: 'role-analyst', desc: 'Record measurements and probability assessments',
            perms: [
                { name: 'Manage Organizations', has: false }, { name: 'Manage Users', has: false }, { name: 'Manage Roles', has: false },
                { name: 'Manage Departments', has: false }, { name: 'View All Departments', has: false }, { name: 'Create Goals', has: false },
                { name: 'Edit Goals', has: false }, { name: 'Delete Goals', has: false }, { name: 'View Goals', has: true },
                { name: 'Manage Goal Influences', has: false }, { name: 'Record Measurements', has: true }, { name: 'Manage Probability Assessments', has: true },
                { name: 'View Analytics', has: true }
            ]
        },
        {
            name: 'Viewer', badgeClass: 'role-viewer', desc: 'Read-only access to goals and analytics',
            perms: [
                { name: 'Manage Organizations', has: false }, { name: 'Manage Users', has: false }, { name: 'Manage Roles', has: false },
                { name: 'Manage Departments', has: false }, { name: 'View All Departments', has: false }, { name: 'Create Goals', has: false },
                { name: 'Edit Goals', has: false }, { name: 'Delete Goals', has: false }, { name: 'View Goals', has: true },
                { name: 'Manage Goal Influences', has: false }, { name: 'Record Measurements', has: false }, { name: 'Manage Probability Assessments', has: false },
                { name: 'View Analytics', has: true }
            ]
        }
    ];
}
