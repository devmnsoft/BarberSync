export const API_URL = process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:8080';
let accessToken = null;
export class MobileApiError extends Error { constructor(message, { status = 0, traceId = null } = {}) { super(message); this.name = 'MobileApiError'; this.status = status; this.traceId = traceId; } }
const unwrap = payload => payload?.data ?? payload?.items ?? payload;
async function request(path, { method = 'GET', body, signal } = {}) {
  let response;
  try { response = await fetch(`${API_URL}${path}`, { method, signal, headers: { Accept: 'application/json', ...(body ? { 'Content-Type': 'application/json' } : {}), ...(accessToken ? { Authorization: `Bearer ${accessToken}` } : {}) }, body: body ? JSON.stringify(body) : undefined }); }
  catch (error) { if (error?.name === 'AbortError') throw error; throw new MobileApiError('Sem conexão com o BarberSync. Nenhuma alteração foi simulada.'); }
  const payload = await response.json().catch(() => null); const traceId = payload?.traceId || response.headers.get('x-trace-id');
  if (!response.ok || payload?.success === false) throw new MobileApiError(payload?.message || 'Não foi possível concluir a operação.', { status: response.status, traceId });
  return unwrap(payload);
}
const query = values => new URLSearchParams(Object.entries(values).filter(([, value]) => value !== undefined && value !== null).map(([key, value]) => [key, String(value)])).toString();
export const mobileApi = {
  setAccessToken(token) { accessToken = token || null; }, logout() { accessToken = null; },
  login: input => request('/api/auth/login', { method: 'POST', body: input }),
  getMobileSummary: signal => request('/api/mobile/summary', { signal }), services: signal => request('/api/mobile/services', { signal }),
  appointments: signal => request('/api/mobile/appointments', { signal }),
  slots: values => request(`/api/mobile/appointments/availability?${query(values)}`), createAppointment: input => request('/api/mobile/appointments', { method: 'POST', body: { ...input, origin: 'Mobile' } }),
  reschedule: (id, startsAt, reason) => request(`/api/mobile/appointments/${id}/reschedule`, { method: 'POST', body: { startsAt, reason } }),
  cancel: (id, reason) => request(`/api/mobile/appointments/${id}/cancel`, { method: 'POST', body: { reason } }),
  history: signal => request('/api/mobile/client/history', { signal }), benefits: signal => request('/api/mobile/client/benefits', { signal }),
  notifications: signal => request('/api/mobile/notifications', { signal }), readNotification: id => request(`/api/mobile/notifications/${id}/read`, { method: 'POST' }),
  notificationInbox: signal => request('/api/notifications/inbox', { signal }), markInboxRead: id => request(`/api/notifications/inbox/${id}/read`, { method: 'POST' }),
  markAllInboxRead: () => request('/api/notifications/inbox/read-all', { method: 'POST' }), notificationPreferences: signal => request('/api/notifications/preferences', { signal }),
  updateNotificationPreferences: items => request('/api/notifications/preferences', { method: 'PUT', body: { items, source: 'Mobile' } }),
  professionalDay: signal => request('/api/mobile/professional/day', { signal }), start: id => request(`/api/mobile/professional/appointments/${id}/start`, { method: 'POST' }),
  finish: id => request(`/api/mobile/professional/appointments/${id}/finish`, { method: 'POST' }), commissions: signal => request('/api/mobile/professional/commissions', { signal }),
  clubSummary: signal => request('/api/mobile/club/summary', { signal }), clubWallet: signal => request('/api/mobile/club/wallet', { signal }),
  clubMemberships: signal => request('/api/mobile/club/memberships', { signal }), clubGiftCards: signal => request('/api/mobile/club/gift-cards', { signal }),
  clubVouchers: signal => request('/api/mobile/club/vouchers', { signal }), redeemClubVoucher: code => request('/api/mobile/club/vouchers/redeem', { method: 'POST', body: { code } }),
  clientPortalRequestCode: input => request('/api/client-portal/auth/request-code', { method: 'POST', body: input }),
  clientPortalVerifyCode: input => request('/api/client-portal/auth/verify-code', { method: 'POST', body: input }),
  clientPortalHome: signal => request('/api/client-portal/home', { signal }), clientPortalAppointments: signal => request('/api/client-portal/appointments', { signal }),
  clientPortalHistory: signal => request('/api/client-portal/history', { signal }), clientPortalConsents: signal => request('/api/client-portal/consents', { signal }),
  clientPortalBudgets: signal => request('/api/client-portal/budgets', { signal }), clientPortalPayments: signal => request('/api/client-portal/payments', { signal }),
  clientPortalBenefits: signal => request('/api/client-portal/benefits', { signal }), clientPortalReviews: input => request('/api/client-portal/reviews', { method: 'POST', body: input }),
  marketingOffers: signal => request('/api/mobile/marketing/offers', { signal }), marketingCampaigns: signal => request('/api/mobile/marketing/campaigns', { signal }),
  trackMarketing: input => request('/api/mobile/marketing/track', { method: 'POST', body: input }),
  catalogServices: signal => request('/api/mobile/catalog/services', { signal }),
  catalogProducts: signal => request('/api/mobile/catalog/products', { signal }),
  catalogCombos: signal => request('/api/mobile/catalog/combos', { signal }),
  simulateCatalogPrice: input => request('/api/mobile/catalog/simulate-price', { method: 'POST', body: input }),
  serviceExecutionToday: signal => request('/api/mobile/service-execution/today', { signal }),
  serviceExecutionOrders: signal => request('/api/mobile/service-execution/orders', { signal }),
  serviceExecutionCheckIn: input => request('/api/mobile/service-execution/check-in', { method: 'POST', body: input }),
  completeServiceOrder: (id, input) => request(`/api/mobile/service-execution/orders/${id}/complete-service`, { method: 'POST', body: input }),
  previewServiceCheckout: input => request('/api/mobile/service-execution/checkout/preview', { method: 'POST', body: input }),
  partnersMarketplace: signal => request('/api/mobile/partners/marketplace', { signal }),
  partnerOffers: signal => request('/api/mobile/partners/offers', { signal }),
  trackPartner: input => request('/api/mobile/partners/track', { method: 'POST', body: input }),
    qualitySummary: signal => request('/api/mobile/quality/summary', { signal }), qualityReviews: signal => request('/api/mobile/quality/reviews', { signal }),
  submitQualityReview: input => request('/api/mobile/quality/reviews', { method: 'POST', body: input }), qualityFollowUps: signal => request('/api/mobile/quality/follow-ups', { signal }),
  completeQualityFollowUp: id => request(`/api/mobile/quality/follow-ups/${id}/complete`, { method: 'POST' }),
  clientPortalSupport: signal => request('/api/client-portal/support', { signal }), openClientPortalSupport: input => request('/api/client-portal/support', { method: 'POST', body: input }),
  team360Me: signal => request('/api/mobile/team360/me', { signal }), team360Schedule: signal => request('/api/mobile/team360/schedule', { signal }),
  team360Productivity: signal => request('/api/mobile/team360/productivity', { signal }), team360Commissions: signal => request('/api/mobile/team360/commissions', { signal }),
  team360Goals: signal => request('/api/mobile/team360/goals', { signal }), team360Trainings: signal => request('/api/mobile/team360/trainings', { signal }),
  completeTeam360Training: id => request(`/api/mobile/team360/trainings/${id}/complete`, { method: 'POST' }),
  finance360Summary: signal => request('/api/mobile/finance360/summary', { signal }),
  finance360Receivables: signal => request('/api/mobile/finance360/receivables', { signal }),
  finance360Payables: signal => request('/api/mobile/finance360/payables', { signal }),
  finance360Commissions: signal => request('/api/mobile/finance360/commissions', { signal }),
  finance360Payroll: signal => request('/api/mobile/finance360/payroll', { signal }),
  blocks: signal => request('/api/mobile/professional/blocks', { signal }), block: input => request('/api/mobile/professional/blocks', { method: 'POST', body: input })
};
