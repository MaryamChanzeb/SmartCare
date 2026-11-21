export interface AppointmentModel {
  appointmentID: number;
  doctorID: number;
  patientID: number;
  appointmentDate: string;   // DateOnly -> "yyyy-MM-dd"
  appointmentTime: string;   // TimeOnly -> "HH:mm:ss"
  appointmentType?: string;
  status: string;            // Scheduled | Confirmed | Completed | Cancelled | NoShow
  notes?: string;
}

export interface BookAppointmentModel {
  doctorID: number | null;
  patientID: number | null;
  appointmentDate: string;
  appointmentTime: string;
  appointmentType?: string;
  notes?: string;
}

export interface UpdateAppointmentModel {
  appointmentDate: string;
  appointmentTime: string;
  appointmentType?: string;
  status: string;
  notes?: string;
}
