import { Component, Inject } from '@angular/core';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
    title: string;
    message: string;
    confirmLabel?: string;
    cancelLabel?: string;
    danger?: boolean;
}

@Component({
    selector: 'app-confirm-dialog',
    standalone: true,
    imports: [MatDialogModule, MatButtonModule, MatIconModule],
    template: `
    <div class="confirm-dialog">
      <div class="confirm-header">
        <mat-icon class="confirm-icon" [class.danger]="data.danger">
          {{ data.danger ? 'warning' : 'help_outline' }}
        </mat-icon>
        <h2 class="confirm-title">{{ data.title }}</h2>
      </div>
      <p class="confirm-message">{{ data.message }}</p>
      <div class="confirm-actions">
        <button mat-stroked-button (click)="dialogRef.close(false)">
          {{ data.cancelLabel ?? 'Cancel' }}
        </button>
        <button
          mat-flat-button
          [color]="data.danger ? 'warn' : 'primary'"
          (click)="dialogRef.close(true)"
        >
          {{ data.confirmLabel ?? 'Confirm' }}
        </button>
      </div>
    </div>
  `,
    styles: [`
    .confirm-dialog { padding: 8px; }
    .confirm-header { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; }
    .confirm-icon { font-size: 28px; width: 28px; height: 28px; color: var(--primary-400); }
    .confirm-icon.danger { color: var(--error); }
    .confirm-title { font-size: 18px; font-weight: 600; color: var(--text-primary); margin: 0; }
    .confirm-message { color: var(--text-secondary); font-size: 14px; margin: 0 0 24px; line-height: 1.6; }
    .confirm-actions { display: flex; justify-content: flex-end; gap: 8px; }
  `]
})
export class ConfirmDialogComponent {
    constructor(
        public dialogRef: MatDialogRef<ConfirmDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
    ) { }
}
