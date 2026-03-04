import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd, ActivatedRoute } from '@angular/router';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { filter, map } from 'rxjs/operators';

interface Breadcrumb {
    label: string;
    url: string;
}

@Component({
    selector: 'app-breadcrumb',
    standalone: true,
    imports: [RouterLink, MatIconModule],
    template: `
    @if (breadcrumbs.length > 1) {
      <nav class="breadcrumb" aria-label="Breadcrumb">
        @for (crumb of breadcrumbs; track crumb.url; let last = $last) {
          @if (!last) {
            <a [routerLink]="crumb.url" class="crumb crumb-link">{{ crumb.label }}</a>
            <span class="material-icons-round crumb-sep">chevron_right</span>
          } @else {
            <span class="crumb crumb-active">{{ crumb.label }}</span>
          }
        }
      </nav>
    }
  `,
    styles: [`
    .breadcrumb {
      display: flex;
      align-items: center;
      gap: 2px;
      padding: 8px 24px 0;
      flex-shrink: 0;
    }
    .crumb {
      font-size: 12px;
      font-weight: 500;
    }
    .crumb-link {
      color: var(--text-secondary);
      text-decoration: none;
      transition: color var(--transition-fast);
    }
    .crumb-link:hover { color: var(--text-primary); }
    .crumb-active { color: var(--text-primary); }
    .crumb-sep {
      font-size: 14px;
      color: var(--text-muted);
    }
  `]
})
export class BreadcrumbComponent implements OnInit {
    breadcrumbs: Breadcrumb[] = [];

    private readonly LABELS: Record<string, string> = {
        '': 'Home',
        'dashboard': 'Dashboard',
        'goals': 'Goals',
        'new': 'New Goal',
        'detail': 'Goal Detail',
        'admin': 'Admin',
        'organizations': 'Organizations',
        'departments': 'Departments',
        'auth': 'Authentication',
        'login': 'Login',
        'forbidden': 'Forbidden',
    };

    constructor(private router: Router, private activatedRoute: ActivatedRoute) { }

    ngOnInit(): void {
        this.router.events.pipe(
            filter(e => e instanceof NavigationEnd)
        ).subscribe(() => {
            this.breadcrumbs = this._buildBreadcrumbs();
        });
        this.breadcrumbs = this._buildBreadcrumbs();
    }

    private _buildBreadcrumbs(): Breadcrumb[] {
        const url = this.router.url.split('?')[0];
        const segments = url.split('/').filter(Boolean);
        const crumbs: Breadcrumb[] = [];
        let currentUrl = '';

        for (const seg of segments) {
            currentUrl += `/${seg}`;
            const label = this.LABELS[seg] ?? this._titleCase(seg);
            crumbs.push({ label, url: currentUrl });
        }

        return crumbs;
    }

    private _titleCase(s: string): string {
        return s.replace(/-/g, ' ').replace(/\b\w/g, c => c.toUpperCase());
    }
}
