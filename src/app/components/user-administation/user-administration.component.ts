import { Component, OnInit, ViewChild } from '@angular/core';
import { async } from '@angular/core/testing';
import { MatDialog } from '@angular/material/dialog';
import { Client } from '@microsoft/microsoft-graph-client';
import { AuthenticationService } from 'src/app/services/authentication/authentication.service';
import { UserAdministrationService } from 'src/app/services/user-administration/user-administartion.service';
import { DialogComponent } from '../dialog/dialog.component';
import { MatTable } from '@angular/material/table';

export interface PeriodicElement {
  name: string;
  position: number;
  weight: number;
  symbol: string;
}

const ELEMENT_DATA = [,];

@Component({
  selector: 'app-user-administration',
  templateUrl: './user-administration.component.html',
  styleUrls: ['./user-administration.component.scss'],
})
export class UserAdministrationComponent implements OnInit {
  @ViewChild(MatTable) table: MatTable<PeriodicElement> | undefined;
  displayedColumns: string[] = ['name', 'surname', 'username', 'role', 'action'];
  dataSource: any[] = [];
  //users: microsoftgraph.User[] = [];
  // users: any[] = [
  // ];
  constructor(public dialog: MatDialog) {} // private readonly authenticationService: AuthenticationService // private readonly userAdministrationService: UserAdministrationService,

  addUsers() {
    const dialogRef = this.dialog.open(DialogComponent);
    dialogRef.afterClosed().subscribe((result) => {
      console.log(`Dialog result: ${result}`);
      this.dataSource.push({ name: 'Henry', surname: 'Demo', username: 'henry.demo@bidtravel.co.za', role: 'User' });
      if (this.table) this.table.renderRows();
    });
  }

  removeUser(userName: string) {
    this.dataSource = this.dataSource.filter((x) => x.username !== userName);
    if (this.table) this.table.renderRows();
  }

  ngOnInit() {
    this.dataSource.push({ name: 'Henry', surname: 'Barker', username: 'henry.barker@britehouse.co.za', role: 'User Administation' });
    this.dataSource.push({
      name: 'Brandon',
      surname: 'Slabbert',
      username: 'brandon.slabbert@britehouse.co.za',
      role: 'User Administation',
    });
    this.dataSource.push({ name: 'Juan', surname: 'Lurie', username: 'juan.lurie@britehouse.co.za', role: 'User' });
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
