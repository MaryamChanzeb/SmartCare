import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { PaymentService } from '../../core/services/payment.service';
import { BillingService } from '../../core/services/billing.service';
import { AppointmentService } from '../../core/services/appointment.service';
import { PatientService } from '../../core/services/patient.service';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { CurrentUserService } from '../../core/services/current-user.service';

import { PaymentModel, ReceivePaymentModel } from '../../core/models/payment.model';
import { BillingModel } from '../../core/models/billing.model';
import { AppointmentModel } from '../../core/models/appointment.model';
import { PatientModel } from '../../core/models/patient.model';
import { UserModel } from '../../core/models/user.model';

@Component({
  selector: 'app-payments',
  standalone: false,
  templateUrl: './payments.html',
  styleUrls: ['./payments.css']
})
export class Payments implements OnInit {

  payments: PaymentModel[] = [];
  bills: BillingModel[] = [];
  appointments: AppointmentModel[] = [];
  patients: PatientModel[] = [];
  users: UserModel[] = [];

  paymentForm: FormGroup;

  showForm = false;
  loading = false;
  errorMessage = '';

  role = '';

  get isPatientRole(): boolean { return this.role === 'Patient'; }
  get canRecordPayment(): boolean { return !this.isPatientRole; }

  constructor(
    private fb: FormBuilder,
    private paymentService: PaymentService,
    private billingService: BillingService,
    private appointmentService: AppointmentService,
    private patientService: PatientService,
    private userService: UserService,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private cdr: ChangeDetectorRef
  ) {

    this.paymentForm = this.fb.group({
      billID: [null, Validators.required],
      amount: [0, [Validators.required, Validators.min(0.01)]],
      transactionReference: [''],
      status: ['Success', Validators.required]
    });

  }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    this.loadBills();
    this.loadAppointments();
    this.loadPatients();
    this.loadUsers();

    this.currentUserService.ensureLoaded().subscribe(() => {

      this.loadPayments();

    });

  }


  loadPayments(): void {

    this.loading = true;

    this.paymentService.getAllPayments().subscribe({

      next: (response: PaymentModel[]) => {

        if (this.isPatientRole) {

          // Ownership filter — Payments has no patient-scoped endpoint,
          // so payments are matched back via Bill → Appointment → Patient.
          const myPatientId = this.currentUserService.getPatientId();

          this.payments = response.filter(payment => {

            const bill = this.bills.find(b => b.billID === payment.billID);

            if (!bill) return false;

            const appt = this.appointments.find(a => a.appointmentID === bill.appointmentID);

            return appt ? appt.patientID === myPatientId : false;

          });

        } else {

          this.payments = response;

        }

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load payments.';

        this.cdr.detectChanges();

      }

    });

  }


  loadBills(): void {

    this.billingService.getAllBills().subscribe({
      next: (response) => { this.bills = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

  }


  loadAppointments(): void {

    this.appointmentService.getAllAppointments().subscribe({
      next: (response) => { this.appointments = response; this.cdr.detectChanges(); },
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


  getPatientNameForBill(billID: number): string {

    const bill = this.bills.find(b => b.billID === billID);

    if (!bill) return `Bill #${billID}`;

    const appt = this.appointments.find(a => a.appointmentID === bill.appointmentID);

    if (!appt) return `Bill #${billID}`;

    const patient = this.patients.find(p => p.patientID === appt.patientID);

    if (!patient) return `Bill #${billID}`;

    const user = this.users.find(u => u.userID === patient.userID);

    return user ? `${user.firstName} ${user.lastName}` : `Bill #${billID}`;

  }


  billLabel(bill: BillingModel): string {

    return `#${bill.billID} — ${this.getPatientNameForBill(bill.billID)} — ${bill.totalAmount} (${bill.paymentStatus})`;

  }


  // NOTE ON SCHEMA: Payment Method isn't a field on the Payment record
  // itself — your Payments table only has BillID/TransactionReference/
  // Amount/PaymentDate/Status. The payment method that was actually used
  // is captured on the linked Bill instead. Rather than invent a
  // per-payment field that has nowhere to be stored, this looks up and
  // displays the method from the Bill each payment is linked to.
  getPaymentMethod(billID: number): string {

    const bill = this.bills.find(b => b.billID === billID);

    return bill ? bill.paymentMethod : '—';

  }


  openRecordForm(): void {

    this.paymentForm.reset({
      amount: 0,
      status: 'Success'
    });

    this.showForm = true;

  }


  onBillSelected(): void {

    const billId = this.paymentForm.value.billID;

    const bill = this.bills.find(b => b.billID === billId);

    if (bill) {
      this.paymentForm.patchValue({ amount: bill.totalAmount });
    }

  }


  cancelForm(): void {

    this.showForm = false;

    this.paymentForm.reset({
      amount: 0,
      status: 'Success'
    });

  }


  recordPayment(): void {

    if (this.paymentForm.invalid) {
      this.paymentForm.markAllAsTouched();
      return;
    }

    const formValue = this.paymentForm.value;

    const paymentData: ReceivePaymentModel = {

      billID: formValue.billID,
      amount: formValue.amount,
      transactionReference: formValue.transactionReference,
      status: formValue.status

    };

    this.paymentService.receivePayment(paymentData).subscribe({

      next: () => {

        this.cancelForm();

        this.loadPayments();

      },

      error: (error) => {

        console.error('Record Payment Error:', error);

        this.errorMessage = 'Unable to record payment.';

        this.cdr.detectChanges();

      }

    });

  }

}
