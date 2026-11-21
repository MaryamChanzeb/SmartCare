import { Injectable } from '@angular/core';
import {
  CanActivate,
  ActivatedRouteSnapshot,
  Router
} from '@angular/router';

import { AuthService } from '../services/auth.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {

  constructor(
    private authService: AuthService,
    private router: Router
  ) { }

  canActivate(route: ActivatedRouteSnapshot): boolean {

    if (!this.authService.isLoggedIn()) {
      this.router.navigate(['/login']);
      return false;
    }

    const allowedRoles: string[] = route.data?.['roles'] ?? [];

    // No 'roles' configured on the route = any logged-in user can view it
    // (e.g. Dashboard, My Profile).
    if (allowedRoles.length === 0) {
      return true;
    }

    const currentRole = this.authService.getRole();

    // SuperAdmin always passes, regardless of which roles a route lists.
    if (currentRole === 'SuperAdmin') {
      return true;
    }

    if (allowedRoles.includes(currentRole)) {
      return true;
    }

    this.router.navigate(['/unauthorized']);

    return false;

  }

}
