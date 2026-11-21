import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { Login } from './features/auth/login/login';
import { Register } from './features/auth/register/register';
import { ForgotPassword } from './features/auth/forgot-password/forgot-password';
import { ResetPassword } from './features/auth/reset-password/reset-password';
import { Dashboard } from './features/dashboard/dashboard';
import { Department } from './features/departments/department';
import { Users } from './features/users/users';
import { Roles } from './features/roles/roles';
import { Doctors } from './features/doctors/doctors';
import { DoctorDetails } from './features/doctor-details/doctor-details';
import { DoctorSchedule } from './features/doctor-schedule/doctor-schedule';
import { Patients } from './features/patients/patients';
import { PatientDetails } from './features/patient-details/patient-details';
import { PatientRegistration } from './features/patient-registration/patient-registration';
import { Appointments } from './features/appointments/appointments';
import { MedicalRecords } from './features/medical-records/medical-records';
import { Prescriptions } from './features/prescriptions/prescriptions';
import { Billing } from './features/billing/billing';
import { Payments } from './features/payments/payments';
import { Profile } from './features/profile/profile';
import { FindDoctors } from './features/find-doctors/find-doctors';
import { Unauthorized } from './features/unauthorized/unauthorized';
import { MainLayout } from './layout/main-layout/main-layout';

import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';

const ADMIN_ONLY = ['Admin', 'SuperAdmin'];
const ADMIN_RECEPTIONIST = ['Admin', 'SuperAdmin', 'Receptionist'];
const ADMIN_RECEPTIONIST_DOCTOR = ['Admin', 'SuperAdmin', 'Receptionist', 'Doctor'];

// Clinical *detail* (prescriptions) stays Doctor/Patient/Admin only.
const MEDICAL_ROLES = ['Admin', 'SuperAdmin', 'Doctor', 'Patient', 'Receptionist'];


const MEDICAL_RECORD_VIEW_ROLES = ['Admin', 'SuperAdmin', 'Doctor', 'Patient', 'Receptionist'];

const BILLING_ROLES = ['Admin', 'SuperAdmin', 'Receptionist', 'Accountant', 'Patient','Doctor'];
const APPOINTMENT_ROLES = ['Admin', 'SuperAdmin', 'Receptionist', 'Doctor', 'Patient'];
const PATIENT_ONLY = ['Patient'];

const routes: Routes = [

  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },

  {
    path: 'login',
    component: Login
  },

  {
    path: 'register',
    component: Register
  },

  {
    path: 'forgot-password',
    component: ForgotPassword
  },

  {
    path: 'reset-password',
    component: ResetPassword
  },

  {
    path: 'unauthorized',
    component: Unauthorized
  },

  {
    path: '',
    component: MainLayout,
    canActivate: [AuthGuard],

    children: [

      {
        path: 'dashboard',
        component: Dashboard
      },

      {
        path: 'profile',
        component: Profile
      },

      {
        path: 'users',
        component: Users,
        canActivate: [RoleGuard],
        data: { roles: ADMIN_ONLY }
      },

      {
        path: 'departments',
        component: Department,
        canActivate: [RoleGuard],
        data: { roles: ADMIN_ONLY }
      },

      {
        path: 'roles',
        component: Roles,
        canActivate: [RoleGuard],
        data: { roles: ADMIN_ONLY }
      },

      {
        path: 'doctors',
        component: Doctors,
        canActivate: [RoleGuard],
        data: { roles: ADMIN_ONLY }
      },

      {
        path: 'doctors/:doctorId',
        component: DoctorDetails
        // No 'roles' restriction — read-only professional info.
      },

      {
        path: 'doctor-schedule',
        component: DoctorSchedule,
        canActivate: [RoleGuard],
        data: { roles: ADMIN_RECEPTIONIST }
      },

      {
        path: 'find-doctors',
        component: FindDoctors,
        canActivate: [RoleGuard],
        data: { roles: PATIENT_ONLY }
      },

      {
        path: 'patients',
        component: Patients,
        canActivate: [RoleGuard],
        data: { roles: ADMIN_RECEPTIONIST_DOCTOR }
      },

      {
        path: 'patients/:patientId',
        component: PatientDetails,
        canActivate: [RoleGuard],
        data: { roles: ADMIN_RECEPTIONIST_DOCTOR }
      },

      {
        path: 'patient-registration',
        component: PatientRegistration,
        canActivate: [RoleGuard],
        data: { roles: ADMIN_RECEPTIONIST }
      },

      {
        path: 'appointments',
        component: Appointments,
        canActivate: [RoleGuard],
        data: { roles: APPOINTMENT_ROLES }
      },

      {
        path: 'medical-records',
        component: MedicalRecords,
        canActivate: [RoleGuard],
        data: { roles: MEDICAL_RECORD_VIEW_ROLES }
      },

      {
        path: 'prescriptions/:recordId',
        component: Prescriptions,
        canActivate: [RoleGuard],
        data: { roles: MEDICAL_ROLES }
      },

      {
        path: 'billing',
        component: Billing,
        canActivate: [RoleGuard],
        data: { roles: BILLING_ROLES }
      },

      {
        path: 'payments',
        component: Payments,
        canActivate: [RoleGuard],
        data: { roles: BILLING_ROLES }
      }

    ]

  },

  {
    path: '**',
    redirectTo: 'login'
  }

];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
