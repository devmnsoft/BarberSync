const fs = require('fs');
const requiredFiles = [
  'index.js',
  'App.js',
  'src/services/api.js',
  'src/theme/colors.js',
  'src/theme/spacing.js',
  'src/screens/LoginScreen.js'
];
for (const file of requiredFiles) {
  if (!fs.existsSync(file)) {
    console.error(`Missing required file: ${file}`);
    process.exit(1);
  }
}

const appSource = fs.readFileSync('App.js', 'utf8');
for (const importPath of ["./src/theme/colors", "./src/theme/spacing", "./src/services/api"]) {
  if (!appSource.includes(importPath)) {
    console.error(`App.js is missing required production import: ${importPath}`);
    process.exit(1);
  }
}

for (const contract of ['mobileApi.slots', 'professionalId, serviceId, date', 'Confirmar agendamento']) {
  if (!appSource.includes(contract)) {
    console.error(`Mobile scheduling flow is missing contract: ${contract}`);
    process.exit(1);
  }
}
for (const forbidden of ['Agendamento demo', 'FullServiceFlow demo', 'mobile-demo-token']) {
  if (appSource.includes(forbidden)) {
    console.error(`Mobile contains forbidden fabricated runtime content: ${forbidden}`);
    process.exit(1);
  }
}
for (const benefitContract of ['benefits.packages', 'benefits.coupons', 'benefits.cashback']) {
  if (!appSource.includes(benefitContract)) {
    console.error(`Mobile relationship summary is missing: ${benefitContract}`);
    process.exit(1);
  }
}
const apiSource = fs.readFileSync('src/services/api.js', 'utf8');
for (const contract of ['clubSummary:', 'clubWallet:', 'clubMemberships:', 'clubGiftCards:', 'clubVouchers:', 'redeemClubVoucher:']) {
  if (!apiSource.includes(contract)) { console.error(`Mobile club contract is missing: ${contract}`); process.exit(1); }
}
for (const contract of ['/notifications/inbox', '/notifications/preferences', "request('/api/mobile/appointments'", '/api/mobile/appointments/availability', '/cancel', '/reschedule']) {
  if (!apiSource.includes(contract)) { console.error(`Mobile notification contract is missing: ${contract}`); process.exit(1); }
}
for (const professionalContract of ['commissions?.open', 'commissions?.paid', "tab === 'Metas'", 'production?.revenue', 'occupancy?.scheduledToday']) {
  if (!appSource.includes(professionalContract)) {
    console.error(`Professional mobile summary is missing: ${professionalContract}`);
    process.exit(1);
  }
}
if (/availableSlots\?\.\[0\]|professionals\?\.\[0\]/.test(appSource)) {
  console.error('Mobile must not silently select the first professional or slot.');
  process.exit(1);
}

for (const jsFile of ['App.js', 'src/services/api.js', 'src/theme/colors.js', 'src/theme/spacing.js']) {
  require('child_process').execFileSync(process.execPath, ['--check', jsFile], { stdio: 'inherit' });
}
console.log('Mobile smoke test passed.');
if (!(fs.readFileSync(require('path').join(__dirname,'../docs/GOVERNANCE_WORKFLOW.md'),'utf8').includes('UUIDs existem'))) throw new Error('governance no-ID contract missing');

const portalSource = fs.readFileSync('src/services/api.js', 'utf8');
['clientPortalRequestCode','clientPortalVerifyCode','clientPortalHome','clientPortalAppointments','clientPortalHistory','clientPortalConsents','clientPortalBudgets','clientPortalPayments','clientPortalBenefits','clientPortalReviews','clientPortalSupport','openClientPortalSupport'].forEach(contract => { if (!portalSource.includes(contract)) { console.error(`Contrato do Portal ausente: ${contract}`); process.exit(1); } });
console.log('CLIENT_PORTAL_MOBILE_CONTRACTS_OK');

['qualitySummary','qualityReviews','submitQualityReview','qualityFollowUps','completeQualityFollowUp'].forEach(contract => { if (!apiSource.includes(contract)) { console.error(`Contrato Quality ausente: ${contract}`); process.exit(1); } });
console.log('QUALITY_MOBILE_CONTRACTS_OK');
