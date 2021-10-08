import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { AuthenticationResult, PopupRequest } from '@azure/msal-browser';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  isLoggedIn: boolean = false;
  userDetail: AuthenticationResult = {
    authority: '',
    uniqueId: '',
    tenantId: '',
    scopes: [],
    account: null,
    idToken: '',
    idTokenClaims: {},
    accessToken: '',
    fromCache: false,
    expiresOn: null,
    tokenType: '',
    correlationId: '',
  };

  constructor(private readonly msalService: MsalService, private readonly router: Router) {}

  ngOnInit() {
    if (this.msalService.instance.getActiveAccount()) {
      this.router.navigateByUrl('/dashboard');
    } else {
      this.login();
    }
  }

  login() {
    this.msalService.loginPopup().subscribe(
      (response) => {
        this.msalService.instance.setActiveAccount(response.account);
        localStorage.setItem('Token', response.idToken);
        this.userDetail = response;
        this.isLoggedIn = true;
      },
      (error) => {
        if (error.errorMessage.includes('AADB2C90118')) {
          this.resetPassword();
        } else {
          this.isLoggedIn = false;
        }
      }
    );
  }

  logout() {
    this.msalService.logout().subscribe(() => {
      this.isLoggedIn = false;
      this.userDetail = {
        authority: '',
        uniqueId: '',
        tenantId: '',
        scopes: [],
        account: null,
        idToken: '',
        idTokenClaims: {},
        accessToken: '',
        fromCache: false,
        expiresOn: null,
        tokenType: '',
        correlationId: '',
      };
    });
  }

  private resetPassword() {
    let authRequestConfig: PopupRequest;
    authRequestConfig = {} as PopupRequest;
    authRequestConfig.authority = environment.passwordResetUrl;

    this.msalService.loginPopup(authRequestConfig).subscribe(
      (response: any) => {
        this.msalService.instance.setActiveAccount(response.account);
        localStorage.setItem('Token', response.idToken);
        this.userDetail = response;
        this.isLoggedIn = true;
      },
      (error) => {
        this.isLoggedIn = false;
        console.log(`Log in failed. Detail: ${error}`);
      }
    );
  }
}
