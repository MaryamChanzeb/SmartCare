import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { DoctorService } from '../../core/services/doctor.service';
import { UserService } from '../../core/services/user.service';

import { DoctorModel, CreateDoctorModel, RegisterDoctorModel } from '../../core/models/doctor.model';
import { UserModel } from '../../core/models/user.model';

@Component({
  selector: 'app-doctors',
  standalone: false,
  templateUrl: './doctors.html',
  styleUrls: ['./doctors.css']
})
export class Doctors implements OnInit {

  doctors: DoctorModel[] = [];
  users: UserModel[] = [];


  availableUsers: UserModel[] = [];

  doctorForm: FormGroup;

  showForm = false;
  loading = false;
  errorMessage = '';

  editingId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private doctorService: DoctorService,
    private userService: UserService,
    private cdr: ChangeDetectorRef
  ) {

    this.doctorForm = this.fb.group({
      // Account fields — only used when creating a new doctor. On edit
      // they're disabled/ignored, since the linked user already exists.
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      gender: [''],
      dateOfBirth: [''],
      licenseNumber: ['', Validators.required],
      specialization: ['', Validators.required],
      qualification: [''],
      experienceYears: [0],
      consultationFee: [0],
      roomNumber: [''],
      joiningDate: [''],
      status: ['Active']
    });

  }

  ngOnInit(): void {

    this.loadDoctors();
    this.loadUsers();

  }


  loadDoctors(): void {

    this.loading = true;

    this.doctorService.getAllDoctors().subscribe({

      next: (response: DoctorModel[]) => {

        this.doctors = response;

        this.loading = false;

        this.refreshAvailableUsers();

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load doctors.';

        this.cdr.detectChanges();

      }

    });

  }


  loadUsers(): void {

    this.userService.getAllUsers().subscribe({

      next: (response: UserModel[]) => {

        this.users = response;

        this.refreshAvailableUsers();

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  private refreshAvailableUsers(): void {

    const assignedUserIds = new Set(
      this.doctors
        .filter(d => d.doctorID !== this.editingId)
        .map(d => d.userID)
    );

    this.availableUsers = this.users.filter(
      u => !assignedUserIds.has(u.userID)
    );

  }


  // Doctor list only stores UserID — look up the display name locally.

  getUserName(userID: number): string {

    const match = this.users.find(u => u.userID === userID);

    return match ? `${match.firstName} ${match.lastName}` : `User #${userID}`;

  }


  openCreateForm(): void {

    this.editingId = null;

    this.doctorForm.reset({
      experienceYears: 0,
      consultationFee: 0,
      status: 'Active'
    });

    // Creating a doctor also creates their user account, so the account
    // fields are required here.
    this.setAccountFieldsRequired(true);

    this.showForm = true;

  }


  // The account fields belong to the linked User, which already exists
  // when editing — so they're only validated/sent on create.
  private setAccountFieldsRequired(required: boolean): void {

    const accountControls: Array<[string, any[]]> = [
      ['firstName', [Validators.required]],
      ['lastName', [Validators.required]],
      ['email', [Validators.required, Validators.email]]
    ];

    accountControls.forEach(([name, validators]) => {

      const control = this.doctorForm.get(name);

      if (!control) return;

      control.setValidators(required ? validators : null);
      control.updateValueAndValidity({ emitEvent: false });

    });

  }


  editDoctor(doctor: DoctorModel): void {

    this.editingId = doctor.doctorID;

    // Editing only changes professional details — the linked user account
    // is managed from User Management, so account fields aren't required.
    this.setAccountFieldsRequired(false);

    this.doctorForm.patchValue({

      licenseNumber: doctor.licenseNumber,
      specialization: doctor.specialization,
      qualification: doctor.qualification,
      experienceYears: doctor.experienceYears,
      consultationFee: doctor.consultationFee,
      roomNumber: doctor.roomNumber,
      joiningDate: doctor.joiningDate ? doctor.joiningDate.substring(0, 10) : '',
      status: doctor.status

    });

    this.showForm = true;

  }


  saveDoctor(): void {

    if (this.doctorForm.invalid) {
      this.doctorForm.markAllAsTouched();
      return;
    }

    const formValue = this.doctorForm.value;

    // UPDATE — professional details only; the linked user is untouched.

    if (this.editingId !== null) {

      const current = this.doctors.find(d => d.doctorID === this.editingId);

      const doctorData: CreateDoctorModel = {

        userID: current ? current.userID : null,
        licenseNumber: formValue.licenseNumber,
        specialization: formValue.specialization,
        qualification: formValue.qualification,
        experienceYears: formValue.experienceYears,
        consultationFee: formValue.consultationFee,
        roomNumber: formValue.roomNumber,
        joiningDate: formValue.joiningDate || undefined,
        status: formValue.status

      };

      this.doctorService
        .updateDoctor(this.editingId, doctorData)
        .subscribe({

          next: () => {

            this.cancelForm();

            this.loadDoctors();

          },

          error: (error) => {

            console.error('Update Doctor Error:', error);

            this.errorMessage = error?.error?.Message || error?.error?.message || 'Unable to update doctor.';

            this.cdr.detectChanges();

          }

        });

      return;
    }

    // CREATE — one combined call creates the User account AND the Doctor
    // profile, so no existing user has to be selected.

    const registrationData: RegisterDoctorModel = {

      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      phone: formValue.phone,
      gender: formValue.gender,
      dateOfBirth: formValue.dateOfBirth || undefined,
      departmentID: null,
      licenseNumber: formValue.licenseNumber,
      specialization: formValue.specialization,
      qualification: formValue.qualification,
      experienceYears: formValue.experienceYears,
      consultationFee: formValue.consultationFee,
      roomNumber: formValue.roomNumber,
      joiningDate: formValue.joiningDate || undefined,
      status: formValue.status

    };

    this.doctorService
      .registerDoctor(registrationData)
      .subscribe({

        next: () => {

          this.cancelForm();

          this.loadDoctors();

        },

        error: (error) => {

          console.error('Register Doctor Error:', error);

          this.errorMessage =
            error?.error?.Message ||
            error?.error?.message ||
            'Unable to create doctor. The email or license number may already be in use.';

          this.cdr.detectChanges();

        }

      });

  }


  cancelForm(): void {

    this.showForm = false;

    this.editingId = null;

    this.refreshAvailableUsers();

    this.doctorForm.reset({
      experienceYears: 0,
      consultationFee: 0,
      status: 'Active'
    });

  }


  deleteDoctor(id: number): void {

    if (!confirm('Are you sure you want to delete this doctor?')) {
      return;
    }

    this.doctorService
      .deleteDoctor(id)
      .subscribe({

        next: () => {

          // Local removal — backend uses soft delete.
          this.doctors = this.doctors.filter(d => d.doctorID !== id);

          this.refreshAvailableUsers();

          this.cdr.detectChanges();

        },

        error: (error) => {

          console.error('Delete Doctor Error:', error);

          this.errorMessage = error?.error?.Message || error?.error?.message || 'Unable to delete doctor.';

          this.cdr.detectChanges();

        }

      });

  }

}
