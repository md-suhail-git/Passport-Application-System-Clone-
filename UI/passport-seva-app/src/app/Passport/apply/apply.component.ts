import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ApplicantService } from '../../services/applicant.service';

interface PassportType {
  PassportTypeID: number;
  TypeName: string;
  ValidityYears: number;
  Fee: number;
}

interface Office {
  OfficeID: number;
  OfficeName: string;
  Address: string;
  City: string;
  State: string;
  PostalCode: string;
}

interface DocumentType {
  DocumentTypeID: number;
  TypeName: string;
  IsRequired: boolean;
  MaxFileSizeMB: number;
}

interface TimeSlot {
  TimeSlot: string;
  IsAvailable: boolean;
  AvailableCount: number;
}

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, FormsModule],
  selector: 'app-apply',
  templateUrl: './apply.component.html',
  styleUrls: ['./apply.component.scss']
})
export class ApplyComponent implements OnInit {

  applicationForm!: FormGroup;
  
  steps = ['Office', 'Passport', 'Application', 'Personal', 'Emergency', 'Documents', 'Preview', 'ARN', 'Schedule', 'Summary', 'Payment', 'Complete'];
  currentStep = 1;
  maxStep = 12;

  passportTypes: PassportType[] = [];
  offices: Office[] = [];
  documentTypes: DocumentType[] = [];
  documents: { [key: number]: File } = {};

  // ARN
  arnNumber = '';
  applicationId = 0;

  // Calendar
  weekDays = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
  currentMonth = new Date();
  calendarDays: (Date | null)[] = [];
  availableDates: Date[] = [];
  bookedDates: Date[] = [];
  selectedDate: Date | null = null;
  selectedSlot = '';
  availableSlots: TimeSlot[] = [];

  // Payment
  paymentMethod = 'online';
  cardNumber = '';
  expiryDate = '';
  cvv = '';
  cardHolderName = '';
  qrCodeUrl = 'https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=upi://pay';
  isProcessingPayment = false;
  transactionId = '';

  isSubmitting = false;
  submissionError = '';
  validationErrors: any;

  constructor(
    private fb: FormBuilder,
    private applicantService: ApplicantService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadDropdowns();
    this.generateCalendar();
  }

  initForm(): void {
    this.applicationForm = this.fb.group({
      OfficeID: [null, Validators.required],
      PassportTypeID: [null, Validators.required],
      ApplicationType: [null, Validators.required],
      PreviousPassportNumber: [''],

      NationalID: ['', [Validators.required, Validators.maxLength(50)]],
      FirstName: ['', [Validators.required, Validators.maxLength(100)]],
      MiddleName: [''],
      LastName: ['', [Validators.required, Validators.maxLength(100)]],
      DateOfBirth: ['', Validators.required],
      Gender: ['', Validators.required],
      PlaceOfBirth: [''],
      MaritalStatus: [''],
      Occupation: [''],

      Email: ['', [Validators.required, Validators.email]],
      Phone: ['', [Validators.required, Validators.maxLength(20)]],
      Address: ['', [Validators.required, Validators.maxLength(500)]],
      City: ['', [Validators.required, Validators.maxLength(100)]],
      State: ['', [Validators.required, Validators.maxLength(100)]],
      PostalCode: ['', [Validators.required, Validators.maxLength(20)]],

      EmergencyContactName: ['', Validators.required],
      EmergencyContactPhone: ['', Validators.required],
      TravelPurpose: [''],
      ExpectedTravelDate: [''],

      Declaration: [false, Validators.requiredTrue]
    });
  }

  loadDropdowns(): void {
    this.applicantService.getPassportTypes().subscribe({
      next: (data) => this.passportTypes = data,
      error: (err) => console.error(err)
    });

    this.applicantService.getOffices().subscribe({
      next: (data) => this.offices = data,
      error: (err) => console.error(err)
    });

    this.applicantService.getDocumentTypes().subscribe({
      next: (data) => this.documentTypes = data,
      error: (err) => console.error(err)
    });
  }

  // Navigation
  goNext(): void {
    if (this.currentStep < this.maxStep) {
      this.currentStep++;
    }
  }

  goBack(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  saveStep(): void {
    localStorage.setItem('applicationDraft', JSON.stringify(this.applicationForm.value));
    alert('Progress saved!');
  }

  // Validations
  isStep4Valid(): boolean {
    const fields = ['FirstName', 'LastName', 'NationalID', 'DateOfBirth', 'Gender', 'Email', 'Phone', 'Address', 'City', 'State', 'PostalCode'];
    return fields.every(f => this.applicationForm.get(f)?.valid);
  }

 isStep5Valid(): boolean {
  return !!(
    this.applicationForm.get('EmergencyContactName')?.valid &&
    this.applicationForm.get('EmergencyContactPhone')?.valid
  );
}


  areRequiredDocsUploaded(): boolean {
    return this.documentTypes
      .filter(d => d.IsRequired)
      .every(d => this.documents[d.DocumentTypeID]);
  }

  // File handling
  onFileSelect(event: Event, documentTypeId: number): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const docType = this.documentTypes.find(d => d.DocumentTypeID === documentTypeId);
      
      if (docType && file.size > docType.MaxFileSizeMB * 1024 * 1024) {
        alert(`File size must be less than ${docType.MaxFileSizeMB}MB`);
        input.value = '';
        return;
      }
      this.documents[documentTypeId] = file;
    }
  }

  // Helper methods
  getOfficeName(): string {
    const office = this.offices.find(o => o.OfficeID === this.applicationForm.get('OfficeID')?.value);
    return office ? `${office.OfficeName} - ${office.City}` : '';
  }

  getOfficeFullAddress(): string {
  const office = this.offices.find(o => o.OfficeID === this.applicationForm.get('OfficeID')?.value);
  return office
    ? `${office.OfficeName}, ${office.Address}, ${office.City}, ${office.State} - ${office.PostalCode}`
    : '';
}

  getPassportTypeName(): string {
    const type = this.passportTypes.find(t => t.PassportTypeID === this.applicationForm.get('PassportTypeID')?.value);
    return type ? type.TypeName : '';
  }

  getPassportFee(): number {
    const type = this.passportTypes.find(t => t.PassportTypeID === this.applicationForm.get('PassportTypeID')?.value);
    return type ? type.Fee : 0;
  }

  getGenderLabel(): string {
    const g = this.applicationForm.get('Gender')?.value;
    return g === 'M' ? 'Male' : g === 'F' ? 'Female' : 'Other';
  }

  getUploadedDocuments(): { typeName: string; fileName: string }[] {
    return Object.keys(this.documents).map(key => {
      const docType = this.documentTypes.find(d => d.DocumentTypeID === +key);
      return {
        typeName: docType?.TypeName || '',
        fileName: this.documents[+key].name
      };
    });
  }

  // Submit Application
  submitApplication(): void {
    if (this.applicationForm.invalid) {
      this.applicationForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formData = new FormData();
    formData.append('ApplicationData', JSON.stringify(this.applicationForm.value));

    Object.keys(this.documents).forEach(key => {
      formData.append(`Documents_${key}`, this.documents[+key]);
    });
console.log("Submitting application:", this.applicationForm.value);
    this.applicantService.submitApplication(formData).subscribe({
     
      next: (res) => {
        this.arnNumber = res.ARN;
        this.applicationId = res.ApplicationID;
        this.currentStep = 8;
        localStorage.removeItem('applicationDraft');
        console.log('ApplicationID:', this.applicationId);
      },
      error: (err) => {
        console.error("Backend error:", err.error);
        this.validationErrors = err.error.Errors;
        this.submissionError = err?.error?.message || 'Submission failed';
        this.isSubmitting = false;
      },
      complete: () => this.isSubmitting = false
    });
  }

  // ARN Dialog actions
  goToSchedule(): void {
    this.loadAvailableDates();
    this.currentStep = 9;
  }

  goToPayment(): void {
    this.currentStep = 10;
  }

  // Calendar methods
  generateCalendar(): void {
    const year = this.currentMonth.getFullYear();
    const month = this.currentMonth.getMonth();
    const firstDay = new Date(year, month, 1).getDay();
    const daysInMonth = new Date(year, month + 1, 0).getDate();

    this.calendarDays = [];
    for (let i = 0; i < firstDay; i++) {
      this.calendarDays.push(null);
    }
    for (let i = 1; i <= daysInMonth; i++) {
      this.calendarDays.push(new Date(year, month, i));
    }
  }

  prevMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() - 1, 1);
    this.generateCalendar();
  }

  nextMonth(): void {
    this.currentMonth = new Date(this.currentMonth.getFullYear(), this.currentMonth.getMonth() + 1, 1);
    this.generateCalendar();
  }

  loadAvailableDates(): void {
    const officeId = this.applicationForm.get('OfficeID')?.value;
    this.applicantService.getAvailableDates(officeId).subscribe({
      next: (data) => {
        this.availableDates = data.availableDates.map((d: string) => new Date(d));
        this.bookedDates = data.bookedDates.map((d: string) => new Date(d));
      }
    });
  }

  isDateAvailable(date: Date): boolean {
    return this.availableDates.some(d => this.isSameDate(d, date));
  }

  isDateSelected(date: Date): boolean {
    return this.selectedDate ? this.isSameDate(this.selectedDate, date) : false;
  }

  isPastDate(date: Date): boolean {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return date < today;
  }

  isSameDate(d1: Date, d2: Date): boolean {
    return d1.getFullYear() === d2.getFullYear() &&
           d1.getMonth() === d2.getMonth() &&
           d1.getDate() === d2.getDate();
  }

  selectDate(date: Date | null): void {
    if (!date || this.isPastDate(date) || !this.isDateAvailable(date)) return;
    this.selectedDate = date;
    this.selectedSlot = '';
    this.loadTimeSlots();
  }

  loadTimeSlots(): void {
    if (!this.selectedDate) return;
    const officeId = this.applicationForm.get('OfficeID')?.value;
    const dateStr = this.selectedDate.toISOString().split('T')[0];

    this.applicantService.getTimeSlots(officeId, dateStr).subscribe({
      next: (data) => this.availableSlots = data,
      error: () => this.availableSlots = []
    });
  }

  selectSlot(slot: string): void {
    this.selectedSlot = slot;
  }

  confirmAppointment(): void {
    if (!this.selectedDate || !this.selectedSlot) return;

    const appointmentData = {
      ApplicationID: this.applicationId,
      OfficeID: this.applicationForm.get('OfficeID')?.value,
      AppointmentDate: this.selectedDate.toISOString().split('T')[0],
      TimeSlot: this.selectedSlot,
      Purpose: 'Biometrics'
    };

    this.applicantService.scheduleAppointment(appointmentData).subscribe({
      next: () => this.currentStep = 10,
      error: (err) => alert(err?.error?.message || 'Failed to schedule appointment')
    });
  }

  // Payment
  processPayment(): void {
    this.isProcessingPayment = true;

    const paymentData = {
      ApplicationID: this.applicationId,
      Amount: this.getPassportFee(),
      PaymentMethod: this.paymentMethod
    };

    this.applicantService.processPayment(paymentData).subscribe({
      next: (res) => {
        this.transactionId = res.TransactionID;
        this.currentStep = 12;
      },
      error: (err) => {
        alert(err?.error?.message || 'Payment failed');
        this.isProcessingPayment = false;
      },
      complete: () => this.isProcessingPayment = false
    });
  }

  printReceipt(): void {
    const printContent = document.getElementById('printReceipt');
    if (printContent) {
      const printWindow = window.open('', '', 'width=800,height=600');
      printWindow?.document.write(`
        <html>
          <head>
            <title>Appointment Receipt</title>
            <style>
              body { font-family: Arial, sans-serif; padding: 20px; }
              .receipt-header { text-align: center; border-bottom: 2px solid #000; padding-bottom: 15px; margin-bottom: 20px; }
              table { width: 100%; border-collapse: collapse; }
              td { padding: 10px; border-bottom: 1px solid #ddd; }
              td:first-child { font-weight: bold; width: 40%; }
            </style>
          </head>
          <body>${printContent.innerHTML}</body>
        </html>
      `);
      printWindow?.document.close();
      printWindow?.print();
    }
  }
}
