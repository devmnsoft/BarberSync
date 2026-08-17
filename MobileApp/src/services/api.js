export const API_URL = process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:8080';

export class MobileApiError extends Error {
  constructor(message, { status = 0, traceId = null } = {}) {
    super(message);
    this.name = 'MobileApiError';
    this.status = status;
    this.traceId = traceId;
  }
}

function unwrap(payload) {
  return payload?.data ?? payload?.items ?? payload;
}

async function httpGet(path, signal) {
  let response;
  try {
    response = await fetch(`${API_URL}${path}`, {
      headers: { Accept: 'application/json' },
      signal
    });
  } catch (error) {
    if (error?.name === 'AbortError') throw error;
    throw new MobileApiError('Não foi possível conectar ao BarberSync. Verifique sua internet e tente novamente.');
  }

  const traceId = response.headers.get('x-trace-id');
  const payload = await response.json().catch(() => null);
  if (!response.ok) {
    throw new MobileApiError(
      payload?.message || 'Não foi possível carregar seus dados agora.',
      { status: response.status, traceId: payload?.traceId || traceId }
    );
  }
  return unwrap(payload);
}

async function getMobileSummary(signal) {
  const summary = await httpGet('/api/mobile/summary', signal);
  if (!summary || typeof summary !== 'object') {
    throw new MobileApiError('O serviço retornou uma resposta inválida.');
  }
  return {
    operations: summary.operations ?? {},
    appointments: Array.isArray(summary.appointments) ? summary.appointments : [],
    loyalty: Array.isArray(summary.loyalty) ? summary.loyalty : [],
    coupons: Array.isArray(summary.coupons) ? summary.coupons : [],
    notifications: Array.isArray(summary.notifications) ? summary.notifications : []
  };
}

export const mobileApi = {
  getMobileSummary,
  getOperationsSnapshot: signal => httpGet('/api/full-service-flow/snapshot', signal),
  getAppointments: signal => httpGet('/api/appointments', signal),
  getLoyaltyAccounts: signal => httpGet('/api/loyalty/accounts', signal)
};
