import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { DoctorModel, CreateDoctorModel, RegisterDoctorModel } from '../models/doctor.model';

@Injectable({
  providedIn: 'root'
})
export class DoctorService {

  private apiUrl = 'https://localhost:7006/api/Doctors';

  constructor(private http: HttpClient) { }

  getAllDoctors(): Observable<DoctorModel[]> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getDoctorById(id: number): Observable<DoctorModel> {
    return this.http.get<DoctorModel>(`${this.apiUrl}/${id}`);
  }

  createDoctor(doctor: CreateDoctorModel): Observable<any> {
    return this.http.post(this.apiUrl, doctor);
  }

  // Combined registration: creates the User account and Doctor profile
  // together in a single backend call.
  registerDoctor(doctor: RegisterDoctorModel): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, doctor);
  }

  updateDoctor(id: number, doctor: CreateDoctorModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, doctor);
  }

  deleteDoctor(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
