import { Injectable } from '@angular/core';
import { HttpClient,HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root'
})
export class ApplicantService {
  private apiUrl = 'https://localhost:44371/api/project';
  
 constructor(private http: HttpClient) {}
  private getAuthHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');

    return new HttpHeaders({
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    });
  }

 
  login(credentials: { loginID: string; password: string }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, credentials);
  }

 
  register(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, data);
  }
   getPassportTypes(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/passport-types`);
  }
  

  getOffices(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/offices`);
  }
   getDocumentTypes(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/document-types`);
  }

  submitApplication(formData: FormData): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/submit`, formData);
  }
    getAvailableDates(officeId: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/available-dates/${officeId}`);
  }
  getTimeSlots(officeId: number, date: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/time-slots/${officeId}/${date}`);
  }
    scheduleAppointment(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/schedule-appointment`, data);
    console.log('ss',data);
  }
   processPayment(data: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/process-payment`, data);
  }

    // Status
  getApplicationStatus(arn: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/status/${arn}`);
  }



 

 getMyApplications() {
  return this.http.get<any[]>(
    `${this.apiUrl}/my-applications`,
    { headers: this.getAuthHeaders() }
  );
}
  
//  confirmPayment(arn: string, amount: number) {
//   return this.http.post<any>(
//     `${this.apiUrl}/payment/confirm`,
//     {
//       ARN: arn,
//       Amount: amount
//     }
//   );
// }
//     startApplication(payload: any): Observable<any> {
//     return this.http.post(
//       `${this.apiUrl}/passport/apply`,
//       payload
//     );
//   }
//  getAvailableSlots(pskId: number, date: string) {
//   return this.http.get<any[]>(
//     `${this.apiUrl}/appointment/available-slots?pskId=${pskId}&date=${date}`
//   );
// }
getProfile() {
  // No loginID needed now, backend extracts from JWT
  return this.http.get(`${this.apiUrl}/profile`);
}


   checkLoginID(loginId: string): Observable<{ isAvailable: boolean }> {
    return this.http.get<{ isAvailable: boolean }>(
      `${this.apiUrl}/check-login/${loginId}`
    );
  }
//  scheduleAppointment(data: any) {
//   return this.http.post(
//     `${this.apiUrl}/appointment/schedule`,
//     data
//   );
// }

}


