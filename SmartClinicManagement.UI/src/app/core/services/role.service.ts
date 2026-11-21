import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';

import { RoleModel, CreateRoleModel } from '../models/role.model';

@Injectable({
  providedIn: 'root'
})
export class RoleService {

  private apiUrl = 'https://localhost:7006/api/Roles';

  constructor(private http: HttpClient) { }

  getAllRoles(): Observable<RoleModel[]> {

    return this.http.get<any>(this.apiUrl).pipe(
      map(response =>
        response && Array.isArray(response.$values)
          ? response.$values
          : response
      )
    );

  }

  getRoleById(id: number): Observable<RoleModel> {
    return this.http.get<RoleModel>(`${this.apiUrl}/${id}`);
  }

  createRole(role: CreateRoleModel): Observable<any> {
    return this.http.post(this.apiUrl, role);
  }

  updateRole(id: number, role: CreateRoleModel): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, role);
  }

  deleteRole(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
