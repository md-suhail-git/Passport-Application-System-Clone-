import { Component, OnInit } from '@angular/core';
import { ApplicantService } from '../../services/applicant.service';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
})
export class ProfileComponent implements OnInit {
   applications: any[] = [];
  loading = true;
  errorMessage = '';

  constructor(private applicantService: ApplicantService) {}

  ngOnInit(): void {
    this.loadApplications();
  }

  loadApplications() {
    this.applicantService.getMyApplications().subscribe({
      next: res => {
        this.applications = res;
        this.loading = false;
      },
      error: err => {
        console.error(err);
        this.errorMessage = 'Unable to load applications.';
        this.loading = false;
      }
    });
  }
}