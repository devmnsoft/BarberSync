const API_PREFIX = import.meta.env.VITE_KIOSK_API_PREFIX || '/KioskApi';

function buildUrl(path) {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${API_PREFIX.replace(/\/$/, '')}${normalizedPath}`;
}

async function request(path, fallback) {
  try {
    const response = await fetch(buildUrl(path), { headers: { Accept: 'application/json' } });
    if (!response.ok) {
      throw new Error(`Falha ao consultar ${path}: ${response.status}`);
    }

    const payload = await response.json();
    return payload.data || payload.items || payload;
  } catch (error) {
    console.warn('[BarberSync Totem] usando fallback demo', error?.message || error);
    return fallback;
  }
}

const fallbackServices = [
  { id: 'demo-corte', name: 'Corte Masculino', price: 45, durationMinutes: 40, category: 'Barbearia' },
  { id: 'demo-combo', name: 'Corte + Barba', price: 70, durationMinutes: 60, category: 'Combo' },
  { id: 'demo-hidratacao', name: 'Hidratação Capilar', price: 60, durationMinutes: 45, category: 'Estética' }
];

const fallbackOperations = {
  generatedAtUtc: new Date().toISOString(),
  kpis: {
    occupancy: 81.4,
    nps: 89.3,
    automation_coverage: 78.1,
    prediction_accuracy: 92.8
  },
  notifications: [
    { type: 'stock-critical', channel: 'operations', message: 'Estoque crítico de pomada matte. Reposição sugerida em 45 min.' },
    { type: 'service-delay', channel: 'totem', message: 'Cliente avisado sobre espera estimada de 8 minutos.' }
  ]
};

export async function loadTotemSnapshot() {
  const [services, operations] = await Promise.all([
    request('/services', fallbackServices),
    request('/operations-snapshot', fallbackOperations)
  ]);

  return {
    services,
    operations,
    gamification: {
      progressLabel: 'Cliente VIP Ouro',
      nextReward: 'Cashback de 15% em combo premium',
      pointsToNextLevel: 120
    }
  };
}

window.loadTotemSnapshot = loadTotemSnapshot;
