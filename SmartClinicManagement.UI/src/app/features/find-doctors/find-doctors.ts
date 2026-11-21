import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { Router } from '@angular/router';

import { DoctorService } from '../../core/services/doctor.service';
import { UserService } from '../../core/services/user.service';
import { DepartmentService } from '../../core/services/department.service';

import { DoctorModel } from '../../core/models/doctor.model';
import { UserModel } from '../../core/models/user.model';
import { DepartmentModel } from '../../core/models/department.model';

@Component({
  selector: 'app-find-doctors',
  standalone: false,
  templateUrl: './find-doctors.html',
  styleUrls: ['./find-doctors.css']
})
export class FindDoctors implements OnInit {

  doctors: DoctorModel[] = [];
  users: UserModel[] = [];
  departments: DepartmentModel[] = [];

  filteredDoctors: DoctorModel[] = [];

  searchTerm = '';
  specializationFilter = '';

  loading = true;

  constructor(
    private doctorService: DoctorService,
    private userService: UserService,
    private departmentService: DepartmentService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

    this.loadDoctors();
    this.loadUsers();
    this.loadDepartments();

  }


  loadDoctors(): void {

    this.loading = true;

    this.doctorService.getAllDoctors().subscribe({

      next: (response) => {

        this.doctors = response.filter(d => d.status === 'Active');

        this.applyFilters();

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


  loadUsers(): void {

    this.userService.getAllUsers().subscribe({
      next: (response) => { this.users = response; this.applyFilters(); this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

  }


  loadDepartments(): void {

    this.departmentService.getAllDepartments().subscribe({
      next: (response) => { this.departments = response; this.cdr.detectChanges(); },
      error: (err) => console.error(err)
    });

  }


  getDoctorName(doctorID: number): string {

    const doctor = this.doctors.find(d => d.doctorID === doctorID);

    if (!doctor) return `Doctor #${doctorID}`;

    const user = this.users.find(u => u.userID === doctor.userID);

    return user ? `Dr. ${user.firstName} ${user.lastName}` : `Doctor #${doctorID}`;

  }


  get specializations(): string[] {

    return [...new Set(this.doctors.map(d => d.specialization))].sort();

  }


  applyFilters(): void {

    const term = this.searchTerm.trim().toLowerCase();

    this.filteredDoctors = this.doctors.filter(doctor => {

      const matchesSpecialization =
        !this.specializationFilter || doctor.specialization === this.specializationFilter;

      const matchesTerm =
        !term ||
        this.getDoctorName(doctor.doctorID).toLowerCase().includes(term) ||
        doctor.specialization.toLowerCase().includes(term);

      return matchesSpecialization && matchesTerm;

    });

  }


  bookWithDoctor(doctor: DoctorModel): void {

    this.router.navigate(['/appointments'], {
      queryParams: { doctorId: doctor.doctorID }
    });

  }

}
