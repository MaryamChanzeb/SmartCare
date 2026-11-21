import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { AuthService } from '../../core/services/auth.service';
import { UserService } from '../../core/services/user.service';
import { DoctorService } from '../../core/services/doctor.service';
import { CurrentUserService } from '../../core/services/current-user.service';
import { DepartmentService } from '../../core/services/department.service';
import { RoleService } from '../../core/services/role.service';

import { UserModel, UpdateUserModel } from '../../core/models/user.model';
import { CreateDoctorModel } from '../../core/models/doctor.model';
import { DepartmentModel } from '../../core/models/department.model';
import { RoleModel } from '../../core/models/role.model';

@Component({
  selector: 'app-profile',
  standalone: false,
  templateUrl: './profile.html',
  styleUrls: ['./profile.css']
})
export class Profile implements OnInit {

  user: UserModel | null = null;

  loading = true;
  saving = false;
  editMode = false;
  errorMessage = '';
  successMessage = '';

  profileForm: FormGroup;

  // ---- Doctor-only: professional information ----
  doctorEditMode = false;
  doctorSaving = false;
  doctorSuccessMessage = '';
  doctorErrorMessage = '';
  doctorForm: FormGroup;

  private departments: DepartmentModel[] = [];
  private roles: RoleModel[] = [];

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private userService: UserService,
    private doctorService: DoctorService,
    private currentUserService: CurrentUserService,
    private departmentService: DepartmentService,
    private roleService: RoleService,
    private cdr: ChangeDetectorRef
  ) {

    this.profileForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      phone: [''],
      gender: [''],
      dateOfBirth: ['']
    });

    this.doctorForm = this.fb.group({
      specialization: ['', Validators.required],
      qualification: [''],
      experienceYears: [0, Validators.min(0)],
      consultationFee: [0, Validators.min(0)],
      roomNumber: [''],
      // Backend has no dedicated Availability/schedule table — this reuses
      // the Doctor entity's existing Status field (Active/Inactive) as the
      // closest real concept of "currently available for appointments."
      status: ['Active']
    });

  }

  ngOnInit(): void {

    this.departmentService.getAllDepartments().subscribe({
      next: (response) => this.departments = response,
      error: (err) => console.error(err)
    });

    this.roleService.getAllRoles().subscribe({
      next: (response) => this.roles = response,
      error: (err) => console.error(err)
    });

    this.currentUserService.ensureLoaded().subscribe(() => {

      this.user = this.currentUserService.currentUser;

      this.loading = false;

      if (this.user) {

        this.profileForm.patchValue({

          firstName: this.user.firstName,
          lastName: this.user.lastName,
          phone: this.user.phone,
          gender: this.user.gender,
          dateOfBirth: this.user.dateOfBirth ? this.user.dateOfBirth.substring(0, 10) : ''

        });

      } else {

        this.errorMessage = 'Unable to load your profile.';

      }

      if (this.doctorInfo) {

        this.doctorForm.patchValue({

          specialization: this.doctorInfo.specialization,
          qualification: this.doctorInfo.qualification,
          experienceYears: this.doctorInfo.experienceYears,
          consultationFee: this.doctorInfo.consultationFee,
          roomNumber: this.doctorInfo.roomNumber,
          status: this.doctorInfo.status

        });

      }

      this.cdr.detectChanges();

    });

  }


  get doctorInfo() {
    return this.currentUserService.currentDoctor;
  }


  get patientInfo() {
    return this.currentUserService.currentPatient;
  }


  // ---- Personal info editing (unchanged from before) ----

  startEdit(): void {

    this.editMode = true;
    this.successMessage = '';

  }


  cancelEdit(): void {

    this.editMode = false;

    if (this.user) {

      this.profileForm.patchValue({

        firstName: this.user.firstName,
        lastName: this.user.lastName,
        phone: this.user.phone,
        gender: this.user.gender,
        dateOfBirth: this.user.dateOfBirth ? this.user.dateOfBirth.substring(0, 10) : ''

      });

    }

  }


  saveProfile(): void {

    if (!this.user) return;

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.saving = true;

    const formValue = this.profileForm.value;

    // Preserve email/department/role/status exactly as they are —
    // the backend's UpdateUserDto is a full replace, so those fields
    // must be sent back untouched rather than left blank/null.
    const currentDept = this.departments.find(d => d.departmentName === this.user!.departmentName);
    const currentRole = this.roles.find(r => r.roleName === this.user!.roleName);

    const updateData: UpdateUserModel = {

      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: this.user.email,
      phone: formValue.phone,
      gender: formValue.gender,
      dateOfBirth: formValue.dateOfBirth,
      departmentID: currentDept ? currentDept.departmentID : null,
      roleID: currentRole ? currentRole.roleID : null,
      status: this.user.status

    };

    this.userService.updateUser(this.user.userID, updateData).subscribe({

      next: () => {

        this.saving = false;
        this.editMode = false;
        this.successMessage = 'Profile updated successfully.';

        if (this.user) {

          this.user.firstName = formValue.firstName;
          this.user.lastName = formValue.lastName;
          this.user.phone = formValue.phone;
          this.user.gender = formValue.gender;
          this.user.dateOfBirth = formValue.dateOfBirth;

          localStorage.setItem('fullName', `${formValue.firstName} ${formValue.lastName}`);

        }

        this.cdr.detectChanges();

      },

      error: (error) => {

        console.error('Update Profile Error:', error);

        this.saving = false;
        this.errorMessage = 'Unable to update profile.';

        this.cdr.detectChanges();

      }

    });

  }


  // ---- Doctor professional info editing ----

  startDoctorEdit(): void {

    this.doctorEditMode = true;
    this.doctorSuccessMessage = '';

  }


  cancelDoctorEdit(): void {

    this.doctorEditMode = false;

    if (this.doctorInfo) {

      this.doctorForm.patchValue({

        specialization: this.doctorInfo.specialization,
        qualification: this.doctorInfo.qualification,
        experienceYears: this.doctorInfo.experienceYears,
        consultationFee: this.doctorInfo.consultationFee,
        roomNumber: this.doctorInfo.roomNumber,
        status: this.doctorInfo.status

      });

    }

  }


  saveDoctorInfo(): void {

    if (!this.doctorInfo) return;

    if (this.doctorForm.invalid) {
      this.doctorForm.markAllAsTouched();
      return;
    }

    this.doctorSaving = true;

    const formValue = this.doctorForm.value;

    // CreateDoctorModel is also used for update — the backend's
    // UpdateDoctorDto has the same shape as CreateDoctorDto. UserID and
    // LicenseNumber are preserved unchanged (not exposed for self-edit —
    // license numbers shouldn't be casually editable by the doctor).
    const doctorData: CreateDoctorModel = {

      userID: this.doctorInfo.userID,
      licenseNumber: this.doctorInfo.licenseNumber,
      specialization: formValue.specialization,
      qualification: formValue.qualification,
      experienceYears: formValue.experienceYears,
      consultationFee: formValue.consultationFee,
      roomNumber: formValue.roomNumber,
      joiningDate: this.doctorInfo.joiningDate,
      status: formValue.status

    };

    this.doctorService.updateDoctor(this.doctorInfo.doctorID, doctorData).subscribe({

      next: () => {

        this.doctorSaving = false;
        this.doctorEditMode = false;
        this.doctorSuccessMessage = 'Professional information updated successfully.';

        if (this.currentUserService.currentDoctor) {

          this.currentUserService.currentDoctor.specialization = formValue.specialization;
          this.currentUserService.currentDoctor.qualification = formValue.qualification;
          this.currentUserService.currentDoctor.experienceYears = formValue.experienceYears;
          this.currentUserService.currentDoctor.consultationFee = formValue.consultationFee;
          this.currentUserService.currentDoctor.roomNumber = formValue.roomNumber;
          this.currentUserService.currentDoctor.status = formValue.status;

        }

        this.cdr.detectChanges();

      },

      error: (error) => {

        console.error('Update Doctor Info Error:', error);

        this.doctorSaving = false;
        this.doctorErrorMessage = 'Unable to update professional information.';

        this.cdr.detectChanges();

      }

    });

  }

}
