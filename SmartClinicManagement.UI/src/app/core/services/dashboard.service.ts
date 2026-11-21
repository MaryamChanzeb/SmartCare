import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { DashboardModel } from '../models/dashboard.model';


function unwrapValues(value: any): any {
  return value && Array.isArray(value.$values) ? value.$values : value;
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private apiUrl = 'https://localhost:7006/api/Dashboard';

  constructor(private http: HttpClient) { }

  getDashboard(): Observable<DashboardModel> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response => {

        const dashboard = unwrapValues(response);

        return {
          ...dashboard,
          monthlyRevenue: unwrapValues(dashboard?.monthlyRevenue) ?? []
        } as DashboardModel;

      })
    );

  }

}
