import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { SidebarComponent } from './sidebar/sidebar.component';
import { TopbarComponent } from './topbar/topbar.component';
import { BreadcrumbComponent } from './breadcrumb/breadcrumb.component';

@Component({
    selector: 'app-layout',
    standalone: true,
    imports: [RouterOutlet, MatSidenavModule, SidebarComponent, TopbarComponent, BreadcrumbComponent],
    templateUrl: './layout.component.html',
    styleUrl: './layout.component.css',
})
export class LayoutComponent {
    sidenavOpened = true;

    toggleSidenav(): void {
        this.sidenavOpened = !this.sidenavOpened;
    }
}
