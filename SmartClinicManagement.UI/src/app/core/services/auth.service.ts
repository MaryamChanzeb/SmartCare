import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Router } from '@angular/router';

import { environment } from '../../../environments/environment';
import { LoginRequest } from '../models/login-request';
import { LoginResponse } from '../models/login-response';
import { ChangePasswordModel } from '../models/change-password.model';
import { RegisterRequest, RegisterResponse } from '../models/register.model';
import {
  ForgotPasswordRequest,
  ResetPasswordRequest,
  AuthActionResponse
} from '../models/forgot-password.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  private apiUrl = `${environment.apiUrl}/Auth`;

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${this.apiUrl}/Login`,
      request
    );
  }


  changePassword(payload: ChangePasswordModel): Observable<any> {
    return this.http.post(`${this.apiUrl}/ChangePassword`, payload);
  }


  register(request: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(
      `${this.apiUrl}/Register`,
      request
    );
  }


  forgotPassword(request: ForgotPasswordRequest): Observable<AuthActionResponse> {
    return this.http.post<AuthActionResponse>(
      `${this.apiUrl}/ForgotPassword`,
      request
    );
  }


  resetPassword(request: ResetPasswordRequest): Observable<AuthActionResponse> {
    return this.http.post<AuthActionResponse>(
      `${this.apiUrl}/ResetPassword`,
      request
    );
  }


  isLoggedIn(): boolean {
    return !!localStorage.getItem('token');
  }


  getToken(): string | null {
    return localStorage.getItem('token');
  }


  getRole(): string {
    return localStorage.getItem('role') ?? '';
  }


  getUserId(): number | null {

    const id = localStorage.getItem('userId');

    return id ? Number(id) : null;

  }


  getFullName(): string {
    return localStorage.getItem('fullName') ?? '';
  }


  logout(): void {

    localStorage.removeItem('token');
    localStorage.removeItem('fullName');
    localStorage.removeItem('role');
    localStorage.removeItem('userId');

    this.router.navigate(['/login']);

  }

}
