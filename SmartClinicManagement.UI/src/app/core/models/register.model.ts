
export type RegistrationRole = 'Patient' | 'Doctor' | 'Receptionist';

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  gender?: string;
  dateOfBirth?: string | null;
  roleName: RegistrationRole;
  password: string;
  confirmPassword: string;

  // Only relevant when roleName === 'Doctor'.
  licenseNumber?: string;
  specialization?: string;
}

export interface RegisterResponse {
  success: boolean;
  message: string;
}
