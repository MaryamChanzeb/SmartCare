import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';
import { Router } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { RegisterRequest } from '../../../core/models/register.model';

// Public registration can only ever create these three roles.
// Admin/SuperAdmin accounts are created by an existing Admin, never here.
const REGISTRATION_ROLES = ['Patient', 'Doctor', 'Receptionist'] as const;

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {

  const password = control.get('password')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return password && confirmPassword && password !== confirmPassword
    ? { passwordMismatch: true }
    : null;

}

@Component({
  selector: 'app-register',
  standalone: false,
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {

  roles = REGISTRATION_ROLES;

  registerForm: FormGroup;

  submitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastService: ToastService,
    private router: Router
  ) {

    this.registerForm = this.fb.group({

      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      phone: [''],
      gender: [''],
      dateOfBirth: [''],
      roleName: ['', Validators.required],

      // Only required/used when roleName === 'Doctor'.
      licenseNumber: [''],
      specialization: [''],

      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]

    }, { validators: passwordsMatchValidator });

  }

  get isDoctorRole(): boolean {
    return this.registerForm.get('roleName')?.value === 'Doctor';
  }

  onSubmit(): void {

    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();

      if (this.registerForm.hasError('passwordMismatch')) {
        this.toastService.error('Password and confirmation do not match.');
      }

      return;
    }

    this.submitting = true;

    const formValue = this.registerForm.value;

    const request: RegisterRequest = {
      firstName: formValue.firstName,
      lastName: formValue.lastName,
      email: formValue.email,
      phone: formValue.phone || undefined,
      gender: formValue.gender || undefined,
      dateOfBirth: formValue.dateOfBirth || undefined,
      roleName: formValue.roleName,
      password: formValue.password,
      confirmPassword: formValue.confirmPassword,
      licenseNumber: this.isDoctorRole ? formValue.licenseNumber : undefined,
      specialization: this.isDoctorRole ? formValue.specialization : undefined
    };

    this.authService.register(request).subscribe({

      next: (response) => {

        this.submitting = false;
        this.toastService.success(response.message || 'Account created successfully.');
        this.router.navigate(['/login']);

      },

      error: (error) => {

        this.submitting = false;
        console.error('Register Error:', error);

        const message = error?.error?.message || 'Unable to create your account. Please try again.';
        this.toastService.error(message);

      }

    });

  }

}
