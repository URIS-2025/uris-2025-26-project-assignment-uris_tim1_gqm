import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AsyncPipe } from '@angular/common';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [MatToolbarModule, MatIconModule, MatButtonModule, MatMenuModule, MatDividerModule, AsyncPipe],
    templateUrl: './topbar.component.html',
    styleUrl: './topbar.component.css',
})
export class TopbarComponent {
    @Input() sidenavOpened = true;
    @Output() toggleSidenav = new EventEmitter<void>();

    get user$() {
        return this.auth.user$;
    }

    constructor(private auth: AuthService, private router: Router) { }

    logout(): void {
        this.auth.logout();
    }

    get initials(): string {
        const user = this.auth.currentUser;
        if (!user) return '?';
        return `${user.firstName[0]}${user.lastName[0]}`.toUpperCase();
    }
}
