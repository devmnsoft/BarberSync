(() => {
  let state = {};

  const requestState = async (method, patch) => {
    const response = await fetch('/KioskFlow', {
      method,
      headers: patch ? { 'Content-Type': 'application/json', Accept: 'application/json' } : { Accept: 'application/json' },
      body: patch ? JSON.stringify(patch) : undefined
    });
    if (!response.ok) throw new Error('Não foi possível manter a etapa atual do atendimento.');
    return response.status === 204 ? {} : response.json();
  };

  window.KioskFlow = {
    deviceCode: window.BarberSyncKiosk?.deviceCode || new URLSearchParams(location.search).get('deviceCode') || '',
    get state() { return state; },
    async initialize() {
      state = await requestState('GET');
      return state;
    },
    async setState(patch) {
      state = await requestState('PUT', { ...patch, deviceCode: this.deviceCode });
      return state;
    },
    async reset() { state = await requestState('DELETE'); },
    async request(url, options = {}) {
      const response = await fetch(url, { headers: { Accept: 'application/json', ...(options.body ? { 'Content-Type': 'application/json' } : {}) }, ...options });
      const payload = await response.json().catch(() => ({ success: false, message: 'Resposta inválida do servidor.' }));
      if (!response.ok || payload.success === false) throw new Error(payload.message || `Falha HTTP ${response.status}`);
      return payload;
    },
    post(url, body) { return this.request(url, { method: 'POST', body: JSON.stringify(body) }); },
    async saveSummary(summary) { return this.setState(summary); },
    get summary() { return state; }
  };
})();
