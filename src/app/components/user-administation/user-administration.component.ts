import { Component, OnInit } from '@angular/core';
import { UserAdministrationService } from 'src/app/services/user-administration/user-administartion.service';

@Component({
  selector: 'app-user-administration',
  templateUrl: './user-administration.component.html',
  styleUrls: ['./user-administration.component.scss'],
})
export class UserAdministrationComponent implements OnInit {
  //users: microsoftgraph.User[] = [];

  constructor(private readonly userAdministrationService: UserAdministrationService) {}

  async ngOnInit() {
   // const user = await this.userAdministrationService.getUser();
    //if (user) this.users.push(user);
  }
}
