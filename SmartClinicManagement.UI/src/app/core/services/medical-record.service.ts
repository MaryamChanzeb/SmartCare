import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
  MedicalRecordModel,
  CreateMedicalRecordModel
} from '../models/medical-record.model';

@Injectable({
  providedIn: 'root'
})
export class MedicalRecordService {

  private apiUrl = 'https://localhost:7006/api/MedicalRecords';

  constructor(private http: HttpClient) { }

  // GET /api/MedicalRecords (no id) returns all completed checkup
  // records — used by Receptionist/Admin. Doctor/Patient still use the
  // scoped lookups below.

  // All completed checkup records — used by Receptionist/Admin.
  getAll(): Observable<MedicalRecordModel[]> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getByPatient(patientId: number): Observable<MedicalRecordModel[]> {

    return this.http.get<any>(`${this.apiUrl}/patient/${patientId}`).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getByDoctor(doctorId: number): Observable<MedicalRecordModel[]> {

    return this.http.get<any>(`${this.apiUrl}/doctor/${doctorId}`).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getById(id: number): Observable<MedicalRecordModel> {
    return this.http.get<MedicalRecordModel>(`${this.apiUrl}/${id}`);
  }

  createRecord(record: CreateMedicalRecordModel): Observable<any> {
    return this.http.post(this.apiUrl, record);
  }

  updateRecord(id: number, record: CreateMedicalRecordModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, record);
  }

}
