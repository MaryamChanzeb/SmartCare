export interface RoleModel {
  roleID: number;
  roleName: string;
  description?: string;
}

export interface CreateRoleModel {
  roleName: string;
  description?: string;
}
