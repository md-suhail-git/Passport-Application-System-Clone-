import { Component, OnInit } from '@angular/core';

import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
  standalone: true,
    imports: [
    CommonModule,
    RouterModule,
    MatIconModule,
    MatButtonModule
  ],
})
export class LayoutComponent implements OnInit {

  fullName: string | null = '';
  loginID: string | null = '';
  sidebarCollapsed = false;

  constructor(private router: Router) {}

  ngOnInit(): void {
    const token = localStorage.getItem('token');
    if (!token) {
      this.router.navigate(['/login']);
      return;
    }

    this.fullName = localStorage.getItem('fullName');
    this.loginID = localStorage.getItem('loginID');
  }

  toggleSidebar() {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  getInitials(): string {
    return (this.fullName || this.loginID || 'U')
      .charAt(0)
      .toUpperCase();
  }

  logout() {
    localStorage.clear();
    this.router.navigate(['/login']);
  }
}
