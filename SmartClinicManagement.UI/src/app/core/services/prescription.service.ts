import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
  PrescriptionModel,
  CreatePrescriptionModel
} from '../models/prescription.model';

@Injectable({
  providedIn: 'root'
})
export class PrescriptionService {

  private apiUrl = 'https://localhost:7006/api/Prescriptions';

  constructor(private http: HttpClient) { }

  // NOTE: No "get all" endpoint — prescriptions are only ever fetched
  // scoped to the medical record they belong to.

  getByRecord(recordId: number): Observable<PrescriptionModel[]> {

    return this.http.get<any>(`${this.apiUrl}/record/${recordId}`).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getById(id: number): Observable<PrescriptionModel> {
    return this.http.get<PrescriptionModel>(`${this.apiUrl}/${id}`);
  }

  addPrescription(prescription: CreatePrescriptionModel): Observable<any> {
    return this.http.post(this.apiUrl, prescription);
  }

  updatePrescription(id: number, prescription: CreatePrescriptionModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, prescription);
  }

}
