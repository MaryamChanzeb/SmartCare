import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { Router } from '@angular/router';

import { MedicalRecordService } from '../../core/services/medical-record.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PatientService } from '../../core/services/patient.service';
import { UserService } from '../../core/services/user.service';
import { AppointmentService } from '../../core/services/appointment.service';
import { AuthService } from '../../core/services/auth.service';
import { CurrentUserService } from '../../core/services/current-user.service';

import {
  MedicalRecordModel,
  CreateMedicalRecordModel
} from '../../core/models/medical-record.model';
import { DoctorModel } from '../../core/models/doctor.model';
import { PatientModel } from '../../core/models/patient.model';
import { UserModel } from '../../core/models/user.model';

type SearchMode = 'patient' | 'doctor' | 'all';

@Component({
  selector: 'app-medical-records',
  standalone: false,
  templateUrl: './medical-records.html',
  styleUrls: ['./medical-records.css']
})
export class MedicalRecords implements OnInit {

  records: MedicalRecordModel[] = [];
  doctors: DoctorModel[] = [];
  patients: PatientModel[] = [];
  users: UserModel[] = [];

  assignedPatients: PatientModel[] = [];

  searchMode: SearchMode = 'patient';
  selectedPatientId: number | null = null;
  selectedDoctorId: number | null = null;
  hasSearched = false;

  recordForm: FormGroup;

  showForm = false;
  loading = false;
  errorMessage = '';

  // FIX: shown when a Patient/Doctor login has no matching Patient/Doctor
  // record at all, instead of silently looking like "no records exist."
  linkWarning = '';

  editingId: number | null = null;

  role = '';

  get isReceptionistRole(): boolean { return this.role === 'Receptionist'; }
  get isPatientRole(): boolean { return this.role === 'Patient'; }
  get isDoctorRole(): boolean { return this.role === 'Doctor'; }
  get isAdminRole(): boolean { return this.role === 'Admin' || this.role === 'SuperAdmin'; }

  get canManageSearch(): boolean { return this.isAdminRole; }
  get canAddRecord(): boolean { return this.isAdminRole || this.isDoctorRole; }

  get patientOptionsForForm(): PatientModel[] {
    return this.isDoctorRole ? this.assignedPatients : this.patients;
  }

  constructor(
    private fb: FormBuilder,
    private medicalRecordService: MedicalRecordService,
    private doctorService: DoctorService,
    private patientService: PatientService,
    private userService: UserService,
    private appointmentService: AppointmentService,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {

    this.recordForm = this.fb.group({
      patientID: [null, Validators.required],
      doctorID: [null, Validators.required],
      diagnosis: [''],
      symptoms: [''],
      treatment: [''],
      notes: [''],
      visitDate: ['']
    });

  }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    if (this.isReceptionistRole) {

      // Receptionist gets a read-only view of all completed checkup
      // records — no per-patient/doctor search needed, and no ability
      // to add/edit (canAddRecord already excludes Receptionist).
      this.loadDoctors();

      this.patientService.getAllPatients().subscribe({

        next: (patients) => {

          this.patients = patients;

          this.userService.getAllUsers().subscribe({

            next: (users) => {

              this.users = users;

              this.searchMode = 'all';
              this.search();

              this.cdr.detectChanges();

            },

            error: (err) => console.error(err)

          });

        },

        error: (err) => console.error(err)

      });

      return;

    }

    // FIX: for Patient/Doctor, resolve their own PatientID/DoctorID
    // directly from freshly-fetched data in this component rather than
    // relying solely on CurrentUserService's cached value. This removes
    // any dependency on cross-component cache timing/staleness as a
    // possible cause of "my own records show as empty."
    this.loadDoctors();

    this.patientService.getAllPatients().subscribe({

      next: (patients) => {

        this.patients = patients;

        this.userService.getAllUsers().subscribe({

          next: (users) => {

            this.users = users;

            this.resolveOwnIdentityAndSearch();

            this.cdr.detectChanges();

          },

          error: (err) => console.error(err)

        });

      },

      error: (err) => console.error(err)

    });

  }


  private resolveOwnIdentityAndSearch(): void {

    if (this.isPatientRole) {

      // FIX (root cause): previously searched the already-fetched
      // `this.patients` list for `p.userID === myUserId` client-side —
      // which only ever showed "not linked" for an account whose Patient
      // row genuinely didn't exist yet (e.g. created via the generic
      // Users screen instead of Register Patient). GET /api/Patients/me
      // resolves the same UserID -> Patient.UserID relationship
      // server-side, from the authenticated JWT, and auto-creates the
      // missing Patient row if needed — so an already-orphaned account
      // self-heals here instead of staying permanently broken.
      this.patientService.getMyProfile().subscribe({

        next: (myPatient) => {

          this.searchMode = 'patient';
          this.selectedPatientId = myPatient.patientID;
          this.search();

        },

        error: (err) => {

          console.error('Resolve own Patient profile error:', err);

          this.linkWarning =
            err?.error?.Message ||
            err?.error?.message ||
            'Your login account isn\'t linked to a Patient profile, so no medical records can be shown. ' +
            'Please contact reception to confirm your Patient record is linked to the correct user account.';

          this.hasSearched = true;

          this.cdr.detectChanges();

        }

      });

      return;

    }

    const myUserId = this.authService.getUserId();

    if (this.isDoctorRole) {

      const myDoctor = this.doctors.find(d => d.userID === myUserId);

      if (!myDoctor) {

        this.linkWarning =
          'Your login account isn\'t linked to a Doctor profile, so no medical records can be shown.';

        this.hasSearched = true;

        this.cdr.detectChanges();

        return;

      }

      this.loadAssignedPatients(myDoctor.doctorID);

      this.searchMode = 'doctor';
      this.selectedDoctorId = myDoctor.doctorID;
      this.search();

    }

  }


  private loadAssignedPatients(myDoctorId: number): void {

    this.appointmentService.getAllAppointments().subscribe({

      next: (appointments) => {

        const myPatientIds = new Set(
          appointments.filter(a => a.doctorID === myDoctorId).map(a => a.patientID)
        );

        this.assignedPatients = this.patients.filter(p => myPatientIds.has(p.patientID));

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  loadDoctors(): void {

    this.doctorService.getAllDoctors().subscribe({
      next: (response) => { this.doctors = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

  }


  getDoctorName(doctorID: number): string {

    const doctor = this.doctors.find(d => d.doctorID === doctorID);

    if (!doctor) return `Doctor #${doctorID}`;

    const user = this.users.find(u => u.userID === doctor.userID);

    return user ? `Dr. ${user.firstName} ${user.lastName}` : `Doctor #${doctorID}`;

  }


  getPatientName(patientID: number): string {

    const patient = this.patients.find(p => p.patientID === patientID);

    if (!patient) return `Patient #${patientID}`;

    const user = this.users.find(u => u.userID === patient.userID);

    return user ? `${user.firstName} ${user.lastName}` : `Patient #${patientID}`;

  }


  setSearchMode(mode: SearchMode): void {

    this.searchMode = mode;
    this.records = [];
    this.hasSearched = false;

  }


  search(): void {

    if (this.searchMode === 'patient' && !this.selectedPatientId) return;
    if (this.searchMode === 'doctor' && !this.selectedDoctorId) return;

    this.loading = true;
    this.hasSearched = true;
    this.linkWarning = '';

    const request =
      this.searchMode === 'all'
        ? this.medicalRecordService.getAll()
        : this.searchMode === 'patient'
          ? this.medicalRecordService.getByPatient(this.selectedPatientId!)
          : this.medicalRecordService.getByDoctor(this.selectedDoctorId!);

    request.subscribe({

      next: (response: MedicalRecordModel[]) => {

        this.records = response;

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load medical records.';

        this.cdr.detectChanges();

      }

    });

  }


  openCreateForm(): void {

    this.editingId = null;

    this.recordForm.reset({

      patientID: this.searchMode === 'patient' ? this.selectedPatientId : null,
      doctorID: this.isDoctorRole ? this.selectedDoctorId : (this.searchMode === 'doctor' ? this.selectedDoctorId : null)

    });

    this.showForm = true;

  }


  editRecord(record: MedicalRecordModel): void {

    this.editingId = record.recordID;

    this.recordForm.patchValue({

      patientID: record.patientID,
      doctorID: record.doctorID,
      diagnosis: record.diagnosis,
      symptoms: record.symptoms,
      treatment: record.treatment,
      notes: record.notes,
      visitDate: record.visitDate ? record.visitDate.substring(0, 10) : ''

    });

    this.showForm = true;

  }


  saveRecord(): void {

    if (this.recordForm.invalid) {
      this.recordForm.markAllAsTouched();
      return;
    }

    const formValue = this.recordForm.value;

    const recordData: CreateMedicalRecordModel = {

      patientID: formValue.patientID,
      doctorID: formValue.doctorID,
      diagnosis: formValue.diagnosis,
      symptoms: formValue.symptoms,
      treatment: formValue.treatment,
      notes: formValue.notes,
      visitDate: formValue.visitDate

    };

    if (this.editingId !== null) {

      this.medicalRecordService.updateRecord(this.editingId, recordData).subscribe({

        next: () => {

          this.cancelForm();

          this.search();

        },

        error: (error) => {

          console.error('Update Medical Record Error:', error);

          this.errorMessage = 'Unable to update medical record.';

          this.cdr.detectChanges();

        }

      });

      return;
    }

    this.medicalRecordService.createRecord(recordData).subscribe({

      next: () => {

        this.cancelForm();

        this.search();

      },

      error: (error) => {

        console.error('Create Medical Record Error:', error);

        this.errorMessage = 'Unable to create medical record.';

        this.cdr.detectChanges();

      }

    });

  }


  cancelForm(): void {

    this.showForm = false;

    this.editingId = null;

    this.recordForm.reset();

  }


  viewPrescriptions(recordId: number): void {

    this.router.navigate(['/prescriptions', recordId]);

  }

}
