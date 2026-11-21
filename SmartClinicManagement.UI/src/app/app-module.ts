import {
  NgModule,
  provideBrowserGlobalErrorListeners
} from '@angular/core';

import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule } from '@angular/common/http';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing-module';
import { HTTP_INTERCEPTORS } from '@angular/common/http';

import { AuthInterceptor } from './core/interceptors/auth.interceptor';

import { App } from './app';

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
import { ToastContainer } from './shared/toast-container/toast-container';
import { Navbar } from './layout/navbar/navbar';
import { Sidebar } from './layout/sidebar/sidebar';
import { Footer } from './layout/footer/footer';
import { MainLayout } from './layout/main-layout/main-layout';

@NgModule({

  declarations: [

    App,

    // Auth
    Login,
    Register,
    ForgotPassword,
    ResetPassword,

    // Features
    Dashboard,
    Department,
    Users,
    Roles,
    Doctors,
    DoctorDetails,
    DoctorSchedule,
    Patients,
    PatientDetails,
    PatientRegistration,
    Appointments,
    MedicalRecords,
    Prescriptions,
    Billing,
    Payments,
    Profile,
    FindDoctors,
    Unauthorized,
    ToastContainer,

    // Layout
    Navbar,
    Sidebar,
    Footer,
    MainLayout

  ],

  imports: [

    BrowserModule,

    AppRoutingModule,

    HttpClientModule,

    ReactiveFormsModule,

    FormsModule

  ],


  providers: [

    provideBrowserGlobalErrorListeners(),

    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }

  ],



  bootstrap: [
    App
  ]

})
export class AppModule { }
