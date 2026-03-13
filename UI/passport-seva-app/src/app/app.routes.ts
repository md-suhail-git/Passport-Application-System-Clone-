import { Routes } from '@angular/router';
import { RegistrationComponent } from './Pages/registration/registration.component';
import { LoginComponent } from './Pages/login/login.component';
import { HomeComponent } from './Overview/home/home.component';
import { authGuard } from './auth.guard';

import { ProfileComponent } from './Overview/profile/profile.component';
import { ApplyComponent } from './Passport/apply/apply.component';
import { LayoutComponent } from './layout/layout.component';

export const routes: Routes = [
 
   // Public pages
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegistrationComponent },

  // Layout pages
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: 'home', component: HomeComponent },
      { path: 'passport/apply', component: ApplyComponent },
      { path: 'my-applications', component: ProfileComponent },
      { path: 'profile', component: ProfileComponent },

      // default after login
      { path: '', redirectTo: 'home', pathMatch: 'full' }
    ]
  },

  // Fallback
  { path: '**', redirectTo: 'login' }
];
