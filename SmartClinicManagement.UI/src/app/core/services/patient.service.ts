import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { PatientModel, CreatePatientModel, RegisterPatientModel } from '../models/patient.model';

@Injectable({
  providedIn: 'root'
})
export class PatientService {

  private apiUrl = 'https://localhost:7006/api/Patients';

  constructor(private http: HttpClient) { }

  getAllPatients(): Observable<PatientModel[]> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getPatientById(id: number): Observable<PatientModel> {
    return this.http.get<PatientModel>(`${this.apiUrl}/${id}`);
  }

  getMyProfile(): Observable<PatientModel> {
    return this.http.get<PatientModel>(`${this.apiUrl}/me`);
  }



  createPatient(patient: CreatePatientModel): Observable<any> {
    return this.http.post(this.apiUrl, patient);
  }

  // Combined registration: creates the User account and Patient profile
  // together in a single backend call.
  registerPatient(patient: RegisterPatientModel): Observable<any> {
    return this.http.post(`${this.apiUrl}/register`, patient);
  }

  updatePatient(id: number, patient: CreatePatientModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, patient);
  }

  deletePatient(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
