import { CommonModule } from '@angular/common';
import { Component, Input, ContentChild, TemplateRef } from '@angular/core';

@Component({
  selector: 'app-page-header',
  standalone: true,
  template: `
    <div class="page-header">
      <div class="page-header-text">
        <h1 class="page-title">{{ title }}</h1>
        @if (subtitle) {
          <p class="page-subtitle">{{ subtitle }}</p>
        }
      </div>
      @if (actions) {
        <div class="page-header-actions">
          <ng-container [ngTemplateOutlet]="actions"></ng-container>
        </div>
      }
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      margin-bottom: 24px;
      gap: 16px;
    }
    .page-title {
      font-size: 22px;
      font-weight: 700;
      color: var(--text-primary);
      letter-spacing: -0.02em;
      margin: 0;
    }
    .page-subtitle {
      font-size: 13px;
      color: var(--text-secondary);
      margin: 4px 0 0;
    }
    .page-header-actions {
      display: flex;
      gap: 8px;
      flex-shrink: 0;
      align-items: center;
    }
  `],
  imports: [CommonModule]
})
export class PageHeaderComponent {
  @Input() title = '';
  @Input() subtitle?: string;
  @ContentChild('actions') actions?: TemplateRef<unknown>;
}
