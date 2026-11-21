import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { RoleService } from '../../core/services/role.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';
import { ToastService } from '../../core/services/toast.service';
import { RoleModel, CreateRoleModel } from '../../core/models/role.model';

@Component({
  selector: 'app-roles',
  standalone: false,
  templateUrl: './roles.html',
  styleUrls: ['./roles.css']
})
export class Roles implements OnInit {

  roles: RoleModel[] = [];

  roleForm: FormGroup;

  showForm = false;
  loading = false;
  errorMessage = '';

  editingId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private confirmDialog: ConfirmDialogService,
    private toastService: ToastService,
    private cdr: ChangeDetectorRef
  ) {

    this.roleForm = this.fb.group({
      roleName: ['', Validators.required],
      description: ['']
    });

  }

  ngOnInit(): void {

    this.loadRoles();

  }


  loadRoles(): void {

    this.loading = true;

    this.roleService.getAllRoles().subscribe({

      next: (response: RoleModel[]) => {

        this.roles = response;

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load roles.';

        this.cdr.detectChanges();

      }

    });

  }


  openCreateForm(): void {

    this.editingId = null;

    this.roleForm.reset();

    this.showForm = true;

  }


  editRole(role: RoleModel): void {

    this.editingId = role.roleID;

    this.roleForm.patchValue({

      roleName: role.roleName,
      description: role.description

    });

    this.showForm = true;

  }


  saveRole(): void {

    if (this.roleForm.invalid) {
      this.roleForm.markAllAsTouched();
      return;
    }

    const roleData: CreateRoleModel = {

      roleName: this.roleForm.value.roleName,
      description: this.roleForm.value.description

    };

    // UPDATE

    if (this.editingId !== null) {

      this.roleService
        .updateRole(this.editingId, roleData)
        .subscribe({

          next: () => {

            this.cancelForm();

            this.loadRoles();

          },

          error: (error) => {

            console.error('Update Role Error:', error);

            this.errorMessage = 'Unable to update role.';

            this.cdr.detectChanges();

          }

        });

      return;
    }

    // CREATE

    this.roleService
      .createRole(roleData)
      .subscribe({

        next: () => {

          this.cancelForm();

          this.loadRoles();

        },

        error: (error) => {

          console.error('Create Role Error:', error);

          this.errorMessage = 'Unable to create role.';

          this.cdr.detectChanges();

        }

      });

  }


  cancelForm(): void {

    this.showForm = false;

    this.editingId = null;

    this.roleForm.reset();

  }


  deleteRole(id: number): void {

    this.confirmDialog
      .confirm('Are you sure you want to delete this role?', { title: 'Delete Role' })
      .then((confirmed) => {

        if (!confirmed) return;

        this.roleService
          .deleteRole(id)
          .subscribe({

            next: () => {

              // Local removal — backend uses soft delete.
              this.roles = this.roles.filter(r => r.roleID !== id);

              this.toastService.success('Role deleted successfully.');

              this.cdr.detectChanges();

            },

            error: (error) => {

              console.error('Delete Role Error:', error);

              this.errorMessage = error?.error?.Message || error?.error?.message || 'Unable to delete role.';

              this.toastService.error(this.errorMessage);

              this.cdr.detectChanges();

            }

          });

      });

  }

}
