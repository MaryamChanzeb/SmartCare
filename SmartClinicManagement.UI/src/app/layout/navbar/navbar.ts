import { Component, ElementRef, HostListener } from '@angular/core';
import { Router } from '@angular/router';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';

import { AuthService } from '../../core/services/auth.service';
import { ChangePasswordModel } from '../../core/models/change-password.model';

@Component({
  selector: 'app-navbar',
  standalone: false,
  templateUrl: './navbar.html',
  styleUrl: './navbar.css'
})
export class Navbar {

  fullName = localStorage.getItem('fullName') || 'User';

  role = localStorage.getItem('role') || 'User';

  showMenu = false;

  // Change Password modal state
  showChangePasswordModal = false;
  changePasswordForm: FormGroup;
  changePasswordSaving = false;
  changePasswordError = '';
  changePasswordSuccess = '';

  constructor(
    private router: Router,
    private authService: AuthService,
    private fb: FormBuilder,
    private elementRef: ElementRef
  ) {

    this.changePasswordForm = this.fb.group({

      currentPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required]

    });

  }

  toggleMenu(): void {

    this.showMenu = !this.showMenu;

  }
  // BUG FIX: the dropdown previously only closed by re-clicking the
  // profile button — clicking anywhere else on the page left it open.
  // This listens on the whole document and closes the menu whenever the
  // click target is outside this component's own host element, which
  // covers every screen size and every page since app-navbar is always
  // present in the shared layout.
  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {

    if (!this.showMenu) return;

    const clickedInsideNavbar = this.elementRef.nativeElement.contains(event.target);

    if (!clickedInsideNavbar) {
      this.showMenu = false;
    }

  }



  // BUG FIX: this button previously only did `showMenu = false` — it never
  // navigated anywhere, so "My Profile" appeared to do nothing.
  goToProfile(): void {

    this.showMenu = false;

    this.router.navigate(['/profile']);

  }


  // BUG FIX: same issue — this button had no click behavior beyond closing
  // the dropdown. It now opens a working Change Password form that calls
  // the existing POST /api/Auth/ChangePassword endpoint.
  openChangePassword(): void {

    this.showMenu = false;

    this.changePasswordForm.reset();
    this.changePasswordError = '';
    this.changePasswordSuccess = '';

    this.showChangePasswordModal = true;

  }


  closeChangePasswordModal(): void {

    this.showChangePasswordModal = false;

  }


  submitChangePassword(): void {

    if (this.changePasswordForm.invalid) {
      this.changePasswordForm.markAllAsTouched();
      return;
    }

    const formValue = this.changePasswordForm.value;

    if (formValue.newPassword !== formValue.confirmPassword) {

      this.changePasswordError = 'New password and confirmation do not match.';

      return;

    }

    const userId = this.authService.getUserId();

    if (!userId) {

      this.changePasswordError = 'Unable to determine your account — please log in again.';

      return;

    }

    this.changePasswordSaving = true;
    this.changePasswordError = '';

    const payload: ChangePasswordModel = {

      userID: userId,
      currentPassword: formValue.currentPassword,
      newPassword: formValue.newPassword,
      confirmPassword: formValue.confirmPassword

    };

    this.authService.changePassword(payload).subscribe({

      next: () => {

        this.changePasswordSaving = false;
        this.changePasswordSuccess = 'Password changed successfully.';
        this.changePasswordForm.reset();

      },

      error: (error) => {

        console.error('Change Password Error:', error);

        this.changePasswordSaving = false;

        this.changePasswordError =
          error?.error?.Message || 'Current password is incorrect or passwords do not match.';

      }

    });

  }


  logout(): void {

    this.authService.logout();

  }

}
