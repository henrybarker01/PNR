import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import {
  MsalGuard,
  MsalInterceptor,
  MsalModule,
  MsalService,
  MSAL_INSTANCE,
} from '@azure/msal-angular';
import {
  BrowserCacheLocation,
  InteractionType,
  IPublicClientApplication,
  PublicClientApplication,
} from '@azure/msal-browser';
import { LogLevel } from '@azure/msal-common';
import { environment } from 'src/environments/environment';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { HeaderComponent } from './components/header/header.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatCardModule } from '@angular/material/card';
import { DashboardService } from './services/dashboard/dashboard.service';
import { FunctionsKeyInterceptor } from './interceptors/functions-key.interceptor';
import { UserAdministrationComponent } from './components/user-administation/user-administration.component';
import { AuthenticationService } from './services/authentication/authentication.service';
import { AvatarModule } from 'ngx-avatar'; 
import { UserAdministrationService } from './services/user-administration/user-administartion.service';
 import { MatIconModule } from '@angular/material/icon';
import { AppDoughnutChartComponent } from './components/shared/app-doughnut-chart/app-doughnut-chart.component';
import { MatTableModule } from '@angular/material/table';
import { NgApexchartsModule } from 'ng-apexcharts'; 

const isIE =
  window.navigator.userAgent.indexOf('MSIE ') > -1 ||
  window.navigator.userAgent.indexOf('Trident/') > -1;

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

const avatarColors = ['#19025d'];

@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    DashboardComponent,
    UserAdministrationComponent,
    AppDoughnutChartComponent,
  ],
  imports: [
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    MsalModule,
    MatCardModule,
    MatTableModule,
    MatIconModule,
    NgApexchartsModule,
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
          ['https://graph.microsoft.com/v1.0/me', ['user.read']],
        ]),
      }
    ),
    BrowserAnimationsModule,
    HttpClientModule,
    AvatarModule.forRoot({
      colors: avatarColors,
    }),
  ],
  providers: [
    {
      provide: MSAL_INSTANCE,
      useFactory: MSALInstanceFactory,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true,
    },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: FunctionsKeyInterceptor,
      multi: true,
    },
    MsalService,
    MsalGuard,
    DashboardService,
    HttpClientModule,
    AuthenticationService,
    UserAdministrationService,
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
