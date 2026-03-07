import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

/**
 * Reusable error boundary component that wraps content and displays
 * a user-friendly error state when an error message is provided.
 *
 * Usage:
 * ```html
 * <app-error-boundary [errorMessage]="errorMsg()" (retry)="reload()">
 *   <p>Your content here</p>
 * </app-error-boundary>
 * ```
 */
@Component({
    selector: 'app-error-boundary',
    standalone: true,
    imports: [CommonModule, MatIconModule, MatButtonModule],
    template: `
        @if (errorMessage()) {
            <div class="error-boundary">
                <mat-icon class="error-icon">error_outline</mat-icon>
                <h3>Something went wrong</h3>
                <p class="error-detail">{{ errorMessage() }}</p>
                <button mat-stroked-button color="primary" (click)="retry.emit()">
                    <mat-icon>refresh</mat-icon>
                    Try Again
                </button>
            </div>
        } @else {
            <ng-content />
        }
    `,
    styles: [`
        .error-boundary {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 48px 24px;
            text-align: center;
            gap: 12px;
        }

        .error-icon {
            font-size: 48px;
            width: 48px;
            height: 48px;
            color: #f44336;
        }

        h3 {
            margin: 0;
            color: #333;
            font-size: 1.25rem;
        }

        .error-detail {
            color: #666;
            margin: 0 0 8px;
            max-width: 400px;
        }
    `],
})
export class ErrorBoundaryComponent {
    errorMessage = input<string | null>(null);
    retry = output<void>();
}
