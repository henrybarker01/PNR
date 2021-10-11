import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IDashboardTelementry } from 'src/app/models/dashboard-telementry.model';
import { IPnrSummary } from 'src/app/models/pnr-summary.model';
import { environment } from 'src/environments/environment';
import { DashboardMockService } from './dashboard-mock.service';

@Injectable()
export class DashboardService {
  constructor(private readonly httpClient: HttpClient, private readonly dashboardMockService: DashboardMockService) {}

  getDashboardStats(startDate: string, endDate: string, pcc: string): Observable<IDashboardTelementry[]> {
    return this.httpClient.get<IDashboardTelementry[]>(
      `${environment.apiUrl}getdashboardtelemetry?PCC=${pcc}&StartDate=${startDate}&EndDate=${endDate}`
    );
  }

  getDashboardMostRecent(startDate: string, endDate: string, pcc: string): Observable<IPnrSummary[]> {
    return this.dashboardMockService.getdashboardmostrecent();
    //startDate: Date, endDate: Date, pcc: string
    // return this.httpClient.get<IPnrSummary[]>(
    //   `${environment.apiUrl}getdashboardmostrecent?PCC=${pcc}&StartDate=${startDate}&EndDate=${endDate}`
    // );
  }
}
