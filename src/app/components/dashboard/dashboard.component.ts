import { OnInit } from '@angular/core';
import { Component } from '@angular/core';
import { IPnrSummary } from 'src/app/models/pnr-summary.model'; 
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
  mostRecentPnrs: IPnrSummary[] = [];
  pnrProcessedItem!: DashboardStatsDto;
  pnrRulesAppliedItem!: DashboardStatsDto;
  pnrRulesNotAppliedItem!: DashboardStatsDto;
  pnrFailedItem!: DashboardStatsDto;
  pnrRetriesItem!: DashboardStatsDto;
  selectedStore: string = '';
  surveySeries: number[] = [];
  surveyLabels: string[] = ['rules applied', 'no rules applied'];
  seriesColors: string[] = ['#546E7A', '#E91E63'];
  displayedColumns: string[] = ['Identifier', 'PCC', 'Time', 'Rules', 'Status'];
  dataSource = ELEMENT_DATA;

  constructor(private readonly dashboardService: DashboardService) {}

  ngOnInit(): void {
    this.getDashboardStats();
    this.getDashboardMostRecent();
  }

  getDashboardStats() {
    this.dashboardService.getDashboardStats().subscribe((x) => {
      this.buildDashBoardStatsItems(x);
    });
  }

  getDashboardMostRecent() {
    this.dashboardService.getDashboardMostRecent().subscribe((x) => {
      this.mostRecentPnrs = x;
    });
  }

  isGroup(index, item): boolean {
    return item.isGroupBy;
  }

  buildDashBoardStatsItems(dashBoardItems: DashboardStatsDto[]) {
    dashBoardItems.forEach((x) => {
      switch (x.item) {
        case DashboardMetricTypes.Processed:
          return (this.pnrProcessedItem = x);
        case DashboardMetricTypes.RulesApplied:
          this.surveySeries.push(x.value);
          return (this.pnrRulesAppliedItem = x);
        case DashboardMetricTypes.RulesNotApplied:
          this.surveySeries.push(x.value);
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

export interface IPnrSummaryTest {
  identifier: string;
  pCC: string;
  dateTimeStamp: string;
  rules: number;
  status: string;
}

export interface GroupBy {
  date: Date;
  isGroupBy: boolean;
  numberOfFailedPnr: number;
  numberOfPassedPnr: number;
}

const ELEMENT_DATA: (IPnrSummaryTest | GroupBy)[] = [
  { date: new Date(), numberOfFailedPnr: 1, numberOfPassedPnr: 1, isGroupBy: true },
  {
    identifier: 'CDTRWKAA',
    pCC: 'P9DF',
    dateTimeStamp: '2021-10-10T07:16:16.7264785+00:00',
    rules: 4,
    status: 'Failed',
  },
  {
    identifier: 'GDCCETDS',
    pCC: 'P9DF',
    dateTimeStamp: '2021-10-10T07:16:16.7264785+00:00',
    rules: 1,
    status: 'Passed',
  },
  {
    identifier: 'FVHUQSRS',
    pCC: 'P9DF',
    dateTimeStamp: '2021-10-10T07:16:16.7264785+00:00',
    rules: 6,
    status: 'Passed',
  },
  { date: new Date('2021-10-09T07:16:16.7264785+00:00'), numberOfFailedPnr: 2, numberOfPassedPnr: 2, isGroupBy: true },
  {
    identifier: 'GDCCETDS',
    pCC: 'P9DF',
    dateTimeStamp: '2021-10-10T07:16:16.7264785+00:00',
    rules: 0,
    status: 'Failed',
  },
  {
    identifier: 'GDCCETDS',
    pCC: 'P9DF',
    dateTimeStamp: '2021-10-10T07:16:16.7264785+00:00',
    rules: 10,
    status: 'Passed',
  },
  {
    identifier: 'GDCCETDS',
    pCC: 'P9DF',
    dateTimeStamp: '2021-10-10T07:16:16.7264785+00:00',
    rules: 6,
    status: 'Passed',
  },
  {
    identifier: 'GDCCETDS',
    pCC: 'P9DF',
    dateTimeStamp: '2021-10-10T07:16:16.7264785+00:00',
    rules: 2,
    status: 'Failed',
  },
];
