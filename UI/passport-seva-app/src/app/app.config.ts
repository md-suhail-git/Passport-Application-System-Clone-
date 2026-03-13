import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { jwtInterceptor } from './interceptors/jwt.interceptor';
import { routes } from './app.routes';
import { provideIonicAngular } from '@ionic/angular/standalone';

export const appConfig: ApplicationConfig = {
  providers: [
    provideHttpClient(
      withInterceptors([jwtInterceptor])
    ),
    provideRouter(routes), provideIonicAngular({})
  ]
};
