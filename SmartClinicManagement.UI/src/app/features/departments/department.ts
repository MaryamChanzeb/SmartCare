import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { DepartmentService } from '../../core/services/department.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ToastService } from '../../core/services/toast.service';
import { DepartmentModel } from '../../core/models/department.model';

@Component({
  selector: 'app-department',
  standalone: false,
  templateUrl: './department.html',
  styleUrls: ['./department.css']
})
export class Department implements OnInit {

  departments: DepartmentModel[] = [];

  departmentForm: FormGroup;

  showForm = false;
  loading = false;
  errorMessage = '';

  editingId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private departmentService: DepartmentService,
    private confirmDialog: ConfirmDialogService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {

    this.departmentForm = this.fb.group({
      departmentName: ['', Validators.required],
      description: ['']
    });

  }

  ngOnInit(): void {

    console.log('Department Component Loaded');

    this.loadDepartments();

  }



  loadDepartments(): void {

    this.loading = true;

    this.departmentService.getAllDepartments().subscribe({

      next: (response: DepartmentModel[]) => {

        this.departments = response;

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load departments.';

        this.cdr.detectChanges();

      }

    });

  }





  // OPEN CREATE FORM


  openCreateForm(): void {

    this.editingId = null;

    this.departmentForm.reset();

    this.showForm = true;

  }


  // EDIT DEPARTMENT

  editDepartment(department: DepartmentModel): void {

    this.editingId = department.departmentID;

    this.departmentForm.patchValue({

      departmentName: department.departmentName,

      description: department.description

    });

    this.showForm = true;

  }


  saveDepartment(): void {

    console.log('Save button clicked');
    console.log(this.departmentForm.value);

    if (this.departmentForm.invalid) {
      this.departmentForm.markAllAsTouched();
      return;
    }

    const departmentData: DepartmentModel = {

      departmentID: this.editingId ?? 0,

      departmentName:
        this.departmentForm.value.departmentName,

      description:
        this.departmentForm.value.description

    };

    // UPDATE

    if (this.editingId !== null) {

      this.departmentService
        .updateDepartment(
          this.editingId,
          departmentData
        )
        .subscribe({

          next: () => {

            this.cancelForm();

            this.loadDepartments();

          },

          error: (error) => {

            console.error(
              'Update Department Error:',
              error
            );

            this.errorMessage =
              'Unable to update department.';

          }

        });

      return;
    }

    // CREATE

    this.departmentService
      .createDepartment(departmentData)
      .subscribe({

        next: () => {

          this.cancelForm();

          this.loadDepartments();

        },

        error: (error) => {

          console.error(
            'Create Department Error:',
            error
          );

          this.errorMessage =
            'Unable to create department.';

        }

      });

  }


  cancelForm(): void {

    this.showForm = false;

    this.editingId = null;

    this.departmentForm.reset();

  }


  deleteDepartment(id: number): void {

    this.confirmDialog
      .confirm('Are you sure you want to delete this department?', { title: 'Delete Department' })
      .then((confirmed) => {

        if (!confirmed) return;

        this.departmentService
          .deleteDepartment(id)
          .subscribe({

            next: () => {

              // Removed locally rather than re-fetched from the server —
              // the backend soft-deletes departments (marks them inactive
              // rather than removing the row), so a reload would still
              // include the just-deleted record until the backend list
              // query itself filters those out. Filtering it out of the
              // in-memory list is what makes it disappear immediately.
              this.departments = this.departments.filter(
                department => department.departmentID !== id
              );

              this.toastService.success('Department deleted successfully.');

              this.cdr.detectChanges();

            },

            error: (error) => {

              console.error(
                'Delete Department Error:',
                error
              );

              this.errorMessage =
                error?.error?.Message || error?.error?.message || 'Unable to delete department.';

              this.toastService.error(this.errorMessage);

              this.cdr.detectChanges();

            }

          });

      });

  }

}
