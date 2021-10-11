import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { PCC } from 'src/app/models/pcc.model';
import { environment } from 'src/environments/environment';

@Injectable()
export class PCCService {
  constructor(private readonly httpClient: HttpClient) {}

  getPccList(): Observable<PCC[]> {
    return this.httpClient.get<PCC[]>(`${environment.apiUrl}GetPccList`);
  }
}
