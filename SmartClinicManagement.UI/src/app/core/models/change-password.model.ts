export interface ChangePasswordModel {
  userID: number;
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}
