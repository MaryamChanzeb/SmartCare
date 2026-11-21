import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { PatientService } from '../../core/services/patient.service';
import { UserService } from '../../core/services/user.service';
import { MedicalRecordService } from '../../core/services/medical-record.service';
import { AppointmentService } from '../../core/services/appointment.service';
import { DoctorService } from '../../core/services/doctor.service';
import { AuthService } from '../../core/services/auth.service';
import { CurrentUserService } from '../../core/services/current-user.service';

import { PatientModel } from '../../core/models/patient.model';
import { UserModel } from '../../core/models/user.model';
import { MedicalRecordModel } from '../../core/models/medical-record.model';
import { AppointmentModel } from '../../core/models/appointment.model';
import { DoctorModel } from '../../core/models/doctor.model';

@Component({
  selector: 'app-patient-details',
  standalone: false,
  templateUrl: './patient-details.html',
  styleUrls: ['./patient-details.css']
})
export class PatientDetails implements OnInit {

  patientId!: number;

  patient: PatientModel | null = null;
  patientUser: UserModel | null = null;

  medicalRecords: MedicalRecordModel[] = [];
  appointments: AppointmentModel[] = [];

  doctors: DoctorModel[] = [];
  users: UserModel[] = [];

  loading = true;
  errorMessage = '';

  role = '';

  get isDoctorRole(): boolean { return this.role === 'Doctor'; }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private patientService: PatientService,
    private userService: UserService,
    private medicalRecordService: MedicalRecordService,
    private appointmentService: AppointmentService,
    private doctorService: DoctorService,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    const idParam = this.route.snapshot.paramMap.get('patientId');

    if (!idParam) {
      this.router.navigate(['/patients']);
      return;
    }

    this.patientId = Number(idParam);

    this.loadDoctors();
    this.loadUsers();

    this.currentUserService.ensureLoaded().subscribe(() => {

      this.loadPatient();

    });

  }


  loadDoctors(): void {

    this.doctorService.getAllDoctors().subscribe({
      next: (response) => { this.doctors = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

  }


  loadUsers(): void {

    this.userService.getAllUsers().subscribe({

      next: (response) => {

        this.users = response;

        if (this.patient) {
          this.patientUser = this.users.find(u => u.userID === this.patient!.userID) ?? null;
        }

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  private loadPatient(): void {

    this.loading = true;

    this.patientService.getPatientById(this.patientId).subscribe({

      next: (response) => {

        this.patient = response;

        this.patientUser = this.users.find(u => u.userID === response.userID) ?? null;

        this.loadMedicalRecords();
        this.loadAppointments();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load patient details.';

        this.cdr.detectChanges();

      }

    });

  }


  private loadMedicalRecords(): void {

    this.medicalRecordService.getByPatient(this.patientId).subscribe({

      next: (records) => {

        // Doctor only sees the records they personally created for this
        // patient — not records from other doctors who've also treated
        // them. Admin/Receptionist see the full history.
        this.medicalRecords = this.isDoctorRole
          ? records.filter(r => r.doctorID === this.currentUserService.getDoctorId())
          : records;

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.cdr.detectChanges();

      }

    });

  }


  private loadAppointments(): void {

    this.appointmentService.getAllAppointments().subscribe({

      next: (allAppointments) => {

        const forThisPatient = allAppointments.filter(a => a.patientID === this.patientId);

        // Same ownership rule — a doctor sees only the appointments
        // between themselves and this patient, not the patient's full
        // appointment history with other doctors.
        this.appointments = this.isDoctorRole
          ? forThisPatient.filter(a => a.doctorID === this.currentUserService.getDoctorId())
          : forThisPatient;

        this.appointments.sort((a, b) =>
          (b.appointmentDate + b.appointmentTime).localeCompare(a.appointmentDate + a.appointmentTime)
        );

        this.cdr.detectChanges();

      },

      error: (err) => console.error(err)

    });

  }


  getDoctorName(doctorID: number): string {

    const doctor = this.doctors.find(d => d.doctorID === doctorID);

    if (!doctor) return `Doctor #${doctorID}`;

    const user = this.users.find(u => u.userID === doctor.userID);

    return user ? `Dr. ${user.firstName} ${user.lastName}` : `Doctor #${doctorID}`;

  }


  viewPrescriptions(recordId: number): void {

    this.router.navigate(['/prescriptions', recordId]);

  }


  back(): void {

    this.router.navigate(['/patients']);

  }

}
