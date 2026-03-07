import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

/**
 * Reusable loading indicator component that shows a Material spinner
 * when the loading input is true, or renders the projected content otherwise.
 *
 * Usage:
 * ```html
 * <app-loading [loading]="isLoading()" [message]="'Loading goals...'">
 *   <div>Your content here</div>
 * </app-loading>
 * ```
 */
@Component({
    selector: 'app-loading',
    standalone: true,
    imports: [CommonModule, MatProgressSpinnerModule],
    template: `
        @if (loading()) {
            <div class="loading-container">
                <mat-spinner [diameter]="diameter()"></mat-spinner>
                @if (message()) {
                    <p class="loading-message">{{ message() }}</p>
                }
            </div>
        } @else {
            <ng-content />
        }
    `,
    styles: [`
        .loading-container {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 48px 24px;
            gap: 16px;
        }

        .loading-message {
            color: #666;
            margin: 0;
            font-size: 0.9rem;
        }
    `],
})
export class LoadingComponent {
    loading = input<boolean>(false);
    message = input<string | null>(null);
    diameter = input<number>(40);
}
