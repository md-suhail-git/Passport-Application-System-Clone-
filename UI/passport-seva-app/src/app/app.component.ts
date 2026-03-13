import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { IonApp, IonTabs, IonHeader, IonButtons, IonToolbar, IonMenuButton, IonTitle, IonButton, IonIcon, IonRouterOutlet, IonTabBar, IonLabel, IonTabButton, IonContent, IonAvatar, IonFooter, IonItem, IonList, IonMenuToggle, IonMenu, IonSplitPane } from "@ionic/angular/standalone";

@Component({
  selector: 'app-root',
   standalone: true,
  imports: [RouterOutlet, CommonModule, MatToolbarModule, // 👈 Add this
    MatButtonModule, // 👈 Add this for mat-button
    MatIconModule],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
logout() {
throw new Error('Method not implemented.');
}
  title = 'passport-seva-app';
  showNavbar: boolean = false;
  constructor(private router: Router) {
    this.router.events.pipe(
      filter(event => event instanceof NavigationEnd)
    ).subscribe((event: any) => {
      // Hide navbar if the URL is /login or /register
      const authRoutes = ['/login', '/register', '/'];
      this.showNavbar = !authRoutes.includes(event.urlAfterRedirects);
    });
}
}
