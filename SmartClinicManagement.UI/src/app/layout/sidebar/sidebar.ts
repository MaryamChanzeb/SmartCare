import { Component } from '@angular/core';

import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-sidebar',
  standalone: false,
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css'
})
export class Sidebar {

  role: string;

  constructor(private authService: AuthService) {

    this.role = this.authService.getRole();

  }

  get isAdmin(): boolean {
    return this.role === 'Admin' || this.role === 'SuperAdmin';
  }

  get isDoctor(): boolean {
    return this.role === 'Doctor';
  }

  get isReceptionist(): boolean {
    return this.role === 'Receptionist';
  }

  get isPatient(): boolean {
    return this.role === 'Patient';
  }

}
