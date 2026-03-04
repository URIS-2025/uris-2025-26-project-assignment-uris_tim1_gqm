import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
    selector: 'app-forbidden',
    standalone: true,
    imports: [RouterLink, MatButtonModule, MatIconModule],
    template: `
    <div class="forbidden-page">
      <span class="material-icons-round error-icon">lock</span>
      <h1>Access Denied</h1>
      <p>You don't have permission to view this page.</p>
      <button mat-flat-button routerLink="/dashboard">
        <mat-icon>home</mat-icon>
        Back to Dashboard
      </button>
    </div>
  `,
    styles: [`
    .forbidden-page {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      min-height: 60vh;
      gap: 16px;
      text-align: center;
    }
    .error-icon {
      font-size: 72px;
      width: 72px;
      height: 72px;
      color: var(--error);
      opacity: 0.6;
    }
    h1 {
      font-size: 28px;
      font-weight: 700;
      color: var(--text-primary);
      margin: 0;
    }
    p {
      color: var(--text-secondary);
      margin: 0;
    }
  `]
})
export class ForbiddenComponent { }
