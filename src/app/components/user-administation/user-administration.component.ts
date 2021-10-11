import { Component, OnInit } from '@angular/core';
import { async } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { Client } from '@microsoft/microsoft-graph-client';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { UserAdministrationService } from 'src/app/services/user-administration/user-administartion.service';
import { DialogComponent } from '../dialog/dialog.component';

export interface PeriodicElement {
  name: string;
  position: number;
  weight: number;
  symbol: string;
}

const ELEMENT_DATA = [
  { name: 'Henry', surname: 'Barker', username: 'henry.barker@britehouse.co.za', role: 'User Administation' },
  { name: 'Brandon', surname: 'Slabbert', username: 'brandon.slabbert@britehouse.co.za', role: 'User Administation' },
  { name: 'Juan', surname: 'Lurie', username: 'juan.lurie@britehouse.co.za', role: 'User' },
];

@Component({
  selector: 'app-user-administration',
  templateUrl: './user-administration.component.html',
  styleUrls: ['./user-administration.component.scss'],
})
export class UserAdministrationComponent implements OnInit {
  displayedColumns: string[] = ['name', 'surname', 'username', 'role', 'action'];
  dataSource = ELEMENT_DATA;
  //users: microsoftgraph.User[] = [];
  // users: any[] = [
  // ];
  constructor(public dialog: MatDialog) {} // private readonly authenticationService: AuthenticationService // private readonly userAdministrationService: UserAdministrationService,

  addUsers() {
    const dialogRef = this.dialog.open(DialogComponent);

    dialogRef.afterClosed().subscribe((result) => {
      console.log(`Dialog result: ${result}`);
    });
  }

  removeUser(userName: string) {}

  ngOnInit() {
    //this.GetUser();
  }
  public async GetUser() {
    // Promise<string> {
    // const graphClient = Client.init({
    //   authProvider: async (done) => {
    //     let token = await this.authenticationService.getToken().catch((reason) => {
    //       done(reason, null);
    //     });
    //     if (token) {
    //       token.subscribe((result) => {
    //         done(null, result.accessToken);
    //       });
    //     } else {
    //       done('', null);
    //     }
    //   },
    // });
    // const user = await graphClient.api('/me').get();
    // console.log(user);
    // return '';
  }
}
