import { Component } from '@angular/core';
import { DashboardService } from 'src/app/services/dashboard/dashboart.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent {
  constructor(private readonly dashboardService: DashboardService) {}
}
