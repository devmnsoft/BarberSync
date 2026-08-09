(() => {
  const FLOW_KEY = 'barbersync.kiosk.flow';
  const SUMMARY_KEY = 'barbersync.kiosk.summary';
  const read = key => { try { return JSON.parse(sessionStorage.getItem(key) || '{}'); } catch { return {}; } };
  const write = (key, value) => sessionStorage.setItem(key, JSON.stringify(value));

  window.KioskFlow = {
    deviceCode: window.BarberSyncKiosk?.deviceCode || new URLSearchParams(location.search).get('deviceCode') || 'KIOSK-001',
    get state() { return read(FLOW_KEY); },
    setState(patch) {
      const next = { ...this.state, ...patch, deviceCode: this.deviceCode, updatedAt: new Date().toISOString() };
      write(FLOW_KEY, next);
      return next;
    },
    reset() { sessionStorage.removeItem(FLOW_KEY); sessionStorage.removeItem(SUMMARY_KEY); },
    async request(url, options = {}) {
      const response = await fetch(url, { headers: { Accept: 'application/json', ...(options.body ? { 'Content-Type': 'application/json' } : {}) }, ...options });
      const payload = await response.json().catch(() => ({ success: false, message: 'Resposta inválida do servidor.' }));
      if (!response.ok || payload.success === false) throw new Error(payload.message || `Falha HTTP ${response.status}`);
      return payload;
    },
    post(url, body) { return this.request(url, { method: 'POST', body: JSON.stringify(body) }); },
    saveSummary(summary) { write(SUMMARY_KEY, summary); this.setState(summary); return summary; },
    get summary() { return read(SUMMARY_KEY); }
  };
})();
