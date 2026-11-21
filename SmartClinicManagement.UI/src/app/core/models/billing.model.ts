export interface BillingModel {
  billID: number;
  appointmentID: number;
  amount: number;
  discount: number;
  tax: number;
  totalAmount: number;
  paymentMethod: string;
  paymentStatus: string;   // Pending | Paid (per Billing entity default)
}

export interface CreateBillingModel {
  appointmentID: number | null;
  amount: number;
  discount: number;
  tax: number;
  paymentMethod: string;
}

export interface MarkBillPaidModel {
  paymentMethod: string;
}
