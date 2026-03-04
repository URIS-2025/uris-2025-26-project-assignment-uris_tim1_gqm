import { Component } from '@angular/core';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-analytics',
    standalone: true,
    imports: [PageHeaderComponent, MatCardModule, MatIconModule],
    template: `
    <div class="page-enter" style="padding: 24px;">
      <app-page-header title="Analytics" subtitle="Insights and cross-organizational reporting."></app-page-header>
      
      <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 24px;">
        <mat-card>
            <mat-card-content style="display:flex; flex-direction:column; align-items:center; padding:48px 24px; color:var(--text-muted); text-align:center;">
                <mat-icon style="font-size: 48px; width:48px; height:48px; margin-bottom:16px;">query_stats</mat-icon>
                <h3 style="color:var(--text-primary); margin-bottom: 8px;">Analytics Dashboard</h3>
                <p>Advanced metrics and goal performance trends will appear here.</p>
            </mat-card-content>
        </mat-card>
      </div>
    </div>
  `
})
export class AnalyticsComponent { }
