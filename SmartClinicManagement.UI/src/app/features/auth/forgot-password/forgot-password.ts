import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { timeout } from 'rxjs/operators';

import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-forgot-password',
  standalone: false,
  templateUrl: './forgot-password.html',
  styleUrl: './forgot-password.css'
})
export class ForgotPassword {

  forgotForm: FormGroup;

  submitting = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastService: ToastService
  ) {

    this.forgotForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]]
    });

  }

  onSubmit(): void {

    if (this.forgotForm.invalid) {
      this.forgotForm.markAllAsTouched();
      return;
    }

    this.submitting = true;

    this.authService.forgotPassword(this.forgotForm.value)
      // A hung request (e.g. the backend blocking on the outgoing SMTP
      // call with no server-side timeout of its own) previously left
      // this Observable never emitting next OR error, so `submitting`
      // never got reset and the button stayed on "Sending..." forever.
      // This client-side timeout guarantees the UI always resolves one
      // way or another within 20s.
      .pipe(
        timeout(20000)
      )
      .subscribe({

        next: (response) => {

          this.submitting = false;
          this.submitted = true;
          this.toastService.success(response.message);

        },

        error: (error) => {

          this.submitting = false;
          console.error('Forgot Password Error:', error);

          const isTimeout = error?.name === 'TimeoutError';

          this.toastService.error(
            isTimeout
              ? 'The server is taking too long to respond. Please try again in a moment.'
              : (error?.error?.message || 'Something went wrong. Please try again.')
          );

        }

      });

  }

}
