(() => {
  const modules = ['Dashboard','Clientes','Agenda','Comandas','Estoque','Campanhas','Copilot','PublicWeb','Totem','Mobile'];
  const scenarios = ['salão-novo','dia-movimentado','estoque-crítico','campanha-retorno','totem-em-uso','alto-faturamento','cliente-vip','agenda-cheia','operação-com-alerta'];
  const checklist = ['Lead criado no PublicWeb','Cliente convertido','Agendamento confirmado','Check-in realizado','Atendimento iniciado','Comanda aberta','Serviço/produto adicionado','Pagamento recebido','Recibo gerado','Estoque baixado','Cashback gerado','Avaliação recebida','Campanha criada','Copilot executou ação','Totem concluiu fluxo','Dashboard atualizado'];
  const store = () => window.BarberSyncDemoStore;
  const money = v => Number(v||0).toLocaleString('pt-BR',{style:'currency',currency:'BRL'});
  function render() {
    if (!document.querySelector('[data-demo5-status]') || !store()) return;
    const summary = store().dashboardSummary();
    document.querySelector('[data-demo5-status]').innerHTML = modules.map(m=>`<div class="demo5-status-item"><span class="demo5-dot"></span><strong>${m}</strong><small>Sincronizado via DemoStore</small></div>`).join('');
    document.querySelector('[data-demo5-scenarios]').innerHTML = scenarios.map(s=>`<button class="btn btn-light" data-demo5-scenario="${s}">${s.replaceAll('-',' ')}</button>`).join('');
    document.querySelector('[data-demo5-checklist]').innerHTML = checklist.map((c,i)=>`<label><input type="checkbox" ${i < Math.min(checklist.length, summary.paidOrders + summary.leads + summary.kiosk + 6) ? 'checked' : ''}/> ${c}</label>`).join('');
    const events = window.BarberSyncEventBus?.history().slice(-12).reverse() || [];
    document.querySelector('[data-demo5-events]').innerHTML = events.map(e=>`<div class="demo5-event"><strong>${e.name}</strong><br><small>${new Date(e.at).toLocaleString('pt-BR')}</small></div>`).join('') || '<p class="text-muted">Sem eventos ainda.</p>';
    document.querySelector('[data-demo5-timeline]').innerHTML = [`Receita ${money(summary.revenue)}`,`${summary.appointments} agendamentos`,`${summary.criticalStock} itens críticos`,`${summary.campaigns} campanhas ativas`,`${summary.reviews} avaliações`].map((x,i)=>`<article><strong>Passo ${i+1}</strong><p>${x}</p></article>`).join('');
  }
  function runFlow(){ const s=store(); const lead=s.createLeadFromPublicWeb({ name:'Cliente Demo 5.0', phone:'(11) 90000-5050', service:'Corte + Barba' }); const c=s.convertLeadToClient(lead.id); const apt=s.createAppointment({ clientId:c.id, serviceId:'srv-001', professionalId:'pro-001', channel:'PublicWeb', time:'16:30' }); s.confirmAppointment(apt.id); s.checkInAppointment(apt.id); s.startAppointment(apt.id); const o=s.openServiceOrder({ appointmentId:apt.id }); s.addServiceToOrder(o.id,{ name:'Corte + Barba', amount:95 }); s.addProductToOrder(o.id,{ productId:'prd-001', quantity:1 }); s.payServiceOrder(o.id,{ method:'PIX' }); s.createCampaign({ name:'Retorno pós-demo', audience:c.name, status:'Ativa', sent:1, opened:1, conversions:0 }); s.refreshDashboard(); }
  document.addEventListener('click', e=>{ const sc=e.target.closest('[data-demo5-scenario]')?.dataset.demo5Scenario; if(sc) store()?.loadScenario(sc); if(e.target.closest('[data-demo5-reset]')) store()?.resetAll(); if(e.target.closest('[data-demo5-flow]')) runFlow(); if(e.target.closest('[data-demo5-start]')) { store()?.loadScenario('dia-movimentado'); runFlow(); } render(); });
  window.addEventListener('barbersync:store-changed', render); window.addEventListener('barbersync:event', render); document.addEventListener('DOMContentLoaded', render);
})();
