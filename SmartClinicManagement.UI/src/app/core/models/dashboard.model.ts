export interface MonthlyRevenuePoint {
  month: string;
  revenue: number;
}export interface DashboardModel {
  totalUsers: number;
  totalDepartments: number;
  totalRoles: number;
  totalDoctors: number;
  totalPatients: number;
  totalAppointments: number;
  totalMedicalRecords: number;
  totalPrescriptions: number;
  totalBills: number;
  totalPayments: number;
  totalRevenue: number;
  pendingBills: number;
  paidBills: number;
  scheduledAppointments: number;
  completedAppointments: number;
  cancelledAppointments: number;
  monthlyRevenue: MonthlyRevenuePoint[];
}
