import { Component } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent {
  selectedMenuItem: string = 'dashboard';

  constructor(private readonly router: Router) {}

  selectMenuItem(selectedItem: string) {
    this.selectedMenuItem = selectedItem;
    this.router.navigateByUrl(selectedItem);
  }
}
