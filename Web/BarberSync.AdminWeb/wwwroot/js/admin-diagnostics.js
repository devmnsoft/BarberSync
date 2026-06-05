(() => {
  const checks = [
    { key: 'api', label: 'API', url: '/AdminApi/api-health' },
    { key: 'adminApi', label: 'AdminApi', url: '/AdminApi/dashboard' },
    { key: 'publicApi', label: 'PublicApi', url: 'http://localhost:8082/PublicApi/services' },
    { key: 'kioskApi', label: 'KioskApi', url: 'http://localhost:8083/KioskApi/services?deviceCode=KIOSK-DEMO-001' },
    { key: 'swagger', label: 'Swagger', url: '/AdminApi/swagger.json' },
    { key: 'assets', label: 'Assets', url: '/css/admin-design-system.css', accept: 'text/css,*/*' },
    { key: 'publicWeb', label: 'PublicWeb', url: 'http://localhost:8082/', accept: 'text/html,*/*' },
    { key: 'kiosk', label: 'Kiosk', url: 'http://localhost:8083/Kiosk/Services', accept: 'text/html,*/*' }
  ];

  const statusClass = (result) => (result.ok ? 'ok' : (result.status === 0 ? 'attention' : 'fail'));
  const safeText = (value) => String(value ?? '').replace(/[<>&]/g, char => ({ '<': '&lt;', '>': '&gt;', '&': '&amp;' }[char]));

  const setCard = (key, status, detail) => {
    const card = document.querySelector(`[data-diagnostic-card="${key}"]`);
    const dot = card?.querySelector('.diagnostic-dot');
    const text = document.querySelector(`[data-diagnostic-detail="${key}"]`);
    if (dot) dot.className = `diagnostic-dot ${status}`;
    if (text) text.textContent = detail;
  };

  const safeFetch = async (url, accept = 'application/json,text/css,*/*') => {
    try {
      const response = await fetch(url, { headers: { Accept: accept }, cache: 'no-store' });
      return { ok: response.ok, status: response.status };
    } catch (error) {
      return { ok: false, status: 0, error: error?.message || String(error) };
    }
  };

  const renderTests = (tests) => {
    const root = document.querySelector('[data-demo-store-tests]');
    if (!root) return;
    root.innerHTML = tests.map(test => `<div class="diagnostic-test ${test.passed ? 'pass' : 'fail'}"><div><strong>${safeText(test.name)}</strong><small>${safeText(test.durationMs)} ms • ${safeText(test.detail)}</small></div><span>${test.passed ? 'Passou' : 'Falhou'}</span></div>`).join('');
  };

  const renderEvents = () => {
    const eventsRoot = document.querySelector('[data-diagnostics-events]');
    const errorsRoot = document.querySelector('[data-diagnostics-errors]');
    const events = window.BarberSyncEventBus?.last?.(12) || [];
    if (eventsRoot) {
      eventsRoot.innerHTML = events.length
        ? events.map(e => `<div class="diagnostic-event"><strong>${safeText(e.title || e.eventName)}</strong><small>${safeText(e.module || 'BarberSync')} • ${new Date(e.createdAt || Date.now()).toLocaleString('pt-BR')}</small></div>`).join('')
        : 'Nenhum evento registrado.';
    }
    const errors = events.filter(e => ['danger', 'error', 'fail'].includes(String(e.severity || '').toLowerCase()) || String(e.eventName || '').toLowerCase().includes('error'));
    if (errorsRoot) {
      errorsRoot.innerHTML = errors.length
        ? errors.map(e => `<div class="diagnostic-event"><strong>${safeText(e.title || e.eventName)}</strong><small>${safeText(e.description || '')}</small></div>`).join('')
        : 'Nenhum erro crítico registrado nesta sessão.';
    }
  };

  const runLocalChecks = () => {
    const store = window.BarberSyncDemoStore;
    const bus = window.BarberSyncEventBus;
    setCard('demoStore', store ? 'ok' : 'fail', store ? 'DemoStore carregado e exportável.' : 'DemoStore indisponível.');
    setCard('eventBus', bus ? 'ok' : 'fail', bus ? 'EventBus carregado com histórico local.' : 'EventBus indisponível.');
    try {
      localStorage.setItem('barbersync.diagnostics.probe', new Date().toISOString());
      localStorage.removeItem('barbersync.diagnostics.probe');
      setCard('localStorage', 'ok', 'Persistência local disponível.');
    } catch (error) {
      setCard('localStorage', 'fail', `Falha localStorage: ${error?.message || error}`);
    }
    try {
      const flow = store?.getFullServiceFlow?.();
      const completed = flow?.status === 'Completed' || Boolean(flow?.completedAt);
      setCard('fullServiceFlow', flow ? (completed ? 'ok' : 'attention') : 'fail', flow ? `${completed ? 'Concluído' : 'Disponível'} • etapa ${flow.currentStep || 'Cliente'}.` : 'Fluxo indisponível no DemoStore.');
    } catch (error) {
      setCard('fullServiceFlow', 'fail', `Falha ao ler fluxo: ${error?.message || error}`);
    }
    setCard('docker', 'attention', 'Status Docker é validado pelo Scripts/quality-gate.ps1 e registrado no relatório Docs/quality-gate-last-run.md.');
  };

  async function runDiagnostics() {
    runLocalChecks();
    for (const check of checks) {
      const result = await safeFetch(check.url, check.accept);
      setCard(check.key, statusClass(result), result.ok ? `${check.label} respondeu HTTP ${result.status}.` : `${check.label} requer atenção: ${result.error || `HTTP ${result.status}`}`);
    }
    const tests = await window.BarberSyncDemoStoreTests?.run?.();
    if (tests) renderTests(tests);
    window.BarberSyncEventBus?.emit?.('diagnostics:completed', { module: 'Diagnóstico', title: 'Diagnóstico executado', checks: checks.length, severity: 'success' });
    renderEvents();
    window.AdminToast?.show?.('Diagnóstico BarberSync executado.', 'success');
  }

  document.addEventListener('click', async (event) => {
    if (event.target.closest('[data-run-diagnostics]') || event.target.closest('[data-run-demo-store-tests]')) await runDiagnostics();
    if (event.target.closest('[data-reset-demo-store]')) {
      window.BarberSyncDemoStore?.resetAll?.();
      window.BarberSyncEventBus?.emit?.('diagnostics:storeReset', { module: 'Diagnóstico', title: 'DemoStore resetado', severity: 'warning' });
      await runDiagnostics();
    }
    if (event.target.closest('[data-export-diagnostics]')) {
      const payload = { generatedAt: new Date().toISOString(), events: window.BarberSyncEventBus?.last?.(20) || [], state: window.BarberSyncDemoStore?.dashboardSummary?.() || null, qualityGate: 'Scripts/quality-gate.ps1' };
      const blob = new Blob([JSON.stringify(payload, null, 2)], { type: 'application/json' });
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = 'barbersync-diagnostics-demo.json';
      link.click();
      URL.revokeObjectURL(link.href);
    }
  });

  window.addEventListener('barbersync:event', renderEvents);
  document.addEventListener('DOMContentLoaded', runDiagnostics);
})();
