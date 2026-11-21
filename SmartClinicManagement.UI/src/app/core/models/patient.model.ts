export interface PatientModel {
  patientID: number;
  userID: number;
  bloodGroup?: string;
  height?: number;
  weight?: number;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  allergies?: string;
  medicalHistory?: string;
  status: string;
  primaryDoctorID?: number | null;
}

export interface CreatePatientModel {
  userID: number | null;
  bloodGroup?: string;
  height?: number | null;
  weight?: number | null;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  allergies?: string;
  medicalHistory?: string;
  status: string;
  primaryDoctorID?: number | null;
}

// Combined registration payload — creates the User account and Patient
// profile together in one call, with an optional assigned doctor.
export interface RegisterPatientModel {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  gender?: string;
  dateOfBirth?: string;
  bloodGroup?: string;
  height?: number | null;
  weight?: number | null;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  allergies?: string;
  medicalHistory?: string;
  status: string;
  doctorID?: number | null;
}
