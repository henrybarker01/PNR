import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';

@Injectable()
export class PCCService {
  constructor(private readonly httpClient: HttpClient) {}

  getPccList() {
    return this.httpClient.get(`${environment.apiUrl}GetPccList`);
  }
}
