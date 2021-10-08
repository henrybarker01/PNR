import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AccountInfo } from '@azure/msal-common';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnInit {
  selectedMenuItem: string = 'dashboard';
  activeAccountInfo: AccountInfo | null | undefined;

  constructor(private readonly router: Router, private readonly authenticationService: AuthenticationService) {}

  ngOnInit() {
    if (environment.production) {
      this.activeAccountInfo = this.authenticationService.getActiveAccount();
      this.authenticationService.userLoggedIn.subscribe((result) => {
        if (result) this.activeAccountInfo = this.authenticationService.getActiveAccount();
      });
    } else {
      this.activeAccountInfo = {
        name: 'John Doe',
        environment: '',
        homeAccountId: '',
        localAccountId: '',
        tenantId: '',
        username: 'john.doe@bidtravel.co.za',
        idTokenClaims: undefined,
      };
    }
  }

  selectMenuItem(selectedItem: string) {
    this.selectedMenuItem = selectedItem;
    this.router.navigateByUrl(selectedItem);
  }
}
