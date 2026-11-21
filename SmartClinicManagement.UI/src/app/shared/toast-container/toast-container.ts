import { Component } from '@angular/core';
import { Observable } from 'rxjs';

import { ToastService, Toast } from '../../core/services/toast.service';

@Component({
  selector: 'app-toast-container',
  standalone: false,
  templateUrl: './toast-container.html',
  styleUrls: ['./toast-container.css']
})
export class ToastContainer {

  toasts$: Observable<Toast[]>;

  constructor(private toastService: ToastService) {
    this.toasts$ = this.toastService.toasts$;
  }

  dismiss(id: number): void {
    this.toastService.dismiss(id);
  }

  iconFor(type: string): string {

    switch (type) {
      case 'success': return 'bi-check-circle-fill';
      case 'error': return 'bi-x-circle-fill';
      case 'warning': return 'bi-exclamation-triangle-fill';
      default: return 'bi-info-circle-fill';
    }

  }

}
