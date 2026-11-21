import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { UserService } from '../../core/services/user.service';
import { DepartmentService } from '../../core/services/department.service';
import { RoleService } from '../../core/services/role.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PatientService } from '../../core/services/patient.service';
import { ToastService } from '../../core/services/toast.service';
import { ConfirmDialogService } from '../../core/services/confirm-dialog.service';

import { UserModel, CreateUserModel, UpdateUserModel } from '../../core/models/user.model';
import { DepartmentModel } from '../../core/models/department.model';
import { RoleModel } from '../../core/models/role.model';
import { DoctorModel, RegisterDoctorModel } from '../../core/models/doctor.model';
import { RegisterPatientModel } from '../../core/models/patient.model';

// The three roles Admin can create from this page. Receptionist has no
// dedicated profile table (it's just a User + Role), so it reuses the
// existing generic user form below. Doctor and Patient each get their
// own form with the extra profile fields their registration endpoints
// require, mirroring the Doctors and Patient-Registration pages.
type CreateRole = 'Doctor' | 'Patient' | 'Receptionist';

@Component({
  selector: 'app-users',
  standalone: false,
  templateUrl: './users.html',
  styleUrls: ['./users.css']
})
export class Users implements OnInit {

  users: UserModel[] = [];
  departments: DepartmentModel[] = [];
  roles: RoleModel[] = [];
  doctors: DoctorModel[] = [];

  userForm: FormGroup;
  doctorCreateForm: FormGroup;
  patientCreateForm: FormGroup;

  // Create flow: Add User -> pick a role -> role-specific form.
  // (Editing an existing user still uses the plain userForm/showForm
  // below, unrelated to this role picker.)
  showRoleSelect = false;
  creatingRole: CreateRole | null = null;
  saving = false;

  showForm = false;
  loading = false;
  errorMessage = '';

  editingId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private departmentService: DepartmentService,
    private roleService: RoleService,
    private doctorService: DoctorService,
    private patientService: PatientService,
    private toastService: ToastService,
    private confirmDialog: ConfirmDialogService,
    private cdr: ChangeDetectorRef
  ) {

    this.userForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      gender: [''],
      dateOfBirth: [''],
      departmentID: [null],
      roleID: [null],
      status: ['Active']
    });

    this.doctorCreateForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      gender: [''],
      dateOfBirth: [''],
      departmentID: [null],
      licenseNumber: ['', Validators.required],
      specialization: ['', Validators.required],
      qualification: [''],
      experienceYears: [0],
      consultationFee: [0],
      roomNumber: [''],
      joiningDate: [''],
      status: ['Active']
    });

    this.patientCreateForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      gender: [''],
      dateOfBirth: [''],
      doctorID: [null],
      bloodGroup: [''],
      height: [null],
      weight: [null],
      emergencyContactName: [''],
      emergencyContactPhone: [''],
      allergies: [''],
      medicalHistory: ['']
    });

  }

  ngOnInit(): void {

    this.loadUsers();
    this.loadDepartments();
    this.loadRoles();
    this.loadDoctors();

  }


  loadUsers(): void {

    this.loading = true;

    this.userService.getAllUsers().subscribe({

      next: (response: UserModel[]) => {

        this.users = response;

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load users.';

        this.cdr.detectChanges();

      }

    });

  }


  loadDepartments(): void {

    this.departmentService.getAllDepartments().subscribe({

      next: (response: DepartmentModel[]) => {

        this.departments = response;

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  loadRoles(): void {

    this.roleService.getAllRoles().subscribe({

      next: (response: RoleModel[]) => {

        this.roles = response;

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  // Used by the Patient create form to let Admin optionally assign a
  // primary doctor — same list the Doctors/Patient-Registration pages use.
  loadDoctors(): void {

    this.doctorService.getAllDoctors().subscribe({

      next: (response: DoctorModel[]) => {

        this.doctors = response;

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  getDoctorName(doctor: DoctorModel): string {

    const user = this.users.find(u => u.userID === doctor.userID);

    return user
      ? `Dr. ${user.firstName} ${user.lastName} (${doctor.specialization})`
      : `Dr. #${doctor.doctorID}`;

  }


  // STEP 1 — "Add User" opens the role picker, not a form directly.

  openCreateForm(): void {

    this.editingId = null;
    this.errorMessage = '';
    this.showForm = false;
    this.creatingRole = null;
    this.showRoleSelect = true;

  }


  // STEP 2 — Admin picks which kind of account to create.

  selectCreateRole(role: CreateRole): void {

    this.creatingRole = role;
    this.showRoleSelect = false;
    this.errorMessage = '';

    if (role === 'Doctor') {
      this.doctorCreateForm.reset({ status: 'Active', experienceYears: 0, consultationFee: 0 });
    } else if (role === 'Patient') {
      this.patientCreateForm.reset();
    } else {
      // Receptionist reuses the plain user form; roleID is fixed to
      // "Receptionist" rather than left for Admin to pick, since the
      // role was already chosen in step 1.
      const receptionistRoleId = this.findRoleIdByName('Receptionist');

      this.userForm.reset({ status: 'Active', roleID: receptionistRoleId });

      if (!receptionistRoleId) {
        this.errorMessage = 'No "Receptionist" role was found. Set one up under Roles first.';
      }
    }

    this.showForm = true;

  }


  backToRoleSelect(): void {

    this.creatingRole = null;
    this.showForm = false;
    this.showRoleSelect = true;
    this.errorMessage = '';

  }


  // EDIT USER (unaffected by the role-first create flow above)

  editUser(user: UserModel): void {

    this.editingId = user.userID;
    this.creatingRole = null;
    this.showRoleSelect = false;

    this.userForm.patchValue({

      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      phone: user.phone,
      gender: user.gender,
      dateOfBirth: user.dateOfBirth ? user.dateOfBirth.substring(0, 10) : '',
      departmentID: this.findDepartmentIdByName(user.departmentName),
      roleID: this.findRoleIdByName(user.roleName),
      status: user.status

    });

    this.showForm = true;

  }


  private findDepartmentIdByName(name?: string): number | null {

    const match = this.departments.find(d => d.departmentName === name);

    return match ? match.departmentID : null;

  }


  private findRoleIdByName(name?: string): number | null {

    if (!name) return null;

    const match = this.roles.find(r => r.roleName?.toLowerCase() === name.toLowerCase());

    return match ? match.roleID : null;

  }


  // Edits (existing users) and Receptionist creation both submit through
  // the plain userForm/CreateUserModel|UpdateUserModel path.

  saveUser(): void {

    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    const formValue = this.userForm.value;

    // UPDATE

    if (this.editingId !== null) {

      const updateData: UpdateUserModel = {

        firstName: formValue.firstName,
        lastName: formValue.lastName,
        email: formValue.email,
        phone: formValue.phone,
        gender: formValue.gender,
        // An empty string here fails backend DateOnly? model binding
        // (it isn't valid JSON for a date), which silently broke Update
        // for any user with no birth date set. Send undefined instead so
        // it's omitted from the request body.
        dateOfBirth: formValue.dateOfBirth || undefined,
        departmentID: formValue.departmentID,
        roleID: formValue.roleID,
        status: formValue.status

      };

      this.saving = true;

      this.userService
        .updateUser(this.editingId, updateData)
        .subscribe({

          next: () => {

            this.saving = false;
            this.cancelForm();
            this.loadUsers();

          },

          error: (error) => {

            this.saving = false;
            console.error('Update User Error:', error);

            this.errorMessage = error?.error?.Message || error?.error?.message || 'Unable to update user.';

            this.cdr.detectChanges();

          }

        });

      return;
    }

    // CREATE (Receptionist — Doctor/Patient go through their own methods
    // below since they hit different registration endpoints)

    const createData: CreateUserModel = {

      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      phone: formValue.phone,
      gender: formValue.gender,
      dateOfBirth: formValue.dateOfBirth || undefined,
      departmentID: formValue.departmentID,
      roleID: formValue.roleID

    };

    this.saving = true;

    this.userService
      .createUser(createData)
      .subscribe({

        next: () => {

          this.saving = false;
          this.cancelForm();
          this.loadUsers();

          this.toastService.success(
            `Receptionist account created successfully. Login credentials have been emailed to ${createData.email}.`
          );

        },

        error: (error) => {

          this.saving = false;
          console.error('Create User Error:', error);

          this.errorMessage = error?.error?.Message || error?.error?.message || 'Unable to create user.';

          this.cdr.detectChanges();

        }

      });

  }


  // CREATE — Doctor. One combined call creates the User account AND the
  // Doctor profile, so the new doctor shows up on the Doctors page too.

  saveDoctor(): void {

    if (this.doctorCreateForm.invalid) {
      this.doctorCreateForm.markAllAsTouched();
      return;
    }

    const formValue = this.doctorCreateForm.value;

    const registrationData: RegisterDoctorModel = {

      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      phone: formValue.phone,
      gender: formValue.gender,
      dateOfBirth: formValue.dateOfBirth || undefined,
      departmentID: formValue.departmentID,
      licenseNumber: formValue.licenseNumber,
      specialization: formValue.specialization,
      qualification: formValue.qualification,
      experienceYears: formValue.experienceYears,
      consultationFee: formValue.consultationFee,
      roomNumber: formValue.roomNumber,
      joiningDate: formValue.joiningDate || undefined,
      status: formValue.status

    };

    this.saving = true;

    this.doctorService
      .registerDoctor(registrationData)
      .subscribe({

        next: () => {

          this.saving = false;
          this.cancelForm();
          this.loadUsers();

          this.toastService.success(
            `Doctor account created successfully. Login credentials have been emailed to ${registrationData.email}.`
          );

        },

        error: (error) => {

          this.saving = false;
          console.error('Register Doctor Error:', error);

          this.errorMessage =
            error?.error?.Message ||
            error?.error?.message ||
            'Unable to create doctor. The email or license number may already be in use.';

          this.cdr.detectChanges();

        }

      });

  }


  // CREATE — Patient. One combined call creates the User account AND the
  // Patient profile, so the new patient shows up on the Patients page too.

  savePatient(): void {

    if (this.patientCreateForm.invalid) {
      this.patientCreateForm.markAllAsTouched();
      return;
    }

    const formValue = this.patientCreateForm.value;

    const registrationData: RegisterPatientModel = {

      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      phone: formValue.phone,
      gender: formValue.gender,
      dateOfBirth: formValue.dateOfBirth || undefined,
      doctorID: formValue.doctorID,
      bloodGroup: formValue.bloodGroup,
      height: formValue.height,
      weight: formValue.weight,
      emergencyContactName: formValue.emergencyContactName,
      emergencyContactPhone: formValue.emergencyContactPhone,
      allergies: formValue.allergies,
      medicalHistory: formValue.medicalHistory,
      status: 'Active'

    };

    this.saving = true;

    this.patientService
      .registerPatient(registrationData)
      .subscribe({

        next: () => {

          this.saving = false;
          this.cancelForm();
          this.loadUsers();

          this.toastService.success(
            `Patient account created successfully. Login credentials have been emailed to ${registrationData.email}.`
          );

        },

        error: (error) => {

          this.saving = false;
          console.error('Register Patient Error:', error);

          this.errorMessage =
            error?.error?.Message ||
            error?.error?.message ||
            'Unable to create patient.';

          this.cdr.detectChanges();

        }

      });

  }


  cancelForm(): void {

    this.showForm = false;
    this.showRoleSelect = false;
    this.creatingRole = null;
    this.editingId = null;

    this.userForm.reset({ status: 'Active' });
    this.doctorCreateForm.reset({ status: 'Active', experienceYears: 0, consultationFee: 0 });
    this.patientCreateForm.reset();

  }


  deleteUser(id: number): void {

    this.confirmDialog
      .confirm('Are you sure you want to delete this user?', { title: 'Delete User' })
      .then((confirmed) => {

        if (!confirmed) return;

        this.userService
          .deleteUser(id)
          .subscribe({

            next: () => {

              // Removed locally instead of reloading from the server.
              // The backend soft-deletes users (marks them inactive
              // rather than removing the row), and its GetAll query still
              // returns soft-deleted rows — so calling loadUsers() here
              // re-fetched the exact same list and the "deleted" user
              // reappeared until a full page refresh. Filtering it out
              // of the in-memory list instead makes it disappear
              // immediately and keeps this consistent with how
              // Departments/Roles/Doctors already handle their deletes.
              this.users = this.users.filter(u => u.userID !== id);

              this.toastService.success('User deleted successfully.');

              this.cdr.detectChanges();

            },

            error: (error) => {

              console.error('Delete User Error:', error);

              this.errorMessage = error?.error?.Message || error?.error?.message || 'Unable to delete user.';

              this.toastService.error(this.errorMessage);

              this.cdr.detectChanges();

            }

          });

      });

  }

}
