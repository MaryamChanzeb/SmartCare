import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

function passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {

  const newPassword = control.get('newPassword')?.value;
  const confirmPassword = control.get('confirmPassword')?.value;

  return newPassword && confirmPassword && newPassword !== confirmPassword
    ? { passwordMismatch: true }
    : null;

}

@Component({
  selector: 'app-reset-password',
  standalone: false,
  templateUrl: './reset-password.html',
  styleUrl: './reset-password.css'
})
export class ResetPassword implements OnInit {

  resetForm: FormGroup;

  token = '';
  tokenMissing = false;
  submitting = false;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private authService: AuthService,
    private toastService: ToastService
  ) {

    this.resetForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]
    }, { validators: passwordsMatchValidator });

  }

  ngOnInit(): void {

    this.token = this.route.snapshot.queryParamMap.get('token') ?? '';

    if (!this.token) {
      this.tokenMissing = true;
    }

  }

  onSubmit(): void {

    if (this.resetForm.invalid || !this.token) {

      this.resetForm.markAllAsTouched();

      if (this.resetForm.hasError('passwordMismatch')) {
        this.toastService.error('New password and confirmation do not match.');
      }

      return;

    }

    this.submitting = true;

    this.authService.resetPassword({
      token: this.token,
      newPassword: this.resetForm.value.newPassword,
      confirmPassword: this.resetForm.value.confirmPassword
    }).subscribe({

      next: (response) => {

        this.submitting = false;
        this.toastService.success(response.message || 'Password reset successfully.');
        this.router.navigate(['/login']);

      },

      error: (error) => {

        this.submitting = false;
        console.error('Reset Password Error:', error);

        const message = error?.error?.message || 'This reset link is invalid or has expired.';
        this.toastService.error(message);

      }

    });

  }

}
