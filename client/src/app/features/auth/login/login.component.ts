import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
    selector: 'app-login',
    standalone: true,
    imports: [
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
        MatProgressSpinnerModule,
    ],
    templateUrl: './login.component.html',
    styleUrl: './login.component.css',
})
export class LoginComponent {
    form: FormGroup;
    loading = false;
    error = '';
    hidePassword = true;

    constructor(
        private fb: FormBuilder,
        private auth: AuthService,
        private router: Router,
        private route: ActivatedRoute,
    ) {
        this.form = this.fb.group({
            email: ['', [Validators.required, Validators.email]],
            password: ['', [Validators.required]],
        });
    }

    async submit(): Promise<void> {
        if (this.form.invalid) return;
        this.loading = true;
        this.error = '';

        try {
            await this.auth.login(this.form.value);
            const returnUrl = this.route.snapshot.queryParams['returnUrl'] ?? '/dashboard';
            this.router.navigateByUrl(returnUrl);
        } catch {
            this.error = 'Invalid email or password. Please try again.';
        } finally {
            this.loading = false;
        }
    }

    get emailCtrl() { return this.form.get('email')!; }
    get passwordCtrl() { return this.form.get('password')!; }
}
