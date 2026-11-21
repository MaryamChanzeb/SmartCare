export interface UserModel {
  userID: number;
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  gender?: string;
  dateOfBirth?: string;
  status: string;
  departmentName?: string;
  roleName?: string;
}

export interface CreateUserModel {
  firstName: string;
  lastName: string;
  email: string;
  phone?: string;
  gender?: string;
  dateOfBirth?: string;
  departmentID?: number | null;
  roleID?: number | null;
}

export interface UpdateUserModel extends CreateUserModel {
  status: string;
}
