(() => {
  const store = () => window.BarberSyncDemoStore;
  const bus = () => window.BarberSyncEventBus;
  const $ = s => document.querySelector(s);
  const stateKey = 'barbersync.demo.leadToCash.current';
  const current = () => JSON.parse(sessionStorage.getItem(stateKey) || '{}');
  const setCurrent = data => sessionStorage.setItem(stateKey, JSON.stringify({ ...current(), ...data }));
  const steps = [ ['Lead','leads'], ['Cliente','clients'], ['Agendamento','appointments'], ['Atendimento','attendances'], ['Comanda','serviceOrders'], ['Pagamento','payments'], ['Fidelidade','loyalty'], ['Avaliação','reviews'] ];
  function render() {
    if (!$('[data-ltc-stepper]')) return;
    const s=store(); const counts=Object.fromEntries(steps.map(([label,m])=>[label,s?.get(m).length || 0]));
    $('[data-ltc-stepper]').innerHTML = steps.map(([label],i)=>`<article class="lead-step"><div class="step-index">${i+1}</div><h3>${label}</h3><strong>${counts[label]}</strong><p>Conversão ${Math.min(98, Math.max(35, counts[label]*12))}% • tempo médio demo ${i+1} min.</p></article>`).join('');
    $('[data-ltc-metrics]').innerHTML = `<div class="saas-row"><strong>Lead → Agendamento</strong><span>${counts.Agendamento}/${Math.max(1,counts.Lead)}</span></div><div class="saas-row"><strong>Atendimento → Pagamento</strong><span>${counts.Pagamento}/${Math.max(1,counts.Atendimento)}</span></div><div class="saas-row"><strong>Pagamentos → Avaliações</strong><span>${counts.Avaliação}/${Math.max(1,counts.Pagamento)}</span></div>`;
    $('[data-ltc-events]').innerHTML = (bus()?.last(12) || []).map(e=>`<li><strong>${e.title}</strong><br><small>${e.module} • ${e.eventName}</small></li>`).join('');
  }
  function runAction(action) {
    const s=store(); if(!s) return; let c=current();
    if(action==='lead'){ const lead=s.createLeadFromPublicWeb({ name:'Lead Demo 6.0', phone:'(11) 95555-6060', service:'Corte + Barba', source:'PublicWeb' }); setCurrent({ leadId:lead.id }); }
    if(action==='client'){ c=current(); const leadId=c.leadId || s.get('leads')[0]?.id; const cli=s.convertLeadToClient(leadId); setCurrent({ clientId:cli?.id }); }
    if(action==='appointment'){ c=current(); const apt=s.createAppointment({ clientId:c.clientId || s.get('clients')[0]?.id, serviceId:'srv-001', professionalId:'pro-001', channel:'PublicWeb', status:'Scheduled' }); setCurrent({ appointmentId:apt.id }); }
    if(action==='checkin'){ c=current(); s.checkInAppointment(c.appointmentId || s.get('appointments')[0]?.id); }
    if(action==='start'){ c=current(); s.startAppointment(c.appointmentId || s.get('appointments')[0]?.id); }
    if(action==='order'){ c=current(); const order=s.openServiceOrder({ appointmentId:c.appointmentId || s.get('appointments')[0]?.id, clientId:c.clientId || s.get('clients')[0]?.id, professionalId:'pro-001', channel:'Admin' }); s.addServiceToOrder(order.id,{ serviceId:'srv-001', name:'Corte + Barba', amount:95 }); s.addProductToOrder(order.id,{ productId:'prd-001', quantity:1 }); setCurrent({ orderId:order.id }); }
    if(action==='pay'){ c=current(); s.payServiceOrder(c.orderId || s.get('serviceOrders').find(o=>o.status!=='Paid')?.id || s.get('serviceOrders')[0]?.id,{ method:'PIX' }); }
    if(action==='review'){ c=current(); s.createReview({ clientId:c.clientId || s.get('clients')[0]?.id, client:'Cliente Lead to Cash', rating:5, nps:10, comment:'Fluxo integrado aprovado.' }); }
    if(action==='campaign'){ s.createCampaign({ name:'Retorno pós-atendimento Lead to Cash', audience:'Clientes atendidos hoje', channel:'WhatsApp', status:'Ativa', sent:20, opened:14, conversions:4, revenue:480 }); }
    s.refreshDashboard(); render();
  }
  document.addEventListener('click', e => { const action=e.target.closest('[data-ltc-action]')?.dataset.ltcAction; if(action) runAction(action); if(e.target.closest('[data-ltc-run]')) ['lead','client','appointment','checkin','start','order','pay','review','campaign'].forEach(runAction); });
  bus()?.on('*', render); document.addEventListener('DOMContentLoaded', render);
})();
