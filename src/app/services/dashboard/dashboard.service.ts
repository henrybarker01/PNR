import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DashboardStatsDto } from 'src/models/DashboardStatsDto';
import { of } from 'rxjs';

@Injectable()
export class DashboardService {
  constructor(private readonly httpClient: HttpClient) {}

  getDashboardStats(): Observable<any[]> {
    const headers = new HttpHeaders({
      'x-functions-key':
        'VqziGr8SPgew6GbqJI4dQsDtp3G8ylhhZEptzA4T970hNG2aApzI5g==',
      'Access-Control-Allow-Origin': '*',
    });

    // return this.httpClient.get<DashboardStatsDto[]>(
    //   'https://fn-bidtravel-pnrfinisher-portal-dev.azurewebsites.net/api/getdashboardtelemetry?PCC=7SVG&StartDate=2021-01-01&EndDate=2022-01-01',
    //   { headers: headers }
    // );

    return of(Data.data);
  }
}

export class Data {
  public static data = [
    {
      item: 'PNRs Processed',
      value: 23,
    },
    {
      item: 'RulesApplied_Applied',
      value: 97,
    },
    {
      item: 'RulesApplied_NotApplied',
      value: 3,
    },
    {
      item: 'Failed',
      value: 1,
    },
    {
      item: 'Retries',
      value: 4,
    },
  ];
}
