(() => {
  const $ = s => document.querySelector(s);
  const store = () => window.BarberSyncDemoStore;
  function render() {
    if (!$('[data-demo-status]')) return;
    const summary = store().dashboardSummary();
    $('[data-demo-status]').innerHTML = [`Clientes ${summary.clients}`, `Agenda ${summary.appointments}`, `Comandas pagas ${summary.paidOrders}`, `Receita R$ ${summary.revenue.toFixed(2)}`, `Cashback R$ ${summary.cashback.toFixed(2)}`, `Estoque crítico ${summary.criticalStock}`].map(x => `<span class="tag">${x}</span>`).join('');
    $('[data-demo-events]').innerHTML = store().get('dashboardEvents').slice(0, 15).map(e => `<li><strong>${e.label || e.eventName}</strong><span>${new Date(e.at).toLocaleString('pt-BR')}</span></li>`).join('') || '<li>Nenhum evento.</li>';
  }
  document.addEventListener('click', e => {
    const scenario = e.target.closest('[data-demo-scenario]')?.dataset.demoScenario;
    if (scenario) store().loadScenario(scenario);
    if (e.target.closest('[data-demo-reset]')) store().resetAll();
    if (e.target.closest('[data-demo-export]')) $('[data-demo-json]').value = store().exportState();
    if (e.target.closest('[data-demo-import]')) store().importState($('[data-demo-json]').value);
    if (e.target.closest('[data-demo-import-public]')) store().importPublicLeads();
    if (e.target.closest('[data-demo-import-kiosk]')) store().importKioskAttendance();
    render();
  });
  window.addEventListener('barbersync:store-changed', render);
  document.addEventListener('DOMContentLoaded', render);
})();
