(() => {
  const money = v => Number(v || 0).toLocaleString('pt-BR', { style:'currency', currency:'BRL' });
  function render() {
    const host = document.querySelector('[data-client360]'); if (!host || !window.BarberSyncDemoStore) return;
    const s = window.BarberSyncDemoStore, clients = s.get('clients'), appts=s.get('appointments'), orders=s.get('serviceOrders'), pays=s.get('payments'), reviews=s.get('reviews'), campaigns=s.get('campaigns');
    host.innerHTML = clients.map(c => {
      const timeline = [
        'Cliente cadastrado', ...appts.filter(a=>a.clientId===c.id).map(a=>`Agendamento ${a.status}: ${a.service}`),
        ...orders.filter(o=>o.clientId===c.id).map(o=>`Comanda #${o.number} ${o.status} ${money(o.paid||o.total)}`),
        ...pays.filter(p=>orders.some(o=>o.id===p.orderId && o.clientId===c.id)).map(p=>`Pagamento ${p.method} ${money(p.amount)}`),
        `Cashback disponível ${money(c.cashback)}`, ...reviews.filter(r=>r.clientId===c.id).map(r=>`Avaliação NPS ${r.nps}`),
        ...campaigns.slice(0,1).map(cmp=>`Campanha enviada: ${cmp.name}`)
      ];
      return `<article class="enterprise-card"><h3>${c.name}</h3><p>${c.segment} • ${c.phone} • ${money(c.totalSpent)}</p><div class="tag-list"><span class="tag">Cashback ${money(c.cashback)}</span><span class="tag">${c.preferredService||'Serviço favorito'}</span></div><div class="timeline-demo">${timeline.map(t=>`<div>${t}</div>`).join('')}</div><div class="quick-actions-grid"><button class="btn btn-light" data-client-action="appointment" data-id="${c.id}">Novo agendamento</button><button class="btn btn-light" data-client-action="order" data-id="${c.id}">Abrir comanda</button><button class="btn btn-light" data-client-action="coupon" data-id="${c.id}">Enviar cupom</button><button class="btn btn-light" data-client-action="note" data-id="${c.id}">Criar observação</button><a class="btn btn-light" href="/Admin/ServiceOrders">Ver recibos</a></div></article>`;
    }).join('');
  }
  document.addEventListener('click', e => { const b=e.target.closest('[data-client-action]'); if(!b) return; const s=window.BarberSyncDemoStore; if(b.dataset.clientAction==='appointment') s.createAppointment({clientId:b.dataset.id}); if(b.dataset.clientAction==='order') s.openServiceOrder({clientId:b.dataset.id}); if(b.dataset.clientAction==='coupon') s.createCampaign({audience:'Cliente selecionado', createCoupon:true}); if(b.dataset.clientAction==='note') { const c=s.find('clients', b.dataset.id); s.patch('clients', b.dataset.id, { notes:[...(c.notes||[]),'Observação criada na demo 4.0'] }); window.BarberSyncEventBus?.emit('client:updated',{client:c}); } render(); });
  window.addEventListener('barbersync:store-changed', render); document.addEventListener('DOMContentLoaded', render);
})();
