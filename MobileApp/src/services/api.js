export const API_URL = process.env.EXPO_PUBLIC_API_URL ?? 'http://localhost:8080';

const demo = {
  operations: {
    revenueToday: 4280,
    occupancy: 88,
    waiting: 3,
    currentFlow: 'Cliente → Check-in → Comanda → Pagamento → Cashback'
  },
  appointments: [
    { id: 'mob-001', serviceName: 'Corte + Barba', professionalName: 'Rafael Barber', time: '18:30', status: 'Confirmado' },
    { id: 'mob-002', serviceName: 'Hidratação Capilar', professionalName: 'Camila Beauty', time: 'Amanhã 10:00', status: 'Disponível' }
  ],
  loyalty: [
    { id: 'loy-001', pointsBalance: 1280, cashbackBalance: 38.5, tierLevel: 3 }
  ]
};

function unwrap(payload, fallback) {
  const data = payload?.data ?? payload?.items ?? payload;
  if (Array.isArray(data)) return data.length ? data : fallback;
  return data && Object.keys(data).length ? data : fallback;
}

async function httpGet(path, fallback) {
  try {
    const response = await fetch(`${API_URL}${path}`, { headers: { Accept: 'application/json' } });
    if (!response.ok) throw new Error(`Request failed for ${path} with status ${response.status}`);
    return unwrap(await response.json(), fallback);
  } catch (error) {
    console.warn('[BarberSync MobileApi] modo demo offline', error?.message || error);
    return fallback;
  }
}

async function getMobileSummary() {
  const summary = await httpGet('/api/mobile/summary', demo);
  return {
    operations: summary.operations ?? demo.operations,
    appointments: summary.appointments ?? demo.appointments,
    loyalty: summary.loyalty ?? demo.loyalty
  };
}

export const mobileApi = {
  getMobileSummary,
  getOperationsSnapshot: () => httpGet('/api/full-service-flow/snapshot', demo.operations),
  getAppointments: () => httpGet('/api/appointments', demo.appointments),
  getLoyaltyAccounts: () => httpGet('/api/loyalty/accounts', demo.loyalty),
  demo
};
