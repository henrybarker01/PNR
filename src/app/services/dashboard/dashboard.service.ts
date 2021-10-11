import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { DashboardStatsDto } from 'src/models/DashboardStatsDto';
import { of } from 'rxjs';
import { IPnrSummary } from 'src/app/models/pnr-summary.model';

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

    return of(MockMetricData.data);
  }

  getdashboardmostrecent(): Observable<any[]> {
    // return this.httpClient.get<IPnrSummary[]>(
    //   `${environment.apiUrl}getdashboardmostrecent?PCC=${pcc}&StartDate=${startDate}&EndDate=${endDate}`
    // );

    return of(MockAllPnrData.data);
  }
}

export class MockMetricData {
  public static data = [
    {
      item: 'PNRs Processed',
      value: 23,
    },
    {
      item: 'RulesApplied_Applied',
      value: 50,
    },
    {
      item: 'RulesApplied_NotApplied',
      value: 50,
    },
    {
      item: 'Failed',
      value: 3,
    },
    {
      item: 'Retries',
      value: 4,
    },
  ];
}

export class MockAllPnrData {
  public static data = [
    {
        identifier: "FVHUQSRS",
        pCC: "7SVG",
        dateTimeStamp: "2021-10-10T07:16:49.7257034+00:00",
        rules: 4,
        status: "Passed"
    },
    {
        identifier: "CDTRWKAA",
        pCC: "7SVG",
        dateTimeStamp: "2021-10-10T07:15:28.7264769+00:00",
        rules: 8,
        status: "Passed"
    },
    {
        identifier: "GDUYUESC",
        pCC: "P9DF",
        dateTimeStamp: "2021-10-10T07:16:12.7264782+00:00",
        rules: 6,
        status: "Failed"
    },
    {
        identifier: "GDCCETDS",
        pCC: "P9DF",
        dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
        rules: 6,
        status: "Passed"
    },
    {
      identifier: "GDCCETDS",
      pCC: "P9DF",
      dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
      rules: 6,
      status: "Passed"
  },
  {
    identifier: "GDCCETDS",
    pCC: "P9DF",
    dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
    rules: 6,
    status: "Passed"
},
{
  identifier: "GDCCETDS",
  pCC: "P9DF",
  dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
  rules: 6,
  status: "Passed"
},
{
  identifier: "GDCCETDS",
  pCC: "P9DF",
  dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
  rules: 6,
  status: "Passed"
},
{
  identifier: "GDCCETDS",
  pCC: "P9DF",
  dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
  rules: 6,
  status: "Passed"
},
{
  identifier: "GDCCETDS",
  pCC: "P9DF",
  dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
  rules: 6,
  status: "Passed"
},
{
  identifier: "GDCCETDS",
  pCC: "P9DF",
  dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
  rules: 6,
  status: "Passed"
},
{
  identifier: "GDCCETDS",
  pCC: "P9DF",
  dateTimeStamp: "2021-10-10T07:16:16.7264785+00:00",
  rules: 6,
  status: "Passed"
},
]
}
