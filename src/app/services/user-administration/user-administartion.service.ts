import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';

@Injectable()
export class UserAdministrationService {
  public authenticated?: boolean;

  constructor(private readonly msalService: MsalService) {}
}
