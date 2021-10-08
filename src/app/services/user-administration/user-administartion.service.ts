import { Injectable } from '@angular/core';
import { MsalService } from '@azure/msal-angular';
import { InteractionType, PublicClientApplication } from '@azure/msal-browser';
import { Client } from '@microsoft/microsoft-graph-client';
import { AuthCodeMSALBrowserAuthenticationProvider } from '@microsoft/microsoft-graph-client/authProviders/authCodeMsalBrowser';
import * as MicrosoftGraph from '@microsoft/microsoft-graph-types';
import { environment } from 'src/environments/environment';

@Injectable()
export class UserAdministrationService {
  public graphClient?: Client;
  public authenticated?: boolean;

  constructor(private readonly msalService: MsalService) {}

  private async getUser(): Promise<MicrosoftGraph.User | undefined> {
    if (!this.authenticated) return undefined;

    const authProvider = new AuthCodeMSALBrowserAuthenticationProvider(this.msalService.instance as PublicClientApplication, {
      account: this.msalService.instance.getActiveAccount()!,
      scopes: environment.msalConfig.auth.scopes,
      interactionType: InteractionType.Popup,
    });

    this.graphClient = Client.initWithMiddleware({
      authProvider: authProvider,
    });

    const graphUser: MicrosoftGraph.User = await this.graphClient
      .api('/me')
      .select('displayName,mail,mailboxSettings,userPrincipalName')
      .get();    

    return graphUser;
  }
}
