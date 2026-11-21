import { Component, OnInit, ChangeDetectorRef } from '@angular/core';

import { AppointmentService } from '../../core/services/appointment.service';
import { DoctorService } from '../../core/services/doctor.service';
import { PatientService } from '../../core/services/patient.service';
import { UserService } from '../../core/services/user.service';

import { AppointmentModel } from '../../core/models/appointment.model';
import { DoctorModel } from '../../core/models/doctor.model';
import { PatientModel } from '../../core/models/patient.model';
import { UserModel } from '../../core/models/user.model';

@Component({
  selector: 'app-doctor-schedule',
  standalone: false,
  templateUrl: './doctor-schedule.html',
  styleUrls: ['./doctor-schedule.css']
})
export class DoctorSchedule implements OnInit {

  doctors: DoctorModel[] = [];
  patients: PatientModel[] = [];
  users: UserModel[] = [];

  allAppointments: AppointmentModel[] = [];
  scheduleForDay: AppointmentModel[] = [];

  selectedDoctorId: number | null = null;
  selectedDate: string;

  loading = false;
  hasSearched = false;

  constructor(
    private appointmentService: AppointmentService,
    private doctorService: DoctorService,
    private patientService: PatientService,
    private userService: UserService,
    private cdr: ChangeDetectorRef
  ) {

    this.selectedDate = new Date().toISOString().substring(0, 10);

  }

  ngOnInit(): void {

    this.loadDoctors();
    this.loadPatients();
    this.loadUsers();
    this.loadAppointments();

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


  loadAppointments(): void {

    this.loading = true;

    this.appointmentService.getAllAppointments().subscribe({

      next: (response) => {

        this.allAppointments = response;

        this.loading = false;

        if (this.selectedDoctorId) {
          this.applyFilter();
        }

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.cdr.detectChanges();

      }

    });

  }


  viewSchedule(): void {

    this.hasSearched = true;

    this.applyFilter();

  }


  private applyFilter(): void {

    this.scheduleForDay = this.allAppointments
      .filter(a => a.doctorID === this.selectedDoctorId && a.appointmentDate === this.selectedDate)
      .sort((a, b) => a.appointmentTime.localeCompare(b.appointmentTime));

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


  get selectedDoctor(): DoctorModel | undefined {

    return this.doctors.find(d => d.doctorID === this.selectedDoctorId);

  }

}
