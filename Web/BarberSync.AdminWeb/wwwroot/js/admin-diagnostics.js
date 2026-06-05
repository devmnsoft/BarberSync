(() => {
  const checks = [
    { key: 'api', label: 'API', url: '/AdminApi/api-health' },
    { key: 'adminApi', label: 'AdminApi', url: '/AdminApi/dashboard' },
    { key: 'publicApi', label: 'PublicApi', url: '/AdminApi/services' },
    { key: 'kioskApi', label: 'KioskApi', url: '/AdminApi/kiosk-status' },
    { key: 'swagger', label: 'Swagger', url: '/AdminApi/swagger.json' },
    { key: 'assets', label: 'Assets', url: '/css/admin-design-system.css' }
  ];
  const setCard = (key, status, detail) => {
    const card = document.querySelector(`[data-diagnostic-card="${key}"]`);
    const dot = card?.querySelector('.diagnostic-dot');
    const text = document.querySelector(`[data-diagnostic-detail="${key}"]`);
    if (dot) dot.className = `diagnostic-dot ${status}`;
    if (text) text.textContent = detail;
  };
  const safeFetch = async (url) => {
    try {
      const response = await fetch(url, { headers: { Accept: 'application/json,text/css,*/*' } });
      return { ok: response.ok, status: response.status };
    } catch (error) {
      return { ok: false, status: 0, error: error?.message || String(error) };
    }
  };
  const renderTests = (tests) => {
    const root = document.querySelector('[data-demo-store-tests]');
    if (!root) return;
    root.innerHTML = tests.map(test => `<div class="diagnostic-test ${test.passed ? 'pass' : 'fail'}"><div><strong>${test.name}</strong><small>${test.detail}</small></div><span>${test.passed ? 'Passou' : 'Falhou'}</span></div>`).join('');
  };
  const renderEvents = () => {
    const eventsRoot = document.querySelector('[data-diagnostics-events]');
    const errorsRoot = document.querySelector('[data-diagnostics-errors]');
    const events = window.BarberSyncEventBus?.last?.(12) || [];
    if (eventsRoot) eventsRoot.innerHTML = events.length ? events.map(e => `<div class="diagnostic-event"><strong>${e.title || e.eventName}</strong><small>${e.module || 'BarberSync'} • ${new Date(e.createdAt || Date.now()).toLocaleString('pt-BR')}</small></div>`).join('') : 'Nenhum evento registrado.';
    const errors = events.filter(e => ['danger','error','fail'].includes(String(e.severity || '').toLowerCase()) || String(e.eventName || '').toLowerCase().includes('error'));
    if (errorsRoot) errorsRoot.innerHTML = errors.length ? errors.map(e => `<div class="diagnostic-event"><strong>${e.title || e.eventName}</strong><small>${e.description || ''}</small></div>`).join('') : 'Nenhum erro crítico registrado nesta sessão.';
  };
  const runLocalChecks = () => {
    setCard('demoStore', window.BarberSyncDemoStore ? 'ok' : 'fail', window.BarberSyncDemoStore ? 'DemoStore carregado e exportável.' : 'DemoStore indisponível.');
    setCard('eventBus', window.BarberSyncEventBus ? 'ok' : 'fail', window.BarberSyncEventBus ? 'EventBus carregado com histórico local.' : 'EventBus indisponível.');
    try {
      localStorage.setItem('barbersync.diagnostics.probe', new Date().toISOString());
      localStorage.removeItem('barbersync.diagnostics.probe');
      setCard('localStorage', 'ok', 'Persistência local disponível.');
    } catch (error) {
      setCard('localStorage', 'fail', `Falha localStorage: ${error?.message || error}`);
    }
  };
  async function runDiagnostics() {
    runLocalChecks();
    for (const check of checks) {
      const result = await safeFetch(check.url);
      setCard(check.key, result.ok ? 'ok' : 'fail', result.ok ? `${check.label} respondeu HTTP ${result.status}.` : `${check.label} falhou: ${result.error || `HTTP ${result.status}`}`);
    }
    const tests = await window.BarberSyncDemoStoreTests?.run?.();
    if (tests) renderTests(tests);
    window.BarberSyncEventBus?.emit?.('diagnostics:completed', { module: 'Diagnóstico', title: 'Diagnóstico executado', checks: checks.length });
    renderEvents();
    window.AdminToast?.show?.('Diagnóstico BarberSync executado.', 'success');
  }
  document.addEventListener('click', async (event) => {
    if (event.target.closest('[data-run-diagnostics]') || event.target.closest('[data-run-demo-store-tests]')) await runDiagnostics();
    if (event.target.closest('[data-reset-demo-store]')) { window.BarberSyncDemoStore?.resetAll?.(); await runDiagnostics(); }
    if (event.target.closest('[data-export-diagnostics]')) {
      const payload = { generatedAt: new Date().toISOString(), events: window.BarberSyncEventBus?.last?.(20) || [], state: window.BarberSyncDemoStore?.dashboardSummary?.() || null };
      const blob = new Blob([JSON.stringify(payload, null, 2)], { type: 'application/json' });
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob); link.download = 'barbersync-diagnostics-demo.json'; link.click(); URL.revokeObjectURL(link.href);
    }
  });
  window.addEventListener('barbersync:event', renderEvents);
  document.addEventListener('DOMContentLoaded', runDiagnostics);
})();
