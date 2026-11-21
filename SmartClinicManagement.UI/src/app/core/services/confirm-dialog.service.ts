import { Injectable } from '@angular/core';

export interface ConfirmDialogOptions {
  title?: string;
  confirmLabel?: string;
  cancelLabel?: string;

  danger?: boolean;
}


@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {

  private static stylesInjected = false;

  confirm(message: string, options?: ConfirmDialogOptions): Promise<boolean> {

    this.ensureStylesInjected();

    const title = options?.title ?? 'Are you sure?';
    const confirmLabel = options?.confirmLabel ?? 'Delete';
    const cancelLabel = options?.cancelLabel ?? 'Cancel';
    const danger = options?.danger ?? true;

    return new Promise<boolean>((resolve) => {

      const overlay = document.createElement('div');
      overlay.className = 'app-confirm-overlay';

      const box = document.createElement('div');
      box.className = 'app-confirm-box';

      box.innerHTML = `
        <h3 class="app-confirm-title"></h3>
        <p class="app-confirm-message"></p>
        <div class="app-confirm-actions">
          <button type="button" class="app-confirm-btn app-confirm-cancel"></button>
          <button type="button" class="app-confirm-btn ${danger ? 'app-confirm-danger' : 'app-confirm-primary'}"></button>
        </div>
      `;

      // textContent (not innerHTML) for user-supplied strings — avoids any
      // risk of the message/title being interpreted as markup.
      (box.querySelector('.app-confirm-title') as HTMLElement).textContent = title;
      (box.querySelector('.app-confirm-message') as HTMLElement).textContent = message;

      const cancelBtn = box.querySelector('.app-confirm-cancel') as HTMLButtonElement;
      const confirmBtn = box.querySelector(danger ? '.app-confirm-danger' : '.app-confirm-primary') as HTMLButtonElement;

      cancelBtn.textContent = cancelLabel;
      confirmBtn.textContent = confirmLabel;

      overlay.appendChild(box);
      document.body.appendChild(overlay);

      const cleanup = (result: boolean) => {
        document.body.removeChild(overlay);
        document.removeEventListener('keydown', onKeydown);
        resolve(result);
      };

      const onKeydown = (e: KeyboardEvent) => {
        if (e.key === 'Escape') cleanup(false);
      };

      cancelBtn.addEventListener('click', () => cleanup(false));
      confirmBtn.addEventListener('click', () => cleanup(true));
      overlay.addEventListener('click', (e) => {
        if (e.target === overlay) cleanup(false);
      });
      document.addEventListener('keydown', onKeydown);

      confirmBtn.focus();

    });

  }

  private ensureStylesInjected(): void {

    if (ConfirmDialogService.stylesInjected) return;

    const style = document.createElement('style');

    style.textContent = `
      .app-confirm-overlay {
        position: fixed;
        inset: 0;
        background: rgba(0, 0, 0, 0.45);
        display: flex;
        align-items: center;
        justify-content: center;
        z-index: 2000;
      }

      .app-confirm-box {
        background: #fff;
        border-radius: 10px;
        padding: 24px;
        width: 90%;
        max-width: 380px;
        box-shadow: 0 12px 40px rgba(0, 0, 0, 0.25);
        text-align: center;
      }

      .app-confirm-title {
        margin: 0 0 10px 0;
        font-size: 18px;
        font-weight: 700;
        color: #212529;
      }

      .app-confirm-message {
        margin: 0 0 20px 0;
        font-size: 14px;
        color: #495057;
      }

      .app-confirm-actions {
        display: flex;
        justify-content: center;
        gap: 10px;
      }

      .app-confirm-btn {
        border: none;
        border-radius: 6px;
        padding: 8px 18px;
        font-size: 14px;
        font-weight: 600;
        cursor: pointer;
      }

      .app-confirm-cancel {
        background: #e9ecef;
        color: #212529;
      }

      .app-confirm-cancel:hover {
        background: #dee2e6;
      }

      .app-confirm-danger {
        background: #dc3545;
        color: #fff;
      }

      .app-confirm-danger:hover {
        background: #bb2d3b;
      }

      .app-confirm-primary {
        background: #0d6efd;
        color: #fff;
      }

      .app-confirm-primary:hover {
        background: #0a58ca;
      }
    `;

    document.head.appendChild(style);

    ConfirmDialogService.stylesInjected = true;

  }

}
