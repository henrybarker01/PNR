import {  HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { MsalGuard, MsalInterceptor, MsalModule, MsalService, MSAL_INSTANCE } from '@azure/msal-angular';
import { BrowserCacheLocation, InteractionType, IPublicClientApplication, PublicClientApplication } from '@azure/msal-browser';
import { LogLevel } from '@azure/msal-common';
import { environment } from 'src/environments/environment';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { HeaderComponent } from './components/header/header.component';
import { DashboardService } from './services/dashboard/dashboart.service';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

const isIE = window.navigator.userAgent.indexOf('MSIE ') > -1 || window.navigator.userAgent.indexOf('Trident/') > -1;

export function loggerCallback(logLevel: LogLevel, message: string) {
  console.log(message);
}

export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: environment.msalConfig.auth,
    cache: {
      cacheLocation: BrowserCacheLocation.LocalStorage,
      storeAuthStateInCookie: isIE,
    },
    system: {
      loggerOptions: {
        loggerCallback,
        logLevel: LogLevel.Info,
        piiLoggingEnabled: false,
      },
    },
  });
}

@NgModule({
  declarations: [AppComponent, HeaderComponent, DashboardComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    MsalModule,
    MsalModule.forRoot(
      new PublicClientApplication({
        auth: environment.msalConfig.auth,
      }),
      {
        
        interactionType: InteractionType.Redirect,
        authRequest: {
          scopes: ['user.read'],
        },
        loginFailedRoute: '',
      },
      {
        interactionType: InteractionType.Redirect,
        protectedResourceMap: new Map([ 
            ['https://graph.microsoft.com/v1.0/me', ['user.read']]
        ])
      }
    ),
    BrowserAnimationsModule,
    HttpClientModule
  ],
  providers: [
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory,
    },
    {
      provide: HTTP_INTERCEPTORS, // Provides as HTTP Interceptor
      useClass: MsalInterceptor,
      multi: true
  },
    MsalService,
    MsalGuard,
    DashboardService,
    HttpClientModule
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
