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
  blocks: signal => request('/api/mobile/professional/blocks', { signal }), block: input => request('/api/mobile/professional/blocks', { method: 'POST', body: input })
};
