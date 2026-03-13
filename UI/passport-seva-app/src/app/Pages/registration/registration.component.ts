import { Component, OnInit } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  FormGroup,
  Validators
} from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { ApplicantService } from '../../services/applicant.service';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  selector: 'app-registration',
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.css']
})
export class RegistrationComponent implements OnInit {

  registrationForm!: FormGroup;

  isLoginIdAvailable = false;
  loginIdCheckMessage = '';
  submissionError = '';
  isSubmitting = false;
  router: any;

  constructor(
    private fb: FormBuilder,
    private applicantService: ApplicantService
  ) {}

  ngOnInit(): void {
    this.registrationForm = this.fb.group(
      {
        RegistrationType: ['PASSPORT_OFFICE', Validators.required],

        FullName: ['', [
          Validators.required,
          Validators.maxLength(150),
          this.fullNameValidator
        ]],

        EmailAddress: ['', [Validators.required, Validators.email]],

        isLoginSameAsEmail: ['Yes', Validators.required],

        LoginID: ['', [Validators.required, Validators.minLength(6)]],

        password: ['', [
          Validators.required,
          Validators.minLength(8),
          this.passwordComplexityValidator
        ]],

        confirmPassword: ['', Validators.required]
      },
      { validators: this.passwordMatchValidator }
    );

    /* Sync LoginID with Email */
    this.registrationForm.get('isLoginSameAsEmail')?.valueChanges.subscribe(val => {
      const loginCtrl = this.registrationForm.get('LoginID');
      const email = this.registrationForm.get('EmailAddress')?.value;

      if (val === 'Yes') {
        loginCtrl?.disable();
        loginCtrl?.setValue(email);
        this.isLoginIdAvailable = true;
        this.loginIdCheckMessage = '';
      } else {
        loginCtrl?.enable();
        loginCtrl?.reset();
        this.isLoginIdAvailable = false;
      }
    });

    /* Update LoginID when Email changes */
    this.registrationForm.get('EmailAddress')?.valueChanges.subscribe(email => {
      if (this.registrationForm.get('isLoginSameAsEmail')?.value === 'Yes') {
        this.registrationForm.get('LoginID')?.setValue(email);
      }
    });
  }

  /* Validators */

  fullNameValidator(control: AbstractControl) {
    return /\b(dr|mr|mrs|ms|col)\b/i.test(control.value || '')
      ? { invalidFullName: true }
      : null;
  }

  passwordMatchValidator(form: FormGroup) {
    return form.get('password')?.value === form.get('confirmPassword')?.value
      ? null
      : { mismatch: true };
  }

  passwordComplexityValidator(control: AbstractControl) {
    const v = control.value || '';
    return /[A-Z]/.test(v) && /[a-z]/.test(v) && /\d/.test(v) && /[@$!%*?&]/.test(v)
      ? null
      : { complexity: true };
  }

  /* Login ID availability */

  async checkLoginIdAvailability(): Promise<void> {
    if (this.registrationForm.get('isLoginSameAsEmail')?.value === 'Yes') {
      this.isLoginIdAvailable = true;
      return;
    }

    const loginId = this.registrationForm.get('LoginID')?.value;

    if (!loginId || loginId.length < 6) {
      this.loginIdCheckMessage = 'Login ID must be at least 6 characters.';
      this.isLoginIdAvailable = false;
      return;
    }

    this.loginIdCheckMessage = 'Checking availability...';

    try {
      const res = await firstValueFrom(
        this.applicantService.checkLoginID(loginId)
      );

      this.isLoginIdAvailable = res.isAvailable;
      this.loginIdCheckMessage = res.isAvailable
        ? 'Login ID is available.'
        : 'Login ID already exists.';
    } catch {
      this.loginIdCheckMessage = 'Unable to verify Login ID.';
      this.isLoginIdAvailable = false;
    }
  }

  /* Submit */

  onSubmit(): void {
    if (this.registrationForm.invalid || !this.isLoginIdAvailable) {
      this.registrationForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const payload = this.registrationForm.getRawValue();

    this.applicantService.register(payload).subscribe({
      next: () => {
        alert('Registration successful!');
        this.registrationForm.reset();
        this.router.navigate(['/login']);     
      },
      error: err => {
        this.submissionError = err?.error?.message || 'Registration failed.';
      },
      complete: () => this.isSubmitting = false
    });
      
  }
}
