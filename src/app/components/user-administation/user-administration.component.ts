import { Component, OnInit } from '@angular/core';
import { async } from '@angular/core/testing';
import { Client } from '@microsoft/microsoft-graph-client';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { UserAdministrationService } from 'src/app/services/user-administration/user-administartion.service';

@Component({
  selector: 'app-user-administration',
  templateUrl: './user-administration.component.html',
  styleUrls: ['./user-administration.component.scss'],
})
export class UserAdministrationComponent implements OnInit {
  //users: microsoftgraph.User[] = [];

  constructor(
    private readonly userAdministrationService: UserAdministrationService,
    private readonly authenticationService: AuthenticationService
  ) {}

  ngOnInit() {
    this.GetUser();
  }
  public async GetUser(): Promise<string> {
    const graphClient = Client.init({
      authProvider: async (done) => {
        let token = await this.authenticationService.getToken().catch((reason) => {
          done(reason, null);
        });

        if (token) {
          token.subscribe((result) => {
            done(null, result.accessToken);
          });
        } else {
          done('', null);
        }
      },
    });

    const user = await graphClient.api('/me').get();
    console.log(user);
    return '';
  }
}
