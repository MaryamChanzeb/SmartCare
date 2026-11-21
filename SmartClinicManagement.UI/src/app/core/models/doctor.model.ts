export interface DoctorModel {
  doctorID: number;
  userID: number;
  licenseNumber: string;
  specialization: string;
  qualification?: string;
  experienceYears: number;
  consultationFee: number;
  roomNumber?: string;
  joiningDate?: string;
  status: string;
}

export interface CreateDoctorModel {
  userID: number | null;
  licenseNumber: string;
  specialization: string;
  qualification?: string;
  experienceYears: number;
  consultationFee: number;
  roomNumber?: string;
  joiningDate?: string;
  status: string;
}

export interface RegisterDoctorModel {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  gender?: string;
  dateOfBirth?: string;
  departmentID?: number | null;
  licenseNumber: string;
  specialization: string;
  qualification?: string;
  experienceYears: number;
  consultationFee: number;
  roomNumber?: string;
  joiningDate?: string;
  status: string;
}
