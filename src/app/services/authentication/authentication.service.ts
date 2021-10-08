import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { PopupRequest, SilentRequest } from '@azure/msal-browser';
import { AccountInfo, AuthenticationResult } from '@azure/msal-common';
import { Observable, Subject } from 'rxjs';
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

  async getToken() {
    const silentRequest: SilentRequest = {
      scopes: environment.msalConfig.auth.scopes,
      authority: environment.msalConfig.auth.authority,
    };
    return await this.msalService.acquireTokenSilent(silentRequest);
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
