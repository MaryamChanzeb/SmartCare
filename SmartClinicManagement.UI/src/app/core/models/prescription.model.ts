export interface PrescriptionModel {
  prescriptionID: number;
  recordID: number;
  medicineName?: string;
  dosage?: string;
  frequency?: string;
  duration?: string;
  instructions?: string;
}

export interface CreatePrescriptionModel {
  recordID: number;
  medicineName: string;
  dosage?: string;
  frequency?: string;
  duration?: string;
  instructions?: string;
}
