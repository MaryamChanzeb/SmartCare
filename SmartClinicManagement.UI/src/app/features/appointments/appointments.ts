import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { ActivatedRoute } from '@angular/router';

import { AppointmentService } from '../../core/services/appointment.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PatientService } from '../../core/services/patient.service';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { CurrentUserService } from '../../core/services/current-user.service';
import { ToastService } from '../../core/services/toast.service';

import {
  AppointmentModel,
  BookAppointmentModel,
  UpdateAppointmentModel
} from '../../core/models/appointment.model';
import { DoctorModel } from '../../core/models/doctor.model';
import { PatientModel } from '../../core/models/patient.model';
import { UserModel } from '../../core/models/user.model';


type RawStatus = 'Scheduled' | 'Confirmed' | 'CheckedIn' | 'Completed' | 'Cancelled' | 'NoShow';

const STATUS_LABELS: Record<string, string> = {
  Scheduled: 'Pending',
  Confirmed: 'Confirmed',
  CheckedIn: 'Checked-In',
  Completed: 'Completed',
  Cancelled: 'Cancelled',
  NoShow: 'No Show'
};

@Component({
  selector: 'app-appointments',
  standalone: false,
  templateUrl: './appointments.html',
  styleUrls: ['./appointments.css']
})
export class Appointments implements OnInit {

  appointments: AppointmentModel[] = [];
  doctors: DoctorModel[] = [];
  patients: PatientModel[] = [];
  users: UserModel[] = [];

  appointmentForm: FormGroup;

  showForm = false;
  loading = false;
  errorMessage = '';

  editingId: number | null = null;

  role = '';

  get isPatientRole(): boolean { return this.role === 'Patient'; }
  get isDoctorRole(): boolean { return this.role === 'Doctor'; }
  get isReceptionistRole(): boolean { return this.role === 'Receptionist'; }
  get isAdminRole(): boolean { return this.role === 'Admin' || this.role === 'SuperAdmin'; }

  
  get canBook(): boolean { return !this.isDoctorRole && !this.isAdminRole; }

 
  get showActionsColumn(): boolean { return !this.isAdminRole; }
  get showTypeColumn(): boolean { return !this.isAdminRole; }

  constructor(
    private fb: FormBuilder,
    private appointmentService: AppointmentService,
    private doctorService: DoctorService,
    private patientService: PatientService,
    private userService: UserService,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private toastService: ToastService,
    private route: ActivatedRoute,
    private cdr: ChangeDetectorRef
  ) {

    this.appointmentForm = this.fb.group({
      doctorID: [null, Validators.required],
      patientID: [null, Validators.required],
      appointmentDate: ['', Validators.required],
      appointmentTime: ['', Validators.required],
      appointmentType: [''],
      notes: ['']
    });

  }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    this.loadDoctors();
    this.loadPatients();
    this.loadUsers();

    this.currentUserService.ensureLoaded().subscribe(() => {

      this.loadAppointments();

      const doctorIdParam = this.route.snapshot.queryParamMap.get('doctorId');

      if (doctorIdParam && this.isPatientRole) {

        this.openBookForm();

        this.appointmentForm.patchValue({
          doctorID: Number(doctorIdParam),
          patientID: this.currentUserService.getPatientId()
        });

      }

      const actionParam = this.route.snapshot.queryParamMap.get('action');

      if (actionParam === 'book' && this.canBook && !doctorIdParam) {

        this.openBookForm();

      }

    });

  }


  loadAppointments(): void {

    this.loading = true;

    this.appointmentService.getAllAppointments().subscribe({

      next: (response: AppointmentModel[]) => {

        // Data isolation — the backend returns everything, so ownership is
        // enforced here: Patient sees only their own, Doctor sees only
        // their own, Receptionist/Admin see all (operational data).
        if (this.isPatientRole) {

          const myPatientId = this.currentUserService.getPatientId();

          this.appointments = response.filter(a => a.patientID === myPatientId);

        } else if (this.isDoctorRole) {

          const myDoctorId = this.currentUserService.getDoctorId();

          this.appointments = response.filter(a => a.doctorID === myDoctorId);

        } else {

          this.appointments = response;

        }

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load appointments.';

        this.cdr.detectChanges();

      }

    });

  }


  loadDoctors(): void {

    this.doctorService.getAllDoctors().subscribe({
      next: (response) => { this.doctors = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

  }


  loadPatients(): void {

    this.patientService.getAllPatients().subscribe({
      next: (response) => { this.patients = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

  }


  loadUsers(): void {

    this.userService.getAllUsers().subscribe({
      next: (response) => { this.users = response; this.cdr.detectChanges(); },
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


  // Displays the friendlier workflow label — the raw value stored/sent is
  // untouched, this is presentation only.
  getStatusLabel(status: string): string {

    return STATUS_LABELS[status] ?? status;

  }


  

  canConfirm(appt: AppointmentModel): boolean {

    return this.isReceptionistRole && appt.status === 'Scheduled';

  }


  canCheckIn(appt: AppointmentModel): boolean {

    return this.isReceptionistRole && appt.status === 'Confirmed';

  }


  canComplete(appt: AppointmentModel): boolean {
    return this.isDoctorRole && appt.status === 'CheckedIn';
  }


  canReschedule(appt: AppointmentModel): boolean {

    return this.isReceptionistRole &&
      ['Scheduled', 'Confirmed', 'CheckedIn'].includes(appt.status);

  }


  canCancel(appt: AppointmentModel): boolean {

    const cancellableStatus = ['Scheduled', 'Confirmed', 'CheckedIn'].includes(appt.status);

    if (!cancellableStatus) return false;

    if (this.isPatientRole) return true;              // their own only — list is already filtered
    if (this.isReceptionistRole) return true;

    return false;   // Doctor and Admin cannot cancel

  }


  // ---- Form handling ----

  openBookForm(): void {

    this.editingId = null;

    this.appointmentForm.reset();

    if (this.isPatientRole) {

      this.appointmentForm.patchValue({
        patientID: this.currentUserService.getPatientId()
      });

    }

    this.showForm = true;

  }


  cancelForm(): void {

    this.showForm = false;

    this.editingId = null;

    this.appointmentForm.reset();

  }


  bookAppointment(): void {

    if (this.appointmentForm.invalid) {
      this.appointmentForm.markAllAsTouched();
      return;
    }

    const formValue = this.appointmentForm.value;

    const bookData: BookAppointmentModel = {

      doctorID: formValue.doctorID,
      patientID: this.isPatientRole ? this.currentUserService.getPatientId() : formValue.patientID,
      appointmentDate: formValue.appointmentDate,
      appointmentTime: this.normalizeTime(formValue.appointmentTime),
      appointmentType: formValue.appointmentType,
      notes: formValue.notes

    };

    this.appointmentService.bookAppointment(bookData).subscribe({

      next: () => {

        this.cancelForm();

        this.loadAppointments();

        this.toastService.success(
          'Appointment booked successfully. A confirmation email has been sent to the patient.'
        );

      },

      error: (error) => {

        console.error('Book Appointment Error:', error);

        this.errorMessage = 'Unable to book appointment.';

        this.cdr.detectChanges();

      }

    });

  }


  openRescheduleForm(appointment: AppointmentModel): void {

    this.editingId = appointment.appointmentID;

    this.appointmentForm.patchValue({

      doctorID: appointment.doctorID,
      patientID: appointment.patientID,
      appointmentDate: appointment.appointmentDate,
      appointmentTime: appointment.appointmentTime?.substring(0, 5),
      appointmentType: appointment.appointmentType,
      notes: appointment.notes

    });

    this.showForm = true;

  }


  saveReschedule(): void {

    if (this.editingId === null) return;

    if (this.appointmentForm.invalid) {
      this.appointmentForm.markAllAsTouched();
      return;
    }

    const current = this.appointments.find(a => a.appointmentID === this.editingId);

    if (!current) return;

    const formValue = this.appointmentForm.value;

    const updateData: UpdateAppointmentModel = {

      appointmentDate: formValue.appointmentDate,
      appointmentTime: this.normalizeTime(formValue.appointmentTime),
      appointmentType: formValue.appointmentType,
      status: current.status,
      notes: formValue.notes

    };

    this.appointmentService.updateAppointment(this.editingId, updateData).subscribe({

      next: () => {

        this.cancelForm();

        this.loadAppointments();

        this.toastService.success(
          'Appointment rescheduled successfully. The patient has been notified by email.'
        );

      },

      error: (error) => {

        console.error('Reschedule Error:', error);

        this.errorMessage = 'Unable to reschedule appointment.';

        this.cdr.detectChanges();

      }

    });

  }


  private setStatus(appointment: AppointmentModel, newStatus: RawStatus): void {

    const updateData: UpdateAppointmentModel = {

      appointmentDate: appointment.appointmentDate,
      appointmentTime: appointment.appointmentTime,
      appointmentType: appointment.appointmentType,
      status: newStatus,
      notes: appointment.notes

    };

    this.appointmentService.updateAppointment(appointment.appointmentID, updateData).subscribe({

      next: () => {

        appointment.status = newStatus;

        
        this.loadAppointments();

        this.cdr.detectChanges();

        if (newStatus === 'Confirmed') {

          this.toastService.success(
            'Appointment confirmed successfully. The patient has been notified by email.'
          );

        }

        if (newStatus === 'CheckedIn') {

          this.toastService.success(
            'Patient checked in successfully.'
          );

        }

        if (newStatus === 'Completed') {

          this.toastService.success(
            'Appointment marked as completed.'
          );

        }

      },

      error: (error) => {

        console.error(`Set Status (${newStatus}) Error:`, error);

        
        this.errorMessage =
          'Unable to update appointment status. If this keeps happening, confirm the Status column migration has been applied.';

        this.cdr.detectChanges();

      }

    });

  }


  confirmAppointment(appointment: AppointmentModel): void {
    this.setStatus(appointment, 'Confirmed');
  }


  checkInAppointment(appointment: AppointmentModel): void {
    this.setStatus(appointment, 'CheckedIn');
  }


  completeAppointment(appointment: AppointmentModel): void {
    this.setStatus(appointment, 'Completed');
  }


  cancelAppointment(appointment: AppointmentModel): void {

    if (!confirm('Are you sure you want to cancel this appointment?')) {
      return;
    }

    this.appointmentService.cancelAppointment(appointment.appointmentID).subscribe({

      next: () => {

        appointment.status = 'Cancelled';

        this.cdr.detectChanges();

        this.toastService.success(
          'Appointment cancelled successfully. The patient has been notified by email.'
        );

      },

      error: (error) => {

        console.error('Cancel Appointment Error:', error);

        this.errorMessage = 'Unable to cancel appointment.';

        this.cdr.detectChanges();

      }

    });

  }


  private normalizeTime(time: string): string {

    return time && time.length === 5 ? `${time}:00` : time;

  }

}
