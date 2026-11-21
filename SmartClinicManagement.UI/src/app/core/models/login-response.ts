export interface LoginResponse {

  success: boolean;

  message: string;

  token: string;

  expiresAt: string;

  userID: number;

  fullName: string;

  role: string;

}
