import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatRippleModule } from '@angular/material/core';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { CommonModule } from '@angular/common';

interface NavItem {
    label: string;
    icon: string;
    route: string;
    permission?: string;
    children?: NavItem[];
}

const NAV_ITEMS: NavItem[] = [
    { label: 'Dashboard', icon: 'grid_view', route: '/dashboard' },
    { label: 'Goals', icon: 'my_location', route: '/goals', permission: 'view_goals' },
    { label: 'Analytics', icon: 'bar_chart', route: '/analytics', permission: 'view_analytics' },
    { label: 'Departments', icon: 'account_tree', route: '/admin/departments', permission: 'manage_departments' },
    { label: 'Users', icon: 'people_outline', route: '/admin/users', permission: 'manage_users' },
    { label: 'Roles & Permissions', icon: 'shield', route: '/admin/roles', permission: 'manage_users' },
];

@Component({
    selector: 'app-sidebar',
    standalone: true,
    imports: [RouterLink, RouterLinkActive, MatIconModule, MatRippleModule, HasPermissionDirective, CommonModule],
    templateUrl: './sidebar.component.html',
    styleUrl: './sidebar.component.css',
})
export class SidebarComponent {
    navItems = NAV_ITEMS;
}
