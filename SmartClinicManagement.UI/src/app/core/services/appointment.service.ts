import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
  AppointmentModel,
  BookAppointmentModel,
  UpdateAppointmentModel
} from '../models/appointment.model';

@Injectable({
  providedIn: 'root'
})
export class AppointmentService {

  private apiUrl = 'https://localhost:7006/api/Appointments';

  constructor(private http: HttpClient) { }

  getAllAppointments(): Observable<AppointmentModel[]> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getAppointmentById(id: number): Observable<AppointmentModel> {
    return this.http.get<AppointmentModel>(`${this.apiUrl}/${id}`);
  }

  bookAppointment(appointment: BookAppointmentModel): Observable<any> {
    return this.http.post(this.apiUrl, appointment);
  }

  // Used for reschedule, confirm, and complete — the backend only exposes
  // a single generic update, so those actions send the full object back
  // with AppointmentDate/AppointmentTime/Status changed as needed.
  updateAppointment(id: number, appointment: UpdateAppointmentModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, appointment);
  }

  cancelAppointment(id: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/cancel`, {});
  }

}
