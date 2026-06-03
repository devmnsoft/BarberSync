(() => {
  const store = () => window.BarberSyncDemoStore;
  const bus = () => window.BarberSyncEventBus;
  const $ = sel => document.querySelector(sel);
  const status = (name, value, cls = 'saas-ok') => `<div class="saas-row"><strong>${name}</strong><span class="saas-status ${cls}">${value}</span></div>`;
  const renderEvents = () => {
    const host = $('[data-saas-events]'); if (!host) return;
    const events = bus()?.last(30) || [];
    host.innerHTML = events.length ? events.map(e => `<article class="saas-event ${e.severity}"><strong>${e.title}</strong><p>${e.description}</p><small>${e.module} • ${new Date(e.createdAt).toLocaleString('pt-BR')} • ${e.eventName}</small></article>`).join('') : '<p>Nenhum evento registrado. Execute uma ação da demo.</p>';
  };
  const render = () => {
    if (!$('[data-saas-control-center]')) return;
    const s = store(); const summary = s?.dashboardSummary?.() || {};
    $('[data-platform-status]').innerHTML = ['API','Admin','PublicWeb','Totem','Mobile','Banco','Seq/logs'].map(x => status(x, 'Operacional')).join('');
    $('[data-channel-status]').innerHTML = status('PublicWeb ativo','Online') + status('Totem online','KIOSK-DEMO-001') + status('Mobile habilitado','Ativo') + status('Admin operacional','OK');
    $('[data-readiness]').innerHTML = ['Swagger OK','AdminApi OK','PublicApi OK','KioskApi OK','Static files OK','CSS carregado','JS carregado','Assets OK'].map(x => status(x, 'OK')).join('');
    const kpis = [['Clientes',summary.clients],['Agendamentos',summary.appointments],['Comandas',s?.get('serviceOrders').length],['Pagamentos',s?.get('payments').length],['Campanhas',summary.campaigns],['Avaliações',summary.reviews],['Estoque crítico',summary.criticalStock],['Leads públicos',s?.get('leads').filter(l=>l.source==='PublicWeb').length],['Atendimentos via Totem',summary.kiosk]];
    $('[data-saas-kpis]').innerHTML = kpis.map(([k,v]) => `<div class="saas-kpi"><span>${k}</span><br><strong>${v ?? 0}</strong></div>`).join('');
    renderEvents();
  };
  document.addEventListener('click', e => {
    const scenario = e.target.closest('[data-saas-action]')?.dataset.saasAction;
    if (scenario) { store()?.loadScenario(scenario); store()?.refreshDashboard(); render(); }
    if (e.target.closest('[data-saas-reset]')) { store()?.resetAll(); render(); }
    if (e.target.closest('[data-saas-export]')) { const blob = new Blob([JSON.stringify(store()?.exportState(), null, 2)], { type:'application/json' }); const a = Object.assign(document.createElement('a'), { href:URL.createObjectURL(blob), download:'barbersync-demo-state-v6.json' }); a.click(); URL.revokeObjectURL(a.href); }
    if (e.target.closest('[data-clear-events]')) { bus()?.clearHistory(); renderEvents(); }
  });
  bus()?.on('*', render); document.addEventListener('DOMContentLoaded', render);
})();
