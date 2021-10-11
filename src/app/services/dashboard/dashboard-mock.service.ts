import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { of } from 'rxjs';

@Injectable()
export class DashboardMockService {
  constructor() {}

  getDashboardStats(): Observable<any[]> {
    return of(MockMetricData.data);
  }

  getdashboardmostrecent(): Observable<any[]> {
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
      value: 97,
    },
    {
      item: 'RulesApplied_NotApplied',
      value: 3,
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
      identifier: 'FVHUQSRS',
      pCC: '7SVG',
      dateTimeStamp: '2021-10-10T07:16:49.7257034+00:00',
      rules: 4,
      status: 'Passed',
    },
    {
      identifier: 'CDTRWKAA',
      pCC: '7SVG',
      dateTimeStamp: '2021-10-10T07:15:28.7264769+00:00',
      rules: 8,
      status: 'Passed',
    },
    {
      identifier: 'GDUYUESC',
      pCC: 'P9DF',
      dateTimeStamp: '2021-10-10T07:16:12.7264782+00:00',
      rules: 6,
      status: 'Failed',
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
      rules: 6,
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
      rules: 6,
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
      rules: 6,
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
      rules: 6,
      status: 'Passed',
    },
    {
      identifier: 'GDCCETDS',
      pCC: 'P9DF',
      dateTimeStamp: '2021-10-10T07:16:16.7264785+00:00',
      rules: 6,
      status: 'Passed',
    },
  ];
}
