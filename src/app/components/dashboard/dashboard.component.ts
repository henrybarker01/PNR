import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { DashboardService } from 'src/app/services/dashboard/dashboard.service';
import { DashboardMetricTypes } from 'src/models/DashboardMetricTypes';
import { DashboardStatsDto } from 'src/models/DashboardStatsDto';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
})
export class DashboardComponent implements OnInit {
  dashBoardItems: DashboardStatsDto[] = [];
  pnrProcessedItem!: DashboardStatsDto;
  pnrRulesAppliedItem!: DashboardStatsDto;
  pnrRulesNotAppliedItem!: DashboardStatsDto;
  pnrFailedItem!: DashboardStatsDto;
  pnrRetriesItem!: DashboardStatsDto;
  selectedStore: string = '';

  constructor(private readonly dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.getDashboardStats();
  }

  getDashboardStats() {
    this.dashboardService.getDashboardStats().subscribe((x) => {
      this.buildDashBoardStatsItems(x);
    });
  }

  buildDashBoardStatsItems(dashBoardItems: DashboardStatsDto[]) {
    dashBoardItems.forEach((x) => {
      switch (x.item) {
        case DashboardMetricTypes.Processed:
          return (this.pnrProcessedItem = x);
        case DashboardMetricTypes.RulesApplied:
          return (this.pnrRulesAppliedItem = x);
        case DashboardMetricTypes.RulesNotApplied:
          return (this.pnrRulesNotAppliedItem = x);
        case DashboardMetricTypes.Failed:
          return (this.pnrFailedItem = x);
        case DashboardMetricTypes.Retries:
          return (this.pnrRetriesItem = x);
        default:
          return (this.pnrRetriesItem = x);
      }
    });
    this.dashBoardItems = dashBoardItems;
  }
}
