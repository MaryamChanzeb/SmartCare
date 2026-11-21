import { Component, OnInit, ChangeDetectorRef } from '@angular/core';

import { DashboardService } from '../../core/services/dashboard.service';
import { AppointmentService } from '../../core/services/appointment.service';
import { BillingService } from '../../core/services/billing.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PatientService } from '../../core/services/patient.service';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';
import { CurrentUserService } from '../../core/services/current-user.service';

import { DashboardModel } from '../../core/models/dashboard.model';
import { AppointmentModel } from '../../core/models/appointment.model';
import { BillingModel } from '../../core/models/billing.model';
import { DoctorModel } from '../../core/models/doctor.model';
import { PatientModel } from '../../core/models/patient.model';
import { UserModel } from '../../core/models/user.model';

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css'
})
export class Dashboard implements OnInit {

  fullName = localStorage.getItem('fullName') ?? 'User';

  role = '';

  loading = true;

  errorMessage = '';

  // Shared lookups (used across roles to resolve doctor/patient display names)
  private doctors: DoctorModel[] = [];
  private patients: PatientModel[] = [];
  private users: UserModel[] = [];

  // ---- Admin ----
  stats: DashboardModel | null = null;
  recentAppointments: AppointmentModel[] = [];

  // ---- Doctor ----
  doctorTodayAppointments: AppointmentModel[] = [];
  doctorUpcomingCount = 0;
  doctorPatientCount = 0;

  // ---- Receptionist ----
  receptionistTodayAppointments: AppointmentModel[] = [];
  receptionistCheckedInCount = 0;
  receptionistPendingCount = 0;
  receptionistPendingAmount = 0;

  // ---- Patient ----
  patientNextAppointment: AppointmentModel | null = null;
  patientUpcomingAppointments: AppointmentModel[] = [];

  get isAdminRole(): boolean {
    return this.role === 'Admin' || this.role === 'SuperAdmin';
  }

  get isDoctorRole(): boolean { return this.role === 'Doctor'; }
  get isReceptionistRole(): boolean { return this.role === 'Receptionist'; }
  get isPatientRole(): boolean { return this.role === 'Patient'; }

  constructor(
    private dashboardService: DashboardService,
    private appointmentService: AppointmentService,
    private billingService: BillingService,
    private doctorService: DoctorService,
    private patientService: PatientService,
    private userService: UserService,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    // Lookups needed for name resolution — loaded regardless of role,
    // used by Admin (recent appointments) and Receptionist (today's list).
    this.doctorService.getAllDoctors().subscribe({
      next: (response) => { this.doctors = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

    this.patientService.getAllPatients().subscribe({
      next: (response) => { this.patients = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

    this.userService.getAllUsers().subscribe({
      next: (response) => { this.users = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

    this.currentUserService.ensureLoaded().subscribe(() => {

      if (this.isDoctorRole) {

        this.loadDoctorDashboard();

      } else if (this.isReceptionistRole) {

        this.loadReceptionistDashboard();

      } else if (this.isPatientRole) {

        this.loadPatientDashboard();

      } else {

        // Admin / SuperAdmin
        this.loadAdminDashboard();

      }

    });

  }


  // ============ ADMIN ============

  private loadAdminDashboard(): void {

    this.loading = true;

    this.loadStatsAndRecent();

  }


  // Shared by Admin and Receptionist — both get the same real dashboard
  // statistics/charts (Appointments by Status, Billing Status, Revenue
  // Trend) and Recent Appointments, reusing the existing DashboardService
  // call rather than duplicating it per role.
  private loadStatsAndRecent(): void {

    this.dashboardService.getDashboard().subscribe({

      next: (stats) => {

        this.stats = stats;

        this.loadRecentAppointments();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load dashboard statistics.';

        this.cdr.detectChanges();

      }

    });

  }


  private loadRecentAppointments(): void {

    this.appointmentService.getAllAppointments().subscribe({

      next: (appointments) => {

        this.recentAppointments = [...appointments]
          .sort((a, b) =>
            (b.appointmentDate + b.appointmentTime).localeCompare(a.appointmentDate + a.appointmentTime)
          )
          .slice(0, 5);

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


  // ============ DOCTOR ============

  private loadDoctorDashboard(): void {

    this.loading = true;

    const myDoctorId = this.currentUserService.getDoctorId();

    this.appointmentService.getAllAppointments().subscribe({

      next: (response) => {

        const mine = response.filter(a => a.doctorID === myDoctorId);

        const today = this.todayString();

        this.doctorTodayAppointments = mine
          .filter(a => a.appointmentDate === today)
          .sort((a, b) => a.appointmentTime.localeCompare(b.appointmentTime));

        this.doctorUpcomingCount = mine.filter(
          a => a.appointmentDate >= today && (a.status === 'Scheduled' || a.status === 'Confirmed')
        ).length;

        this.doctorPatientCount = new Set(mine.map(a => a.patientID)).size;

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load your appointments.';

        this.cdr.detectChanges();

      }

    });

  }


  // ============ RECEPTIONIST ============

  private loadReceptionistDashboard(): void {

    this.loading = true;

    const today = this.todayString();

    this.appointmentService.getAllAppointments().subscribe({

      next: (appointments) => {

        this.receptionistTodayAppointments = appointments
          .filter(a => a.appointmentDate === today)
          .sort((a, b) => a.appointmentTime.localeCompare(b.appointmentTime));

        // "Check-in" isn't a distinct status in the backend — a Confirmed
        // appointment today is treated as checked-in; Scheduled means the
        // patient hasn't arrived/been confirmed yet.
        this.receptionistCheckedInCount = this.receptionistTodayAppointments.filter(
          a => a.status === 'Confirmed' || a.status === 'Completed'
        ).length;

        this.loadPendingBills();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load today\'s appointments.';

        this.cdr.detectChanges();

      }

    });

  }


  private loadPendingBills(): void {

    this.billingService.getAllBills().subscribe({

      next: (bills: BillingModel[]) => {

        const pending = bills.filter(b => b.paymentStatus === 'Pending');

        this.receptionistPendingCount = pending.length;

        this.receptionistPendingAmount = pending.reduce((sum, b) => sum + b.totalAmount, 0);

        // Receptionist now sees the same real statistics/charts (and
        // Recent Appointments) as Admin — reusing the same DashboardService
        // call and template, not a separate/duplicated view.
        this.loadStatsAndRecent();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.cdr.detectChanges();

      }

    });

  }


  // ============ PATIENT ============

  private loadPatientDashboard(): void {

    this.loading = true;

    const myPatientId = this.currentUserService.getPatientId();

    this.appointmentService.getAllAppointments().subscribe({

      next: (response) => {

        const mine = response.filter(a => a.patientID === myPatientId);

        const today = this.todayString();

        this.patientUpcomingAppointments = mine
          .filter(a => a.appointmentDate >= today && (a.status === 'Scheduled' || a.status === 'Confirmed'))
          .sort((a, b) =>
            (a.appointmentDate + a.appointmentTime).localeCompare(b.appointmentDate + b.appointmentTime)
          );

        this.patientNextAppointment =
          this.patientUpcomingAppointments.length > 0 ? this.patientUpcomingAppointments[0] : null;

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load your appointments.';

        this.cdr.detectChanges();

      }

    });

  }


  // ============ Shared helpers ============

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


  private todayString(): string {
    return new Date().toISOString().substring(0, 10);
  }


  getGreeting(): string {

    const hour = new Date().getHours();

    if (hour < 12) return 'Good Morning';
    if (hour < 17) return 'Good Afternoon';
    return 'Good Evening';

  }


  // ============ CHARTS (Admin) ============
  //
  // Built entirely from the existing DashboardModel — no new backend
  // endpoint and no new npm chart library. The donut is a plain CSS
  // conic-gradient (data-driven via an inline style), the bars are plain
  // flexbox widths. Both are real, functioning charts with zero added
  // dependencies/build risk.

  private readonly statusColors: Record<string, string> = {
    Scheduled: '#fd7e14',
    Completed: '#198754',
    Cancelled: '#dc3545',
    Other: '#6c757d'
  };

  get appointmentStatusSegments(): { label: string; value: number; color: string }[] {

    if (!this.stats) return [];

    const { scheduledAppointments, completedAppointments, cancelledAppointments, totalAppointments } = this.stats;

    // Appointments that are Confirmed/CheckedIn (in progress) aren't
    // broken out individually in DashboardModel — grouped here as
    // "Other" rather than silently dropped, so the segments always sum
    // to the real total.
    const other = Math.max(
      0,
      totalAppointments - scheduledAppointments - completedAppointments - cancelledAppointments
    );

    return [
      { label: 'Scheduled', value: scheduledAppointments, color: this.statusColors['Scheduled'] },
      { label: 'Completed', value: completedAppointments, color: this.statusColors['Completed'] },
      { label: 'Cancelled', value: cancelledAppointments, color: this.statusColors['Cancelled'] },
      { label: 'Other', value: other, color: this.statusColors['Other'] }
    ].filter(s => s.value > 0);

  }

  // CSS conic-gradient string for the donut, e.g. "#fd7e14 0deg 120deg, ...".
  get appointmentStatusGradient(): string {

    const segments = this.appointmentStatusSegments;
    const total = segments.reduce((sum, s) => sum + s.value, 0);

    if (total === 0) return '#e9ecef 0deg 360deg';

    let start = 0;

    const stops = segments.map(s => {
      const sweep = (s.value / total) * 360;
      const stop = `${s.color} ${start}deg ${start + sweep}deg`;
      start += sweep;
      return stop;
    });

    return stops.join(', ');

  }

  get billingSegments(): { label: string; value: number; color: string }[] {

    if (!this.stats) return [];

    return [
      { label: 'Paid', value: this.stats.paidBills, color: '#198754' },
      { label: 'Pending', value: this.stats.pendingBills, color: '#ffc107' }
    ];

  }

  get maxBillingValue(): number {
    return Math.max(1, ...this.billingSegments.map(s => s.value));
  }

  // ============ REVENUE TREND (replaces Doctors vs Patients) ============
  //
  // Real monthly paid-revenue data from the backend (DashboardDto.MonthlyRevenue)
  // — a simple, dependency-free bar chart using SVG, scaled to fit compactly.

  get revenuePoints(): { month: string; revenue: number }[] {
    return this.stats?.monthlyRevenue ?? [];
  }

  get maxMonthlyRevenue(): number {
    return Math.max(1, ...this.revenuePoints.map(p => p.revenue));
  }

  revenueBarHeight(revenue: number): number {
    // Percentage height (0-100) relative to the tallest month, used as an
    // inline style so the bar never overflows its fixed-height container.
    return Math.round((revenue / this.maxMonthlyRevenue) * 100);
  }

  formatRevenueShort(value: number): string {

    if (value >= 1000) {
      return '$' + (value / 1000).toFixed(1) + 'k';
    }

    return '$' + value.toFixed(0);

  }

}
