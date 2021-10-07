export const environment = {
  production: false,
  msalConfig: {
    auth: {
      clientId: 'c798dbaa-124a-4ccc-acd3-75fda7956fed',
      authority: 'https://BidtravelB2C001.b2clogin.com/BidtravelB2C001.onmicrosoft.com/B2C_1_SignIn',
      redirectUri: 'http://localhost:4200',
      postLogoutRedirectUri: '/',
      knownAuthorities: ['https://BidtravelB2C001.b2clogin.com'],
    },
  },
  passwordResetUrl: 'https://BidtravelB2C001.b2clogin.com/BidtravelB2C001.onmicrosoft.com/B2C_1_KaapAgriResetPassword',
};

