export interface PaymentModel {
  paymentID: number;
  billID: number;
  transactionReference?: string;
  amount: number;
  paymentDate: string;
  status: string;   // Pending | Success | Failed (per Payment entity)
}

export interface ReceivePaymentModel {
  billID: number | null;
  amount: number;
  transactionReference?: string;
  status: string;
}
