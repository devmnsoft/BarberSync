(() => {
  const statuses = [
    ['Scheduled','Agendados'], ['CheckedIn','Check-in'], ['InService','Em atendimento'], ['OpenOrder','Comanda aberta'], ['AwaitingPayment','Aguardando pagamento'], ['Finished','Finalizados']
  ];
  const money = v => Number(v || 0).toLocaleString('pt-BR', { style:'currency', currency:'BRL' });
  const store = () => window.BarberSyncDemoStore;
  function orderForAppointment(apt) { return store().get('serviceOrders').find(o => o.appointmentId === apt.id); }
  function bucket(apt) { const order = orderForAppointment(apt); if (order?.status === 'Open') return order.total > 0 ? 'AwaitingPayment' : 'OpenOrder'; if (order?.status === 'Paid' || apt.status === 'Finished') return 'Finished'; return apt.status || 'Scheduled'; }
  function actions(apt) {
    const order = orderForAppointment(apt);
    const buttons = [];
    if (apt.status === 'Scheduled') buttons.push(['confirm','Confirmar']);
    if (['Scheduled','Confirmed'].includes(apt.status)) buttons.push(['checkin','Check-in']);
    if (apt.status === 'CheckedIn') buttons.push(['start','Iniciar']);
    if (apt.status === 'InService') buttons.push(['finish','Finalizar atendimento']);
    if (!order) buttons.push(['open-order','Abrir comanda']);
    if (order && order.items.length === 0) buttons.push(['add-product','Adicionar produto']);
    if (order && order.status !== 'Paid') buttons.push(['pay','Pagar PIX']);
    buttons.push(['review','Avaliar']);
    return buttons.map(([a,t]) => `<button class="btn btn-light" data-op-action="${a}" data-id="${apt.id}">${t}</button>`).join('');
  }
  function card(apt) { const order = orderForAppointment(apt); return `<article class="feature-card operation-card"><span class="badge badge-info">${apt.channel || 'Admin'}</span><h4>${apt.client}</h4><p>${apt.service} • ${apt.professional}</p><p><strong>${apt.time}</strong> • ${apt.status} • ${money(apt.value || order?.total)}</p><div class="quick-actions-grid">${actions(apt)}</div></article>`; }
  function render() {
    const board = document.querySelector('[data-operations-board]'); if (!board || !store()) return;
    const appointments = store().get('appointments').sort((a,b) => String(a.time).localeCompare(String(b.time)));
    board.innerHTML = statuses.map(([key,label]) => `<section class="panel"><div class="panel-header"><h3>${label}</h3><span class="badge badge-success">${appointments.filter(a => bucket(a) === key).length}</span></div>${appointments.filter(a => bucket(a) === key).map(card).join('') || '<p class="text-muted">Sem cards.</p>'}</section>`).join('');
    const summary = store().dashboardSummary();
    const live = document.querySelector('[data-demo-live-summary]');
    if (live) live.innerHTML = `<strong>Operação 4.0:</strong> ${summary.appointments} agendamentos, ${summary.paidOrders} comandas pagas, ${money(summary.revenue)} em receita, ${money(summary.cashback)} cashback.`;
  }
  document.addEventListener('click', e => {
    const btn = e.target.closest('[data-op-action]'); if (!btn || !store()) return;
    const apt = store().find('appointments', btn.dataset.id); let order = orderForAppointment(apt);
    if (btn.dataset.opAction === 'confirm') store().confirmAppointment(apt.id);
    if (btn.dataset.opAction === 'checkin') store().checkInAppointment(apt.id);
    if (btn.dataset.opAction === 'start') store().startAppointment(apt.id);
    if (btn.dataset.opAction === 'finish') store().finishAppointment(apt.id);
    if (btn.dataset.opAction === 'open-order') { order = store().openServiceOrder({ appointmentId:apt.id, clientId:apt.clientId, professionalId:apt.professionalId, channel:apt.channel }); store().addServiceOrderItem(order.id, { type:'service', name:apt.service, amount:apt.value || 65 }); }
    if (btn.dataset.opAction === 'add-product') { if (!order) order = store().openServiceOrder({ appointmentId:apt.id }); store().addServiceOrderItem(order.id, { type:'product', productId:'prd-001', name:'Pomada Modeladora', quantity:1, amount:39 }); }
    if (btn.dataset.opAction === 'pay') { if (!order) order = store().openServiceOrder({ appointmentId:apt.id }); if (!order.items.length) store().addServiceOrderItem(order.id, { type:'service', name:apt.service, amount:apt.value || 65 }); store().payServiceOrder(order.id, { method:'PIX' }); }
    if (btn.dataset.opAction === 'review') store().createReview({ clientId:apt.clientId, rating:5, nps:10 });
    render(); store().refreshDashboard();
  });
  document.addEventListener('click', e => { if (e.target.closest('[data-flow-reset]')) store().resetAll(); const s=e.target.closest('[data-flow-scenario]')?.dataset.flowScenario; if(s) store().loadScenario(s); });
  window.addEventListener('barbersync:store-changed', render); document.addEventListener('DOMContentLoaded', render);
})();
