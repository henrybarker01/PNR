import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from 'src/environments/environment';
import { AuthenticationService } from './services/authentication/authentication.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
})
export class AppComponent implements OnInit {
  constructor(private readonly router: Router, private readonly authenticationService: AuthenticationService) {}

  ngOnInit() {
    if (environment.production) {
      if (this.authenticationService.isLoggedIn()) {
        this.router.navigateByUrl('/dashboard');
      } else {
        this.login();
      }
    }
  }

  login() {
    this.authenticationService.login();
  }

  logout() {
    this.authenticationService.logout();
  }
}
