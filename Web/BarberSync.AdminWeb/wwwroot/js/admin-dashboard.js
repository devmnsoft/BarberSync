(() => {
  const money = v => Number(v || 0).toLocaleString('pt-BR', { style:'currency', currency:'BRL' });
  const store = () => window.BarberSyncDemoStore;
  function render() {
    if (!store()) return;
    const summary = store().dashboardSummary();
    const kpis = document.querySelector('[data-dashboard-kpis]');
    if (kpis) {
      const items = [
        ['Receita hoje', money(summary.revenue), 'Pagamentos no DemoStore'], ['Leads PublicWeb hoje', summary.publicLeads, 'Importáveis pela Central da Demo'], ['Atendimentos Totem', summary.kioskAttendances, 'Fluxos do Kiosk'],
        ['Comandas pagas hoje', summary.paidOrders, 'PDV integrado'], ['Cashback gerado', money(summary.cashback), 'Fidelidade atualizada'], ['Avaliações recebidas', summary.reviews, 'NPS demo'],
        ['Conversão de agenda', `${summary.conversion}%`, 'Check-in/atendimento'], ['Estoque crítico', summary.criticalStock, 'Baixa automática'], ['Clientes ativos', summary.clients, 'Cliente 360'],
        ...summary.channels.map(c => [`Receita ${c.channel}`, money(c.revenue), 'Receita por canal'])
      ];
      kpis.innerHTML = items.map(i => `<article class="kpi-card"><div class="kpi-icon">📊</div><div><p class="kpi-label">${i[0]}</p><strong class="kpi-value">${i[1]}</strong><span class="kpi-variation">${i[2]}</span></div></article>`).join('');
    }
    const next = document.querySelector('[data-next-appointments]'); if (next) next.innerHTML = store().get('appointments').slice(0,5).map(a => `<li>${a.time} ${a.client} • ${a.status}</li>`).join('');
    const stock = document.querySelector('[data-stock-critical]'); if (stock) stock.innerHTML = store().get('products').filter(p => p.stock <= p.minStock).map(p => `<li>${p.name}: ${p.stock}/${p.minStock}</li>`).join('') || '<li>Sem itens críticos.</li>';
    const copilot = document.querySelector('[data-copilot-list]'); if (copilot) copilot.innerHTML = store().get('copilotSuggestions').map(s => `<li>${s.title}</li>`).join('');
    const events = document.querySelector('[data-dashboard-events]'); if (events) events.innerHTML = store().get('dashboardEvents').slice(0,8).map(e => `<li><strong>${e.label || e.eventName}</strong><span>${new Date(e.at).toLocaleTimeString('pt-BR')}</span></li>`).join('');
  }
  document.addEventListener('click', e => { if (e.target.closest('[data-dashboard-refresh]')) store()?.refreshDashboard(); if (e.target.closest('[data-demo-action]')) { const a=e.target.closest('[data-demo-action]').dataset.demoAction; window.AdminToast?.show?.(a, 'success'); } });
  window.addEventListener('barbersync:store-changed', render); window.addEventListener('barbersync:dashboard-refresh', render); document.addEventListener('DOMContentLoaded', render);
})();
