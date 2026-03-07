import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';

export type ToastType = 'error' | 'success' | 'warning' | 'info';

@Injectable({ providedIn: 'root' })
export class ToastService {
    constructor(private snackBar: MatSnackBar) { }

    showError(message: string, duration = 5000): void {
        this.show(message, 'error', duration);
    }

    showSuccess(message: string, duration = 3000): void {
        this.show(message, 'success', duration);
    }

    showWarning(message: string, duration = 4000): void {
        this.show(message, 'warning', duration);
    }

    showInfo(message: string, duration = 3000): void {
        this.show(message, 'info', duration);
    }

    private show(message: string, type: ToastType, duration: number): void {
        const config: MatSnackBarConfig = {
            duration,
            horizontalPosition: 'end',
            verticalPosition: 'top',
            panelClass: [`toast-${type}`],
        };

        this.snackBar.open(message, 'Close', config);
    }
}
