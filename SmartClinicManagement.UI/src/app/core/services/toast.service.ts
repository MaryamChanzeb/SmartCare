import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface Toast {
  id: number;
  message: string;
  type: ToastType;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {

  private toastsSubject = new BehaviorSubject<Toast[]>([]);
  toasts$ = this.toastsSubject.asObservable();

  private nextId = 1;

  private show(message: string, type: ToastType, durationMs = 4000): void {

    const toast: Toast = { id: this.nextId++, message, type };

    this.toastsSubject.next([...this.toastsSubject.value, toast]);

    setTimeout(() => this.dismiss(toast.id), durationMs);

  }

  success(message: string, durationMs?: number): void {
    this.show(message, 'success', durationMs);
  }

  error(message: string, durationMs?: number): void {
    this.show(message, 'error', durationMs);
  }

  info(message: string, durationMs?: number): void {
    this.show(message, 'info', durationMs);
  }

  warning(message: string, durationMs?: number): void {
    this.show(message, 'warning', durationMs);
  }

  dismiss(id: number): void {
    this.toastsSubject.next(this.toastsSubject.value.filter(t => t.id !== id));
  }

}
