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

(() => { const store=()=>window.BarberSyncDemoStore; const bus=()=>window.BarberSyncEventBus; const row=(k,v)=>`<div class="saas-row"><strong>${k}</strong><span>${v}</span></div>`; function render(){ if(!document.querySelector('[data-operations-20]')) return; const s=store(); if(!s) return; const origin=document.querySelector('[data-op-filter-origin]')?.value||''; const status=document.querySelector('[data-op-filter-status]')?.value||''; let a=s.get('appointments'); if(origin) a=a.filter(x=>x.channel===origin); if(status) a=a.filter(x=>x.status===status); document.querySelector('[data-op-metrics]').innerHTML=row('Tempo médio de espera','8 min')+row('Tempo médio atendimento','42 min')+row('Comandas pendentes',s.get('serviceOrders').filter(o=>o.status!=='Paid').length)+row('Pagamentos pendentes',s.get('serviceOrders').filter(o=>o.status!=='Paid').length); document.querySelector('[data-op-cards]').innerHTML=a.map(x=>`<div class="saas-row"><strong>${x.time||'Agora'} • ${x.client||'Cliente'} • ${x.status}</strong><span class="saas-actions"><button class="btn btn-light btn-sm" data-op-action="confirm" data-id="${x.id}">Confirmar</button><button class="btn btn-light btn-sm" data-op-action="checkin" data-id="${x.id}">Check-in</button><button class="btn btn-light btn-sm" data-op-action="start" data-id="${x.id}">Iniciar</button><button class="btn btn-light btn-sm" data-op-action="finish" data-id="${x.id}">Finalizar</button><button class="btn btn-light btn-sm" data-op-action="order" data-id="${x.id}">Comanda</button><button class="btn btn-light btn-sm" data-op-action="pay" data-id="${x.id}">Pagar</button><button class="btn btn-light btn-sm" data-op-action="review" data-id="${x.id}">Avaliar</button><button class="btn btn-light btn-sm" data-op-action="cancel" data-id="${x.id}">Cancelar</button></span></div>`).join('') || '<p>Nenhum card para o filtro selecionado.</p>'; } document.addEventListener('change',e=>{if(e.target.matches('[data-op-filter-origin],[data-op-filter-status]'))render()}); document.addEventListener('click',e=>{ const b=e.target.closest('[data-op-action]'); if(!b) return; const s=store(), id=b.dataset.id, act=b.dataset.opAction; if(act==='confirm')s.confirmAppointment(id); if(act==='checkin')s.checkInAppointment(id); if(act==='start')s.startAppointment(id); if(act==='finish')s.finishAppointment(id); if(act==='cancel')s.cancelAppointment(id); if(act==='order'){const apt=s.find('appointments',id); const o=s.openServiceOrder({appointmentId:id,clientId:apt?.clientId,professionalId:apt?.professionalId,channel:apt?.channel}); s.addServiceToOrder(o.id,{serviceId:apt?.serviceId,name:apt?.service,amount:apt?.value||65});} if(act==='pay'){const o=s.get('serviceOrders').find(o=>o.appointmentId===id)||s.get('serviceOrders')[0]; if(o)s.payServiceOrder(o.id,{method:'PIX'});} if(act==='review')s.createReview({client:'Cliente operação',rating:5,nps:10}); s.refreshDashboard(); render(); }); bus()?.on('*',render); document.addEventListener('DOMContentLoaded',render); })();

(() => {
  const store=()=>window.BarberSyncDemoStore;
  function renderCommercialFlowOperation(){
    const host=document.querySelector('[data-commercial-flow-operation]'); if(!host||!store()) return;
    const s=store(), f=s.exportState().commercialFlow||{}, apt=f.appointmentId?s.find('appointments',f.appointmentId):f.appointment, order=f.orderId?s.find('serviceOrders',f.orderId):f.serviceOrder;
    if(!f.id){ host.innerHTML='<p>Execute /Admin/CommercialFlow para ver a jornada na Operação ao Vivo.</p>'; return; }
    const stage=order?.status==='Paid'?'Finalizado':order?.status==='Open'?'Aguardando pagamento':apt?.status==='InService'?'Em atendimento':apt?.status==='CheckedIn'?'Check-in':'Agendado';
    host.innerHTML=`<article class="enterprise-card wide"><h3>Fluxo Comercial 15.0 na Operação</h3><div class="status-flow"><span>Agendado</span><span>Check-in</span><span>Em atendimento</span><span>Comanda aberta</span><span>Aguardando pagamento</span><span>Finalizado</span></div><p><strong>Status atual:</strong> ${stage}</p><p>${apt?.client||f.client?.name||'Cliente demo'} • ${apt?.service||'-'} • ${order?`Comanda #${order.number}`:'Comanda pendente'}</p><a class="btn btn-light" href="/Admin/CommercialFlow">Abrir fluxo</a></article>`;
  }
  window.addEventListener('barbersync:store-changed',renderCommercialFlowOperation); window.BarberSyncEventBus?.on('*',renderCommercialFlowOperation); document.addEventListener('DOMContentLoaded',renderCommercialFlowOperation);
})();
