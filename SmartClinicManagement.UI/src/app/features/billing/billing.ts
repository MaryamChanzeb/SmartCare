import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { BillingService } from '../../core/services/billing.service';
import { AppointmentService } from '../../core/services/appointment.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PatientService } from '../../core/services/patient.service';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { CurrentUserService } from '../../core/services/current-user.service';

import {
  BillingModel,
  CreateBillingModel
} from '../../core/models/billing.model';
import { AppointmentModel } from '../../core/models/appointment.model';
import { DoctorModel } from '../../core/models/doctor.model';
import { PatientModel } from '../../core/models/patient.model';
import { UserModel } from '../../core/models/user.model';

@Component({
  selector: 'app-billing',
  standalone: false,
  templateUrl: './billing.html',
  styleUrls: ['./billing.css']
})
export class Billing implements OnInit {

  bills: BillingModel[] = [];
  appointments: AppointmentModel[] = [];
  doctors: DoctorModel[] = [];
  patients: PatientModel[] = [];
  users: UserModel[] = [];

  billForm: FormGroup;

  showForm = false;
  loading = false;
  errorMessage = '';

  editingId: number | null = null;

  role = '';

  get isPatientRole(): boolean { return this.role === 'Patient'; }
  get isAdminRole(): boolean { return this.role === 'Admin' || this.role === 'SuperAdmin'; }
  get isReceptionistRole(): boolean { return this.role === 'Receptionist'; }
  get isDoctorRole(): boolean {
    return this.role === 'Doctor';
  }

  // Creating/editing bills — front desk and accounts, never the Patient.
  get canManageBilling(): boolean { return !this.isPatientRole && !this.isAdminRole; }

  // Only the Receptionist actually takes payment. Admin views billing
  // data but must not be able to mark a bill paid.
  get canMarkPaid(): boolean { return this.isReceptionistRole; }

  // Patient gets bill details + status only — no Actions column at all.
  get showActionsColumn(): boolean { return this.isReceptionistRole; }

  constructor(
    private fb: FormBuilder,
    private billingService: BillingService,
    private appointmentService: AppointmentService,
    private doctorService: DoctorService,
    private patientService: PatientService,
    private userService: UserService,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private cdr: ChangeDetectorRef
  ) {

    this.billForm = this.fb.group({

      appointmentID: [null, Validators.required],

      // NOTE ON SCHEMA: the backend's Billing table only stores a single
      // "Amount" column — there's no separate "consultation fee" vs
      // "additional charges" split. consultationFee and additionalCharges
      // are two frontend inputs that get summed into that one Amount
      // field right before saving (see buildBillPayload()). If you want
      // these tracked as genuinely separate, persisted values later,
      // that needs a new column on Billing — this UI can't create data
      // that has nowhere to be stored.
      consultationFee: [0, [Validators.required, Validators.min(0)]],
      additionalCharges: [0, Validators.min(0)],

      discount: [0, Validators.min(0)],
      tax: [0, Validators.min(0)],
      paymentMethod: ['Cash', Validators.required]

    });

  }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    this.loadAppointments();
    this.loadDoctors();
    this.loadPatients();
    this.loadUsers();

    this.currentUserService.ensureLoaded().subscribe(() => {

      this.loadBills();

    });

  }


  loadBills(): void {

    this.loading = true;

    this.billingService.getAllBills().subscribe({

      next: (response: BillingModel[]) => {

        // Patient: only own bills
        if (this.isPatientRole) {

          const myPatientId = this.currentUserService.getPatientId();

          this.bills = response.filter(bill => {

            const appt = this.appointments.find(
              a => a.appointmentID === bill.appointmentID
            );

            return appt ? appt.patientID === myPatientId : false;

          });

        }

        // Doctor: only bills of their patients
        else if (this.isDoctorRole) {

          const myDoctorId = this.currentUserService.getDoctorId();

          this.bills = response.filter(bill => {

            const appt = this.appointments.find(
              a => a.appointmentID === bill.appointmentID
            );

            return appt ? appt.doctorID === myDoctorId : false;

          });

        }

        // Admin / Receptionist: all bills
        else {

          this.bills = response;

        }

        this.loading = false;
        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;
        this.errorMessage = 'Unable to load bills.';

        this.cdr.detectChanges();

      }

    });
  }


  loadAppointments(): void {

    this.appointmentService.getAllAppointments().subscribe({
      next: (response) => { this.appointments = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
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


  getPatientNameForAppointment(appointmentID: number): string {

    const appt = this.appointments.find(a => a.appointmentID === appointmentID);

    if (!appt) return `Appointment #${appointmentID}`;

    const patient = this.patients.find(p => p.patientID === appt.patientID);

    if (!patient) return `Appointment #${appointmentID}`;

    const user = this.users.find(u => u.userID === patient.userID);

    return user ? `${user.firstName} ${user.lastName}` : `Appointment #${appointmentID}`;

  }


  appointmentLabel(appointment: AppointmentModel): string {

    return `#${appointment.appointmentID} — ${this.getPatientNameForAppointment(appointment.appointmentID)} (${appointment.appointmentDate})`;

  }


  // Auto-fills Consultation Fee from the selected appointment's doctor —
  // a real, stored value (Doctor.ConsultationFee) rather than making the
  // person type it in from memory every time.
  onAppointmentSelected(): void {

    const appointmentId = this.billForm.value.appointmentID;

    const appt = this.appointments.find(a => a.appointmentID === appointmentId);

    if (!appt) return;

    const doctor = this.doctors.find(d => d.doctorID === appt.doctorID);

    if (doctor) {

      this.billForm.patchValue({ consultationFee: doctor.consultationFee });

    }

  }


  // Live preview only — the authoritative TotalAmount is computed by the
  // backend and returned in the response after save.
  get estimatedTotal(): number {

    const v = this.billForm.value;

    const subtotal = (v.consultationFee || 0) + (v.additionalCharges || 0);

    return subtotal - (v.discount || 0) + (v.tax || 0);

  }


  openCreateForm(): void {

    this.editingId = null;

    this.billForm.reset({
      consultationFee: 0,
      additionalCharges: 0,
      discount: 0,
      tax: 0,
      paymentMethod: 'Cash'
    });

    this.showForm = true;

  }


  editBill(bill: BillingModel): void {

    this.editingId = bill.billID;

    // The original consultation-fee/additional-charges split isn't
    // recoverable — the backend only ever stored their sum as Amount.
    // Editing loads that sum into Consultation Fee and resets Additional
    // Charges to 0; add further charges on top if needed.
    this.billForm.patchValue({

      appointmentID: bill.appointmentID,
      consultationFee: bill.amount,
      additionalCharges: 0,
      discount: bill.discount,
      tax: bill.tax,
      paymentMethod: bill.paymentMethod

    });

    this.showForm = true;

  }


  private buildBillPayload(): CreateBillingModel {

    const formValue = this.billForm.value;

    return {

      appointmentID: formValue.appointmentID,
      amount: (formValue.consultationFee || 0) + (formValue.additionalCharges || 0),
      discount: formValue.discount,
      tax: formValue.tax,
      paymentMethod: formValue.paymentMethod

    };

  }


  saveBill(): void {

    if (this.billForm.invalid) {
      this.billForm.markAllAsTouched();
      return;
    }

    const billData = this.buildBillPayload();

    if (this.editingId !== null) {

      this.billingService.updateBill(this.editingId, billData).subscribe({

        next: () => {

          this.cancelForm();

          this.loadBills();

        },

        error: (error) => {

          console.error('Update Bill Error:', error);

          this.errorMessage = 'Unable to update bill.';

          this.cdr.detectChanges();

        }

      });

      return;
    }

    this.billingService.createBill(billData).subscribe({

      next: () => {

        this.cancelForm();

        this.loadBills();

      },

      error: (error) => {

        console.error('Create Bill Error:', error);

        this.errorMessage = 'Unable to create bill.';

        this.cdr.detectChanges();

      }

    });

  }


  cancelForm(): void {

    this.showForm = false;

    this.editingId = null;

    this.billForm.reset({
      consultationFee: 0,
      additionalCharges: 0,
      discount: 0,
      tax: 0,
      paymentMethod: 'Cash'
    });

  }


  markAsPaid(bill: BillingModel): void {

    this.billingService.markAsPaid(bill.billID, { paymentMethod: bill.paymentMethod }).subscribe({

      next: () => {

        bill.paymentStatus = 'Paid';

        this.cdr.detectChanges();

      },

      error: (error) => {

        console.error('Mark As Paid Error:', error);

        this.errorMessage = 'Unable to mark bill as paid.';

        this.cdr.detectChanges();

      }

    });

  }

}
