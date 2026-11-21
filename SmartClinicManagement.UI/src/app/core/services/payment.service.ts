import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { PaymentModel, ReceivePaymentModel } from '../models/payment.model';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {

  private apiUrl = 'https://localhost:7006/api/Payments';

  constructor(private http: HttpClient) { }

  getAllPayments(): Observable<PaymentModel[]> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getPaymentById(id: number): Observable<PaymentModel> {
    return this.http.get<PaymentModel>(`${this.apiUrl}/${id}`);
  }

  getPaymentsByBill(billId: number): Observable<PaymentModel[]> {

    return this.http.get<any>(`${this.apiUrl}/bill/${billId}`).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  // Backend only supports receiving/recording a payment — there is no
  // update or delete endpoint for Payments.
  receivePayment(payment: ReceivePaymentModel): Observable<PaymentModel> {
    return this.http.post<PaymentModel>(this.apiUrl, payment);
  }

}
