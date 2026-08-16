const API_PREFIX = import.meta.env.VITE_KIOSK_API_PREFIX || '/KioskApi';

function buildUrl(path) {
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;
  return `${API_PREFIX.replace(/\/$/, '')}${normalizedPath}`;
}

async function request(path) {
  const response = await fetch(buildUrl(path), { headers: { Accept: 'application/json' } });
  if (!response.ok) {
    const traceId = response.headers.get('X-Trace-Id');
    const error = new Error(`Não foi possível carregar o Totem (${response.status}).`);
    error.traceId = traceId;
    throw error;
  }

  const payload = await response.json();
  return payload.data || payload.items || payload;
}

export async function loadTotemSnapshot() {
  const [services, operations] = await Promise.all([
    request('/services'),
    request('/operations-snapshot')
  ]);

  return {
    services,
    operations
  };
}

window.loadTotemSnapshot = loadTotemSnapshot;
