import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { PatientService } from '../../core/services/patient.service';
import { UserService } from '../../core/services/user.service';
import { DoctorService } from '../../core/services/doctor.service';
import { AppointmentService } from '../../core/services/appointment.service';
import { AuthService } from '../../core/services/auth.service';
import { CurrentUserService } from '../../core/services/current-user.service';

import { PatientModel, CreatePatientModel } from '../../core/models/patient.model';
import { UserModel } from '../../core/models/user.model';
import { DoctorModel } from '../../core/models/doctor.model';
import { AppointmentModel } from '../../core/models/appointment.model';

@Component({
  selector: 'app-patients',
  standalone: false,
  templateUrl: './patients.html',
  styleUrls: ['./patients.css']
})
export class Patients implements OnInit {

  patients: PatientModel[] = [];
  filteredPatients: PatientModel[] = [];

  searchTerm = '';
  users: UserModel[] = [];
  doctors: DoctorModel[] = [];
  appointments: AppointmentModel[] = [];

  patientForm: FormGroup;

  // Editing only — new patients are created via the Register Patient
  // screen, which creates the User account and Patient profile together.
  showForm = false;
  loading = false;
  errorMessage = '';

  editingId: number | null = null;

  role = '';

  get isDoctorRole(): boolean { return this.role === 'Doctor'; }
  get isAdminRole(): boolean { return this.role === 'Admin' || this.role === 'SuperAdmin'; }
  get isReceptionistRole(): boolean { return this.role === 'Receptionist'; }

  // Doctor gets a read-only "My Patients" list (only patients related to
  // them, via assignment or an actual booked appointment) — no
  // create/edit/delete from this screen.
  get canManagePatients(): boolean { return !this.isDoctorRole; }

  // Registering a brand-new patient is a Receptionist task only — Admin
  // can still view/edit/delete existing patients via canManagePatients,
  // but does not create new ones.
  get canRegisterPatient(): boolean { return this.isReceptionistRole; }

  constructor(
    private fb: FormBuilder,
    private patientService: PatientService,
    private userService: UserService,
    private doctorService: DoctorService,
    private appointmentService: AppointmentService,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private cdr: ChangeDetectorRef
  ) {

    this.patientForm = this.fb.group({
      bloodGroup: [''],
      height: [null],
      weight: [null],
      emergencyContactName: [''],
      emergencyContactPhone: [''],
      allergies: [''],
      medicalHistory: [''],
      status: ['Active'],
      doctorID: [null]
    });

  }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    this.loadUsers();
    this.loadDoctors();
    this.loadAppointments();

    this.currentUserService.ensureLoaded().subscribe(() => {

      this.loadPatients();

    });

  }


  loadPatients(): void {

    this.loading = true;

    this.patientService.getAllPatients().subscribe({

      next: (response: PatientModel[]) => {

        if (this.isDoctorRole) {

          // A patient is "this doctor's" if either:
          //  - they were explicitly assigned this doctor at registration
          //    (primaryDoctorID), OR
          //  - they have an actual booked appointment with this doctor —
          //    e.g. a patient who booked with Doctor A must show up for
          //    Doctor A even if no one set a primary doctor.
          // A doctor must never see another doctor's patients who don't
          // meet either condition.
          const myDoctorId = this.currentUserService.getDoctorId();

          const patientIdsFromAppointments = new Set(
            this.appointments
              .filter(a => a.doctorID === myDoctorId)
              .map(a => a.patientID)
          );

          this.patients = response.filter(p =>
            p.primaryDoctorID === myDoctorId ||
            patientIdsFromAppointments.has(p.patientID)
          );

        } else {

          this.patients = response;

        }

        this.loading = false;
        this.applySearch();
        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load patients.';

        this.cdr.detectChanges();

      }

    });

  }


  loadUsers(): void {

    this.userService.getAllUsers().subscribe({

      next: (response: UserModel[]) => {

        this.users = response;

        this.applySearch();

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  loadDoctors(): void {

    this.doctorService.getAllDoctors().subscribe({

      next: (response: DoctorModel[]) => {
        this.doctors = response;
        this.cdr.detectChanges();
      },

      error: (err) => console.error(err)

    });

  }


  loadAppointments(): void {

    this.appointmentService.getAllAppointments().subscribe({

      next: (response: AppointmentModel[]) => {

        this.appointments = response;

        // Doctor scoping depends on appointments too, so re-run once
        // both patients and appointments are in.
        if (this.isDoctorRole && this.patients.length > 0) {
          this.loadPatients();
        }

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  // Patient search — matches against the linked User's name/email/phone,
  // since that's where those fields actually live (Patient itself has no
  // name/contact info of its own).
  applySearch(): void {

    const term = this.searchTerm.trim().toLowerCase();

    if (!term) {

      this.filteredPatients = this.patients;

      return;

    }

    this.filteredPatients = this.patients.filter(patient => {

      const user = this.users.find(u => u.userID === patient.userID);

      if (!user) return false;

      const haystack = `${user.firstName} ${user.lastName} ${user.email} ${user.phone ?? ''}`.toLowerCase();

      return haystack.includes(term);

    });

  }


  getUserName(userID: number): string {

    const match = this.users.find(u => u.userID === userID);

    return match ? `${match.firstName} ${match.lastName}` : `User #${userID}`;

  }


  getDoctorName(doctorID?: number | null): string {

    if (!doctorID) return 'Unassigned';

    const doctor = this.doctors.find(d => d.doctorID === doctorID);

    if (!doctor) return `Doctor #${doctorID}`;

    const user = this.users.find(u => u.userID === doctor.userID);

    return user ? `Dr. ${user.firstName} ${user.lastName}` : `Doctor #${doctorID}`;

  }


  editPatient(patient: PatientModel): void {

    this.editingId = patient.patientID;

    this.patientForm.patchValue({

      bloodGroup: patient.bloodGroup,
      height: patient.height,
      weight: patient.weight,
      emergencyContactName: patient.emergencyContactName,
      emergencyContactPhone: patient.emergencyContactPhone,
      allergies: patient.allergies,
      medicalHistory: patient.medicalHistory,
      status: patient.status,
      doctorID: patient.primaryDoctorID ?? null

    });

    this.showForm = true;

  }


  savePatient(): void {

    if (this.editingId === null) return;

    if (this.patientForm.invalid) {
      this.patientForm.markAllAsTouched();
      return;
    }

    const current = this.patients.find(p => p.patientID === this.editingId);

    if (!current) return;

    const formValue = this.patientForm.value;

    const patientData: CreatePatientModel = {

      userID: current.userID,
      bloodGroup: formValue.bloodGroup,
      height: formValue.height,
      weight: formValue.weight,
      emergencyContactName: formValue.emergencyContactName,
      emergencyContactPhone: formValue.emergencyContactPhone,
      allergies: formValue.allergies,
      medicalHistory: formValue.medicalHistory,
      status: formValue.status,
      primaryDoctorID: formValue.doctorID

    };

    this.patientService.updatePatient(this.editingId, patientData).subscribe({

      next: () => {

        this.cancelForm();

        this.loadPatients();

      },

      error: (error) => {

        console.error('Update Patient Error:', error);

        this.errorMessage = error?.error?.Message || error?.error?.message || 'Unable to update patient.';

        this.cdr.detectChanges();

      }

    });

  }


  cancelForm(): void {

    this.showForm = false;

    this.editingId = null;

    this.patientForm.reset({ status: 'Active' });

  }


  deletePatient(id: number): void {

    if (!confirm('Are you sure you want to delete this patient?')) {
      return;
    }

    this.patientService.deletePatient(id).subscribe({

      next: () => {

        this.patients = this.patients.filter(p => p.patientID !== id);

        this.applySearch();

        this.cdr.detectChanges();

      },

      error: (error) => {

        console.error('Delete Patient Error:', error);

        this.errorMessage = error?.error?.Message || error?.error?.message || 'Unable to delete patient.';

        this.cdr.detectChanges();

      }

    });

  }

}
