import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';

import { PatientService } from '../../core/services/patient.service';
import { DoctorService } from '../../core/services/doctor.service';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';

import { RegisterPatientModel } from '../../core/models/patient.model';
import { DoctorModel } from '../../core/models/doctor.model';
import { UserModel } from '../../core/models/user.model';

@Component({
  selector: 'app-patient-registration',
  standalone: false,
  templateUrl: './patient-registration.html',
  styleUrls: ['./patient-registration.css']
})
export class PatientRegistration implements OnInit {

  registrationForm: FormGroup;

  saving = false;
  errorMessage = '';
  successMessage = '';

  doctors: DoctorModel[] = [];
  users: UserModel[] = [];

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService,
    private doctorService: DoctorService,
    private userService: UserService,
    private authService: AuthService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {

    this.registrationForm = this.fb.group({

      // Person / account info (goes into Users)
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      gender: [''],
      dateOfBirth: [''],

      // Doctor assignment
      doctorID: [null],

      // Patient-specific info (goes into Patients)
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

    const role = this.authService.getRole();

    if (role === 'Admin' || role === 'SuperAdmin') {

      // Admin manages/views patients but does not register new ones —
      // that's a Receptionist task.
      this.router.navigate(['/patients']);

      return;

    }

    this.loadDoctors();
    this.loadUsers();

  }


  loadDoctors(): void {

    this.doctorService.getAllDoctors().subscribe({

      next: (response) => {
        this.doctors = response;
        this.cdr.detectChanges();
      },

      error: (err) => console.error(err)

    });

  }


  loadUsers(): void {

    this.userService.getAllUsers().subscribe({

      next: (response) => {
        this.users = response;
        this.cdr.detectChanges();
      },

      error: (err) => console.error(err)

    });

  }


  // Doctor list only stores UserID — look up the display name locally.
  getDoctorName(doctor: DoctorModel): string {

    const user = this.users.find(u => u.userID === doctor.userID);

    return user
      ? `Dr. ${user.firstName} ${user.lastName} (${doctor.specialization})`
      : `Dr. #${doctor.doctorID}`;

  }


  register(): void {

    if (this.registrationForm.invalid) {
      this.registrationForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    const formValue = this.registrationForm.value;

    // Single combined call — creates the User account and Patient profile
    // together on the backend. No existing-user dropdown is involved.
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

    this.patientService.registerPatient(registrationData).subscribe({

      next: () => {

        this.saving = false;

        this.successMessage =
          `${formValue.firstName} ${formValue.lastName} has been registered as a patient. ` +
          `A welcome email with their login credentials has been sent to ${formValue.email}.`;

        this.registrationForm.reset();

        this.cdr.detectChanges();

      },

      error: (error) => {

        console.error('Register Patient Error:', error);

        this.saving = false;

        this.errorMessage =
          error?.error?.Message ||
          error?.error?.message ||
          'Unable to register the patient. The email may already be in use.';

        this.cdr.detectChanges();

      }

    });

  }


  goToPatients(): void {

    this.router.navigate(['/patients']);

  }

}
