import { Component, OnInit, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { DepartmentApiService } from '../../core/api/department-api.service';
import { AuthService } from '../../core/auth/auth.service';
import { Department, Organization } from '../../core/api/api.models';

@Component({
    selector: 'app-departments',
    standalone: true,
    imports: [
        ReactiveFormsModule, MatTableModule, MatButtonModule, MatIconModule,
        MatProgressSpinnerModule, MatPaginatorModule, MatFormFieldModule,
        MatInputModule, MatSelectModule, PageHeaderComponent, HasPermissionDirective,
    ],
    templateUrl: './departments.component.html',
    styleUrl: './departments.component.css',
})
export class DepartmentsComponent implements OnInit {
    departments: Department[] = [];
    organizations: Organization[] = [];
    totalCount = 0;
    pageSize = 10;
    pageNumber = 1;
    loading = true;
    showForm = false;
    editing: Department | null = null;

    displayedColumns = ['name', 'description', 'organization', 'actions'];
    form: FormGroup;
    private auth = inject(AuthService);
    private destroyRef = inject(DestroyRef);

    constructor(
        private deptApi: DepartmentApiService,
        private dialog: MatDialog,
        private fb: FormBuilder
    ) {
        this.form = this.fb.group({
            name: ['', Validators.required],
            description: [''],
        });
    }

    ngOnInit(): void {
        this.deptApi.getOrganizations({ page: 1, size: 100 }).subscribe({
            next: res => this.organizations = res.items ?? [],
            error: () => { }
        });

        this.auth.organizationId$.pipe(
            takeUntilDestroyed(this.destroyRef)
        ).subscribe(() => {
            this.pageNumber = 1;
            this.load();
        });
    }

    load(): void {
        this.loading = true;
        this.deptApi.getDepartments({ page: this.pageNumber, size: this.pageSize }).subscribe({
            next: res => { this.departments = res.items ?? []; this.totalCount = res.total ?? 0; this.loading = false; },
            error: () => { this.loading = false; }
        });
    }

    onPage(e: PageEvent): void { this.pageNumber = e.pageIndex + 1; this.pageSize = e.pageSize; this.load(); }

    openCreate(): void { this.editing = null; this.form.reset(); this.showForm = true; }
    openEdit(d: Department): void { this.editing = d; this.form.patchValue(d); this.showForm = true; }
    cancelForm(): void { this.showForm = false; this.editing = null; }

    save(): void {
        if (this.form.invalid) return;
        const req = this.form.value;
        req.organizationId = this.auth.organizationId; // auto-inject active org

        const op = this.editing
            ? this.deptApi.updateDepartment(this.editing.id, req)
            : this.deptApi.createDepartment(req);
        op.subscribe(() => { this.showForm = false; this.load(); });
    }

    getOrgName(orgId: string): string {
        return this.organizations.find(o => o.id === orgId)?.name ?? '—';
    }

    delete(d: Department): void {
        const ref = this.dialog.open(ConfirmDialogComponent, {
            data: { title: 'Delete Department', message: `Delete "${d.name}"?`, danger: true, confirmLabel: 'Delete' }
        });
        ref.afterClosed().subscribe(ok => { if (ok) this.deptApi.deleteDepartment(d.id).subscribe(() => this.load()); });
    }
}
