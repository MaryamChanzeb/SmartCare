import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

import { DoctorService } from '../../core/services/doctor.service';
import { UserService } from '../../core/services/user.service';
import { AuthService } from '../../core/services/auth.service';

import { DoctorModel } from '../../core/models/doctor.model';
import { UserModel } from '../../core/models/user.model';

@Component({
  selector: 'app-doctor-details',
  standalone: false,
  templateUrl: './doctor-details.html',
  styleUrls: ['./doctor-details.css']
})
export class DoctorDetails implements OnInit {

  doctorId!: number;

  doctor: DoctorModel | null = null;
  doctorUser: UserModel | null = null;

  loading = true;
  errorMessage = '';

  role = '';

  get canBook(): boolean {
    return this.role === 'Patient';
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private doctorService: DoctorService,
    private userService: UserService,
    private authService: AuthService,
    private cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    const idParam = this.route.snapshot.paramMap.get('doctorId');

    if (!idParam) {
      this.router.navigate(['/find-doctors']);
      return;
    }

    this.doctorId = Number(idParam);

    this.loadDoctor();

  }


  loadDoctor(): void {

    this.loading = true;

    this.doctorService.getDoctorById(this.doctorId).subscribe({

      next: (response) => {

        this.doctor = response;

        this.userService.getAllUsers().subscribe({

          next: (users) => {

            this.doctorUser = users.find(u => u.userID === response.userID) ?? null;

            this.loading = false;

            this.cdr.detectChanges();

          },

          error: (err) => {

            console.error(err);

            this.loading = false;

            this.cdr.detectChanges();

          }

        });

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load doctor details.';

        this.cdr.detectChanges();

      }

    });

  }


  bookAppointment(): void {

    this.router.navigate(['/appointments'], {
      queryParams: { doctorId: this.doctorId }
    });

  }


  back(): void {

    this.router.navigate(['/find-doctors']);

  }

}
