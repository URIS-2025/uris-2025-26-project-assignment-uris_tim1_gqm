import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { Role, Organization, Department } from '../../../core/api/api.models';
import { UserApiService } from '../../../core/api/user-api.service';
import { DepartmentApiService } from '../../../core/api/department-api.service';
import { AuthService } from '../../../core/auth/auth.service';
import { catchError, finalize, of } from 'rxjs';

function gqmEmailValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) return null;
    return control.value.endsWith('@gqmplus.com') ? null : { invalidDomain: true };
}

@Component({
    selector: 'app-user-dialog',
    standalone: true,
    imports: [
        CommonModule,
        ReactiveFormsModule,
        MatDialogModule,
        MatButtonModule,
        MatFormFieldModule,
        MatInputModule,
        MatSelectModule,
        MatProgressSpinnerModule
    ],
    styles: [`
    .dialog-container { padding: 8px; }
    mat-form-field { width: 100%; margin-bottom: 8px; }
    .form-row { display: flex; gap: 16px; }
    .form-row > * { flex: 1; }
  `],
    template: `
    <h2 mat-dialog-title>Add New User</h2>
    
    <mat-dialog-content class="dialog-container">
      @if (initialLoading()) {
        <div style="display: flex; justify-content: center; padding: 24px;">
          <mat-spinner diameter="32"></mat-spinner>
        </div>
      } @else {
        <form [formGroup]="userForm" (ngSubmit)="onSubmit()">
          
          <div class="form-row">
            <mat-form-field appearance="outline">
              <mat-label>First Name</mat-label>
              <input matInput formControlName="firstName" required>
            </mat-form-field>
            
            <mat-form-field appearance="outline">
              <mat-label>Last Name</mat-label>
              <input matInput formControlName="lastName" required>
            </mat-form-field>
          </div>

          <mat-form-field appearance="outline">
            <mat-label>Email (@gqmplus.com)</mat-label>
            <input matInput formControlName="email" type="email" placeholder="example@gqmplus.com" required>
            @if (userForm.get('email')?.hasError('invalidDomain') && userForm.get('email')?.touched) {
              <mat-error>Email must end with <strong>&#64;gqmplus.com</strong></mat-error>
            }
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Password</mat-label>
            <input matInput formControlName="password" required>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Organization</mat-label>
            <mat-select formControlName="organizationId" required>
              @for (org of organizations(); track org.id) {
                <mat-option [value]="org.id">{{ org.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          <mat-form-field appearance="outline">
            <mat-label>Role</mat-label>
            <mat-select formControlName="roleId" required>
              @for (role of roles(); track role.id) {
                <mat-option [value]="role.id">{{ role.name }}</mat-option>
              }
            </mat-select>
          </mat-form-field>

          @if (isDepartmentManagerSelected()) {
            <mat-form-field appearance="outline">
              <mat-label>Managed Department</mat-label>
              <mat-select formControlName="departmentId" required>
                @if (departmentsLoading()) {
                  <mat-option disabled>Loading...</mat-option>
                } @else if (departments().length === 0) {
                  <mat-option disabled>No departments in selected org.</mat-option>
                } @else {
                  @for (dept of departments(); track dept.id) {
                    <mat-option [value]="dept.id">{{ dept.name }}</mat-option>
                  }
                }
              </mat-select>
            </mat-form-field>
          }

        </form>
      }
    </mat-dialog-content>
    
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close [disabled]="submitting()">Cancel</button>
      <button mat-flat-button color="primary" [disabled]="!userForm.valid || submitting() || initialLoading()" (click)="onSubmit()">
        @if (submitting()) {
          <mat-spinner diameter="20" style="display:inline-block; margin-right: 8px;"></mat-spinner>
        }
        Create User
      </button>
    </mat-dialog-actions>
  `
})
export class UserDialogComponent implements OnInit {
    private fb = inject(FormBuilder);
    private dialogRef = inject(MatDialogRef<UserDialogComponent>);
    private userService = inject(UserApiService);
    private departmentService = inject(DepartmentApiService);
    private authService = inject(AuthService);

    userForm: FormGroup;

    initialLoading = signal<boolean>(true);
    submitting = signal<boolean>(false);
    departmentsLoading = signal<boolean>(false);

    roles = signal<Role[]>([]);
    organizations = signal<Organization[]>([]);
    departments = signal<Department[]>([]);

    selectedRoleId = signal<string>('');

    isDepartmentManagerSelected = computed(() => {
        const roleId = this.selectedRoleId();
        const role = this.roles().find(r => r.id === roleId);
        if (!role) return false;

        // Return true if the role name implies department manager
        return role.name.toLowerCase().includes('department manager');
    });

    constructor() {
        this.userForm = this.fb.group({
            firstName: ['', Validators.required],
            lastName: ['', Validators.required],
            email: ['', [Validators.required, Validators.email, gqmEmailValidator]],
            password: ['Test@123', Validators.required],
            organizationId: ['', Validators.required],
            roleId: ['', Validators.required],
            departmentId: ['']
        });

        // Watch organization changes to fetch associated departments
        this.userForm.get('organizationId')?.valueChanges.subscribe(orgId => {
            this.userForm.get('departmentId')?.setValue('');
            if (orgId) {
                this.loadDepartments(orgId);
            } else {
                this.departments.set([]);
            }
        });

        // Track roleId changes using a signal for the computed property
        this.userForm.get('roleId')?.valueChanges.subscribe(roleId => {
            this.selectedRoleId.set(roleId);

            // Allow computed value to update, then apply validation
            setTimeout(() => {
                const deptControl = this.userForm.get('departmentId');
                if (this.isDepartmentManagerSelected()) {
                    deptControl?.setValidators(Validators.required);
                } else {
                    deptControl?.clearValidators();
                    deptControl?.setValue('');
                }
                deptControl?.updateValueAndValidity();
            });
        });
    }

    ngOnInit(): void {
        this.loadInitialData();
    }

    private loadInitialData() {
        // Fetch roles and organizations concurrently
        let rolesFetched = false;
        let orgsFetched = false;

        const checkLoading = () => { if (rolesFetched && orgsFetched) this.initialLoading.set(false); };

        this.userService.getRoles().pipe(
            catchError(() => of([]))
        ).subscribe(res => {
            this.roles.set(res);
            rolesFetched = true;
            checkLoading();
        });

        this.departmentService.getOrganizations({ size: 100 }).pipe(
            catchError(() => of({ items: [] }))
        ).subscribe(res => {
            let loadedOrgs = res.items || [];

            // Filter organizations based on current user's role
            const currentUser = this.authService.currentUser;
            if (currentUser) {
                const isSystemAdmin = currentUser.permissions?.includes('manage_organizations');
                if (!isSystemAdmin && currentUser.organizationId) {
                    loadedOrgs = loadedOrgs.filter(org => org.id === currentUser.organizationId);
                }
            }

            this.organizations.set(loadedOrgs);

            // If there's only one organization available (e.g., Org Admin), select it automatically
            if (loadedOrgs.length === 1) {
                this.userForm.patchValue({ organizationId: loadedOrgs[0].id });
            }

            orgsFetched = true;
            checkLoading();
        });
    }

    private loadDepartments(orgId: string) {
        this.departmentsLoading.set(true);
        this.departmentService.getDepartmentsByOrg(orgId, { size: 100 }).pipe(
            catchError(() => of({ items: [] })),
            finalize(() => this.departmentsLoading.set(false))
        ).subscribe(res => {
            this.departments.set(res.items || []);
        });
    }

    onSubmit() {
        if (this.userForm.invalid || this.submitting()) return;

        this.submitting.set(true);
        const val = this.userForm.value;

        // Step 1: Create User
        this.userService.createUser({
            firstName: val.firstName,
            lastName: val.lastName,
            email: val.email,
            password: val.password
        }).subscribe({
            next: (createdUser) => {
                // Step 2: Assign Role
                this.userService.assignRole({
                    userId: createdUser.id,
                    roleId: val.roleId,
                    organizationId: val.organizationId
                }).subscribe({
                    next: () => {
                        // Step 3: Set Department Manager if applicable
                        if (this.isDepartmentManagerSelected() && val.departmentId) {

                            // To update department, we need to fetch it first to get its current fields.
                            this.departmentService.getDepartmentById(val.departmentId).subscribe(dept => {
                                this.departmentService.updateDepartment(val.departmentId, {
                                    name: dept.name,
                                    description: dept.description,
                                    organizationId: dept.organizationId,
                                    managerId: createdUser.id // <--- SET THE MANAGER
                                }).subscribe({
                                    next: () => this.dialogRef.close(true),
                                    error: () => this.dialogRef.close(true) // Still close if it partially succeeded
                                });
                            });
                        } else {
                            this.dialogRef.close(true); // Success without department assignment
                        }
                    },
                    error: (err) => {
                        console.error('Failed to assign role', err);
                        this.submitting.set(false);
                    }
                });
            },
            error: (err) => {
                console.error('Failed to create user', err);
                this.submitting.set(false);
            }
        });
    }
}
