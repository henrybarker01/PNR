import { Component } from '@angular/core';

const ELEMENT_DATA = [{ name: 'Henry', surname: 'Barker', username: 'henry.barker@britehouse.co.za', role: 'User Administation' }];

@Component({
  selector: 'app-dialog',
  templateUrl: 'dialog.component.html',
  styleUrls: ['./dialog.component.scss'],
})
export class DialogComponent {
  displayedColumns: string[] = ['name', 'surname', 'username', 'action'];
  dataSource = ELEMENT_DATA;
  usersFound: boolean = false;

  addUser(userName: string) {}

  searchUsers() {
    setTimeout(() => {
      this.usersFound = true;
    }, 300);
  }
}
