import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import {
  BillingModel,
  CreateBillingModel,
  MarkBillPaidModel
} from '../models/billing.model';

@Injectable({
  providedIn: 'root'
})
export class BillingService {

  private apiUrl = 'https://localhost:7006/api/Billing';

  constructor(private http: HttpClient) { }

  getAllBills(): Observable<BillingModel[]> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getBillById(id: number): Observable<BillingModel> {
    return this.http.get<BillingModel>(`${this.apiUrl}/${id}`);
  }

  createBill(bill: CreateBillingModel): Observable<BillingModel> {
    return this.http.post<BillingModel>(this.apiUrl, bill);
  }

  updateBill(id: number, bill: CreateBillingModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, bill);
  }

  markAsPaid(id: number, payload: MarkBillPaidModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}/pay`, payload);
  }

}
