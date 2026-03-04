import { Component, OnInit } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatDialog } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatDialogModule } from '@angular/material/dialog';
import { PageHeaderComponent } from '../../shared/components/page-header.component';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog.component';
import { HasPermissionDirective } from '../../core/permissions/has-permission.directive';
import { DepartmentApiService } from '../../core/api/department-api.service';
import { Organization } from '../../core/api/api.models';

@Component({
    selector: 'app-organizations',
    standalone: true,
    imports: [
        ReactiveFormsModule,
        MatTableModule, MatButtonModule, MatIconModule,
        MatProgressSpinnerModule, MatPaginatorModule,
        MatFormFieldModule, MatInputModule, MatDialogModule,
        PageHeaderComponent, HasPermissionDirective,
    ],
    templateUrl: './organizations.component.html',
    styleUrl: './organizations.component.css',
})
export class OrganizationsComponent implements OnInit {
    orgs: Organization[] = [];
    totalCount = 0;
    pageSize = 10;
    pageNumber = 1;
    loading = true;
    showForm = false;
    editing: Organization | null = null;

    displayedColumns = ['name', 'description', 'actions'];

    form: FormGroup;

    constructor(private deptApi: DepartmentApiService, private dialog: MatDialog, private fb: FormBuilder) {
        this.form = this.fb.group({ name: ['', Validators.required], description: [''] });
    }

    ngOnInit(): void { this.load(); }

    load(): void {
        this.loading = true;
        this.deptApi.getOrganizations({ page: this.pageNumber, size: this.pageSize }).subscribe({
            next: res => { this.orgs = res.items ?? []; this.totalCount = res.totalCount ?? 0; this.loading = false; },
            error: () => { this.loading = false; }
        });
    }

    onPage(e: PageEvent): void { this.pageNumber = e.pageIndex + 1; this.pageSize = e.pageSize; this.load(); }

    openCreate(): void { this.editing = null; this.form.reset(); this.showForm = true; }
    openEdit(org: Organization): void { this.editing = org; this.form.patchValue(org); this.showForm = true; }
    cancelForm(): void { this.showForm = false; this.editing = null; }

    save(): void {
        if (this.form.invalid) return;
        const req = this.form.value;
        const op = this.editing
            ? this.deptApi.updateOrganization(this.editing.id, req)
            : this.deptApi.createOrganization(req);
        op.subscribe(() => { this.showForm = false; this.load(); });
    }

    delete(org: Organization): void {
        const ref = this.dialog.open(ConfirmDialogComponent, {
            data: { title: 'Delete Organization', message: `Delete "${org.name}"?`, danger: true, confirmLabel: 'Delete' }
        });
        ref.afterClosed().subscribe(ok => { if (ok) this.deptApi.deleteOrganization(org.id).subscribe(() => this.load()); });
    }
}
