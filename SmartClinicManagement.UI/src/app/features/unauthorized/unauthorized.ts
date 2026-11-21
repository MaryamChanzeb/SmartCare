import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-unauthorized',
  standalone: false,
  templateUrl: './unauthorized.html',
  styleUrls: ['./unauthorized.css']
})
export class Unauthorized {

  constructor(private router: Router) { }

  goToDashboard(): void {
    this.router.navigate(['/dashboard']);
  }

}
