import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { PrescriptionService } from '../../core/services/prescription.service';
import { MedicalRecordService } from '../../core/services/medical-record.service';
import { AuthService } from '../../core/services/auth.service';

import {
  PrescriptionModel,
  CreatePrescriptionModel
} from '../../core/models/prescription.model';
import { MedicalRecordModel } from '../../core/models/medical-record.model';

@Component({
  selector: 'app-prescriptions',
  standalone: false,
  templateUrl: './prescriptions.html',
  styleUrls: ['./prescriptions.css']
})
export class Prescriptions implements OnInit {

  recordId!: number;
  record: MedicalRecordModel | null = null;

  prescriptions: PrescriptionModel[] = [];

  prescriptionForm: FormGroup;

  showForm = false;
  loading = false;
  errorMessage = '';

  editingId: number | null = null;

  // Route access was widened to let Receptionist view a patient's
  // prescriptions (front desk needs this for billing/insurance queries),
  // but Receptionist should still be read-only here — only Doctor/Admin
  // can add or edit prescriptions.
  role = '';
  get canManagePrescriptions(): boolean {
    return this.role === 'Doctor' || this.role === 'Admin' || this.role === 'SuperAdmin';
  }

  constructor(
    private fb: FormBuilder,
    private prescriptionService: PrescriptionService,
    private medicalRecordService: MedicalRecordService,
    private authService: AuthService,
    private route: ActivatedRoute,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {

    this.prescriptionForm = this.fb.group({
      medicineName: ['', Validators.required],
      dosage: [''],
      frequency: [''],
      duration: [''],
      instructions: ['']
    });

  }

  ngOnInit(): void {

    this.role = this.authService.getRole();

    const idParam = this.route.snapshot.paramMap.get('recordId');

    if (!idParam) {
      this.router.navigate(['/medical-records']);
      return;
    }

    this.recordId = Number(idParam);

    this.loadRecord();
    this.loadPrescriptions();

  }


  loadRecord(): void {

    this.medicalRecordService.getById(this.recordId).subscribe({

      next: (response) => {
        this.record = response;
        this.cdr.detectChanges();
      },

      error: (err) => console.error(err)

    });

  }


  loadPrescriptions(): void {

    this.loading = true;

    this.prescriptionService.getByRecord(this.recordId).subscribe({

      next: (response: PrescriptionModel[]) => {

        this.prescriptions = response;

        this.loading = false;

        this.cdr.detectChanges();

      },

      error: (err) => {

        console.error(err);

        this.loading = false;

        this.errorMessage = 'Unable to load prescriptions.';

        this.cdr.detectChanges();

      }

    });

  }


  openCreateForm(): void {

    if (!this.canManagePrescriptions) return;

    this.editingId = null;

    this.prescriptionForm.reset();

    this.showForm = true;

  }


  editPrescription(prescription: PrescriptionModel): void {

    if (!this.canManagePrescriptions) return;

    this.editingId = prescription.prescriptionID;

    this.prescriptionForm.patchValue({

      medicineName: prescription.medicineName,
      dosage: prescription.dosage,
      frequency: prescription.frequency,
      duration: prescription.duration,
      instructions: prescription.instructions

    });

    this.showForm = true;

  }


  savePrescription(): void {

    if (!this.canManagePrescriptions) return;

    if (this.prescriptionForm.invalid) {
      this.prescriptionForm.markAllAsTouched();
      return;
    }

    const formValue = this.prescriptionForm.value;

    const prescriptionData: CreatePrescriptionModel = {

      recordID: this.recordId,
      medicineName: formValue.medicineName,
      dosage: formValue.dosage,
      frequency: formValue.frequency,
      duration: formValue.duration,
      instructions: formValue.instructions

    };

    // UPDATE

    if (this.editingId !== null) {

      this.prescriptionService.updatePrescription(this.editingId, prescriptionData).subscribe({

        next: () => {

          this.cancelForm();

          this.loadPrescriptions();

        },

        error: (error) => {

          console.error('Update Prescription Error:', error);

          this.errorMessage = 'Unable to update prescription.';

          this.cdr.detectChanges();

        }

      });

      return;
    }

    // CREATE

    this.prescriptionService.addPrescription(prescriptionData).subscribe({

      next: () => {

        this.cancelForm();

        this.loadPrescriptions();

      },

      error: (error) => {

        console.error('Create Prescription Error:', error);

        this.errorMessage = 'Unable to add prescription.';

        this.cdr.detectChanges();

      }

    });

  }


  cancelForm(): void {

    this.showForm = false;

    this.editingId = null;

    this.prescriptionForm.reset();

  }


  backToRecords(): void {

    this.router.navigate(['/medical-records']);

  }

}
