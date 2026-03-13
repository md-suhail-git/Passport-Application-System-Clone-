import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
  const router = inject(Router);
  
  // Logic: Check if the user ID exists in local storage
  const isLoggedIn = !!localStorage.getItem('userID'); 

  if (isLoggedIn) {
    return true; // Allow access
  } else {
    // Redirect to login if not authenticated
    router.navigate(['/login']);
    return false;
  }
};