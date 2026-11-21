export interface MedicalRecordModel {
  recordID: number;
  patientID: number;
  doctorID: number;
  diagnosis?: string;
  symptoms?: string;
  treatment?: string;
  notes?: string;
  visitDate?: string;
}

export interface CreateMedicalRecordModel {
  patientID: number | null;
  doctorID: number | null;
  diagnosis?: string;
  symptoms?: string;
  treatment?: string;
  notes?: string;
  visitDate?: string;
}
