import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Router } from '@angular/router';
import { AccountInfo } from '@azure/msal-common';
import { PCC } from 'src/app/models/pcc.model';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { PCCService } from 'src/app/services/pcc/pcc.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent implements OnInit {
  selectedMenuItem: string = 'dashboard';
  activeAccountInfo: AccountInfo | null | undefined;
  selectedPCC: PCC[] = [];
  pccList: PCC[] = [];

  constructor(
    private readonly router: Router,
    private readonly authenticationService: AuthenticationService,
    private readonly pccService: PCCService
  ) {}

  pcc = new FormControl();

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

    this.pccService.getPccList().subscribe((result) => {
      this.pccList = result;
      this.pccList.unshift({
        PCC: 'All PCCs',
      });
      this.selectedPCC.push(this.pccList[0]);
    });
  }

  selectMenuItem(selectedItem: string) {
    this.selectedMenuItem = selectedItem;
    this.router.navigateByUrl(selectedItem);
  }

  search() {}
}
