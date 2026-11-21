import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, forkJoin } from 'rxjs';
import { filter, take } from 'rxjs/operators';

import { AuthService } from './auth.service';
import { UserService } from './user.service';
import { DoctorService } from './doctor.service';
import { PatientService } from './patient.service';

import { UserModel } from '../models/user.model';
import { DoctorModel } from '../models/doctor.model';
import { PatientModel } from '../models/patient.model';



@Injectable({
  providedIn: 'root'
})
export class CurrentUserService {

  private loaded$ = new BehaviorSubject<boolean>(false);

  currentUser: UserModel | null = null;
  currentDoctor: DoctorModel | null = null;
  currentPatient: PatientModel | null = null;

  constructor(
    private authService: AuthService,
    private userService: UserService,
    private doctorService: DoctorService,
    private patientService: PatientService
  ) { }


  // Safe to call from every component that needs it — only fetches once
  // per session; later calls just wait for/return the cached result.

  ensureLoaded(): Observable<boolean> {

    if (!this.loaded$.value && this.authService.isLoggedIn()) {

      this.load();

    }

    return this.loaded$.asObservable().pipe(
      filter(loaded => loaded === true),
      take(1)
    );

  }



  reset(): void {

    this.currentUser = null;
    this.currentDoctor = null;
    this.currentPatient = null;

    this.loaded$.next(false);

  }


  private load(): void {

    const userId = this.authService.getUserId();

    if (!userId) {
      this.loaded$.next(true);
      return;
    }

    forkJoin({
      users: this.userService.getAllUsers(),
      doctors: this.doctorService.getAllDoctors(),
      patients: this.patientService.getAllPatients()
    }).subscribe({

      next: ({ users, doctors, patients }) => {

        this.currentUser = users.find(u => u.userID === userId) ?? null;
        this.currentDoctor = doctors.find(d => d.userID === userId) ?? null;
        this.currentPatient = patients.find(p => p.userID === userId) ?? null;

        this.loaded$.next(true);

      },

      error: (err) => {

        console.error('CurrentUserService failed to load:', err);

        this.loaded$.next(true);

      }

    });

  }


  getDoctorId(): number | null {
    return this.currentDoctor ? this.currentDoctor.doctorID : null;
  }


  getPatientId(): number | null {
    return this.currentPatient ? this.currentPatient.patientID : null;
  }

}
