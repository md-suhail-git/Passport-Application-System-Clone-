import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApplicantService } from '../../services/applicant.service';


@Component({
  standalone: true,
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
  LoginForm!: FormGroup;
  errorMessage: string = '';
  isSubmitting: boolean = false;

  constructor(
    private fb: FormBuilder,
    private applicantService: ApplicantService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.LoginForm = this.fb.group({
      // We use LoginID as the key to match your database field
      LoginID: ['', [Validators.required]],
      Password: ['', [Validators.required]]
    });
  }

  onLogin(): void {
    if (this.LoginForm.invalid) {
      this.LoginForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    const loginData = this.LoginForm.value;

    this.applicantService.login(loginData).subscribe({
      next: (response) => {
        
        // 1. Save the JWT Token (usually in LocalStorage or a Cookie)
        localStorage.setItem('token', response.token);
    localStorage.setItem('fullName', response.fullName);
         localStorage.setItem('loginID', loginData.LoginID);

    alert('Login successful');
        
        // 2. Redirect the user to the Dashboard/Application home
      this.router.navigate(['/home']);
      },
      error: (err) => {
        this.isSubmitting = false;
         alert('Invalid credentials');
        // Check if the backend sent a specific message
        this.errorMessage = err.error?.message || 'Invalid Login ID or Password.';
      },
      complete: () => {
        this.isSubmitting = false;
      }
    });
  }
}