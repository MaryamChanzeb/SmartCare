import { Component } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { CurrentUserService } from '../../../core/services/current-user.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class Login {

  loginForm: FormGroup;

  submitting = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private currentUserService: CurrentUserService,
    private toastService: ToastService,
    private router: Router
  ) {

    this.loginForm = this.fb.group({

      email: [
        '',
        [
          Validators.required,
          Validators.email
        ]
      ],

      password: [
        '',
        [
          Validators.required,
          Validators.minLength(6)
        ]
      ],

      rememberMe: [false]

    });

  }

  onSubmit() {

    if (this.loginForm.invalid)
      return;

    this.submitting = true;

    this.authService.login(this.loginForm.value)
      .subscribe({

        next: (response) => {

          this.submitting = false;

          if (response.success) {

            localStorage.setItem('token', response.token);
            localStorage.setItem('fullName', response.fullName);
            localStorage.setItem('role', response.role);
            localStorage.setItem('userId', response.userID.toString());


            this.currentUserService.reset();

            this.toastService.success(response.message || 'Login successful.');

            this.router.navigate(['/dashboard']);

          } else {

            this.toastService.error(response.message || 'Invalid Email or Password.');

          }

        },

        error: (error) => {

          this.submitting = false;

          console.error(error);

          const message = error?.error?.message || 'Invalid Email or Password.';
          this.toastService.error(message);

        }

      });

  }

}
