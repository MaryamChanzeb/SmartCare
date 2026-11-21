import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { DepartmentModel } from '../models/department.model';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {

  private apiUrl = 'https://localhost:7006/api/Departments';

  constructor(private http: HttpClient) { }

  getAllDepartments(): Observable<DepartmentModel[]> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response => {
        // Handles .NET's $id/$values wrapper if present,
        // otherwise passes plain arrays straight through
        if (response && Array.isArray(response.$values)) {
          return response.$values;
        }
        return response;
      })
    );

  }

  createDepartment(department: DepartmentModel): Observable<DepartmentModel> {
    return this.http.post<DepartmentModel>(this.apiUrl, department);
  }

  updateDepartment(id: number, department: DepartmentModel): Observable<DepartmentModel> {
    return this.http.put<DepartmentModel>(`${this.apiUrl}/${id}`, department);
  }

  deleteDepartment(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
