export const environment = {
  production: true,
  apiUrl: 'https://fn-bidtravel-pnrfinisher-portal-dev.azurewebsites.net/api/',
  functionsKey: 'VqziGr8SPgew6GbqJI4dQsDtp3G8ylhhZEptzA4T970hNG2aApzI5g==',
  msalConfig: {
    auth: {
      clientId: 'c798dbaa-124a-4ccc-acd3-75fda7956fed',
      authority: 'https://BidtravelB2C001.b2clogin.com/BidtravelB2C001.onmicrosoft.com/B2C_1_SignIn',
      redirectUri: 'https://storagepnrfinisherdev.z1.web.core.windows.net/',
      postLogoutRedirectUri: '/',
      knownAuthorities: ['https://BidtravelB2C001.b2clogin.com'],
      scopes: ['user.read'],
    },
  },
  passwordResetUrl: 'https://BidtravelB2C001.b2clogin.com/BidtravelB2C001.onmicrosoft.com/B2C_1_KaapAgriResetPassword',
};
 