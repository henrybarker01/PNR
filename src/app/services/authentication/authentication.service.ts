import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { PopupRequest } from '@azure/msal-browser';
import { AccountInfo } from '@azure/msal-common';
import { Subject } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable()
export class AuthenticationService {
  userLoggedIn: Subject<boolean> = new Subject<boolean>();

  constructor(private readonly msalService: MsalService) {}

  getActiveAccount(): AccountInfo | null {
    return this.msalService.instance.getActiveAccount();
  }

  isLoggedIn(): boolean {
    return this.msalService.instance.getActiveAccount() != null;
  }

  login() {
    this.msalService.loginPopup().subscribe(
      (response) => {
        this.msalService.instance.setActiveAccount(response.account);
        localStorage.setItem('Token', response.idToken);
        this.userLoggedIn.next(true);
      },
      (error) => {
        if (error.errorMessage.includes('AADB2C90118')) {
          this.resetPassword();
        } else {
          this.userLoggedIn.next(false);
        }
      }
    );
  }

  resetPassword() {
    let authRequestConfig: PopupRequest;
    authRequestConfig = {} as PopupRequest;
    authRequestConfig.authority = environment.passwordResetUrl;

    this.msalService.loginPopup(authRequestConfig).subscribe(
      (response: any) => {
        this.msalService.instance.setActiveAccount(response.account);
        localStorage.setItem('Token', response.idToken);
        this.userLoggedIn.next(true);
      },
      () => {
        this.userLoggedIn.next(false);
      }
    );
  }

  logout() {
    this.msalService.logout().subscribe(() => {
      this.userLoggedIn.next(false);
    });
  }
}
