(() => {
  const store = () => window.BarberSyncDemoStore;
  const toast = (message, type = 'success') => window.AdminToast?.show?.(message, type);
  const esc = value => String(value ?? '').replace(/[&<>'"]/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[ch]));
  const currency = value => `R$ ${Number(value || 0).toFixed(2).replace('.', ',')}`;
  const first = moduleName => store()?.get(moduleName)?.[0];

  const statusNext = {
    'Agendado': { label: 'Confirmar', status: 'Confirmado' },
    'Lista de espera': { label: 'Confirmar', status: 'Confirmado' },
    'Confirmado': { label: 'Check-in', status: 'Check-in' },
    'Check-in': { label: 'Iniciar', status: 'Em atendimento' },
    'Em atendimento': { label: 'Finalizar', status: 'Atendimento finalizado' },
    'Atendimento finalizado': { label: 'Abrir comanda', status: 'Comanda aberta', order: true },
    'Comanda aberta': { label: 'Pagar', pay: true }
  };

  function ensureDemoStore() { return !!store(); }
  function updateAppointment(id, status) {
    store().patch('appointments', id, { status });
    if (status === 'Comanda aberta') store().createServiceOrderFromAppointment(id);
    toast(`Agendamento atualizado para ${status}.`);
    renderOperations();
  }
  function payOrder(id, method = 'PIX') {
    const order = store().payServiceOrder(id, { method });
    if (order) toast(`Pagamento ${method} registrado. Recibo ${order.receiptNumber}.`);
    renderOperations();
  }
  function createQuickClient() {
    const index = store().get('clients').length + 1;
    const client = store().add('clients', { name: `Cliente Demo ${index}`, phone: `(11) 90000-${String(index).padStart(4, '0')}`, email: `cliente${index}@demo.com`, segment: 'Novo', status: 'Ativo', cashback: 0, score: 70, nextBestAction: 'Criar primeiro agendamento' });
    toast('Cliente criado e disponível para agendamento.');
    return client;
  }
  function createQuickAppointment(clientId) {
    const client = store().find('clients', clientId) || first('clients') || createQuickClient();
    const service = first('services');
    const professional = first('professionals');
    const appointment = store().createLinkedAppointment(client.id, service.id, professional.id, { time: '15:30', status: 'Agendado', channel: 'Admin' });
    toast(`Agendamento criado para ${client.name}.`);
    renderOperations();
    return appointment;
  }
  function createQuickCampaign(audience = 'Clientes sem retorno') {
    const campaign = store().add('campaigns', { name: `Campanha ${audience} ${new Date().toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' })}`, audience, period: 'Hoje + 7 dias', channel: 'WhatsApp', status: 'Ativa', result: 'Simulação enviada', impacted: 88, estimatedRevenue: 3600, funnel: '88 > 44 > 18 > 8' });
    store().get('clients').filter(c => c.segment === 'Inativo' || c.status === 'Reativar' || audience.includes('VIP')).slice(0, 8).forEach(c => store().add('timeline', { clientId: c.id, message: `Campanha ${campaign.name} enviada.`, createdAt: new Date().toISOString() }));
    toast('Campanha criada e timeline dos clientes atualizada.');
    renderOperations();
    return campaign;
  }
  function generateReplenishment() {
    const critical = store().filter('products', 'critical');
    const message = critical.length ? `Pedido de reposição demo gerado para ${critical.map(p => p.name).join(', ')}.` : 'Sem itens críticos para reposição.';
    toast(message, critical.length ? 'success' : 'info');
    sessionStorage.setItem('BarberSync:lastReplenishment', JSON.stringify({ items: critical, createdAt: new Date().toISOString() }));
    return critical;
  }

  function column(title, items, empty, body) {
    return `<article class="operation-column"><h3>${title}<span>${items.length}</span></h3>${items.length ? items.map(body).join('') : `<div class="operation-empty">${empty}</div>`}</article>`;
  }
  function appointmentCard(a) {
    const next = statusNext[a.status] || {};
    const order = store().get('serviceOrders').find(o => o.appointmentId === a.id);
    return `<div class="operation-card"><div><strong>${esc(a.client)}</strong><span>${esc(a.time)} • ${esc(a.service)}</span><small>${esc(a.professional)} • ${esc(a.channel || 'Admin')}</small></div><span class="badge badge-info">${esc(a.status)}</span><div class="operation-actions">${next.label ? `<button class="btn btn-primary btn-sm" data-flow-appointment="${a.id}" data-next-status="${next.status || ''}" data-flow-order="${next.order ? '1' : ''}" data-flow-pay="${next.pay ? '1' : ''}">${next.label}</button>` : ''}${order ? `<button class="btn btn-light btn-sm" data-flow-pay-order="${order.id}">Pagar</button>` : `<button class="btn btn-light btn-sm" data-flow-open-order="${a.id}">Abrir comanda</button>`}<button class="btn btn-light btn-sm" data-flow-review="${a.clientId || ''}" data-flow-order-id="${order?.id || ''}">Avaliar</button></div></div>`;
  }
  function orderCard(o) {
    return `<div class="operation-card"><div><strong>${esc(o.name || o.id)} • ${esc(o.client)}</strong><span>${esc(o.service)} • ${currency(o.total)}</span><small>${esc(o.paymentMethod || 'PIX')} • ${esc(o.channel || 'Admin')}</small></div><span class="badge ${o.status === 'Paga' ? 'badge-success' : 'badge-warning'}">${esc(o.status)}</span><div class="operation-actions">${o.status !== 'Paga' ? `<button class="btn btn-primary btn-sm" data-flow-pay-order="${o.id}">Pagar PIX</button>` : `<button class="btn btn-light btn-sm" data-flow-receipt="${o.id}">Recibo</button>`}</div></div>`;
  }
  function renderOperations() {
    if (!ensureDemoStore()) return;
    const root = document.querySelector('[data-operations-board]');
    const mini = document.querySelector('[data-demo-live-summary]');
    const appointments = store().get('appointments');
    const orders = store().get('serviceOrders');
    const products = store().filter('products', 'critical');
    if (mini) mini.innerHTML = `<span>Agenda: <strong>${appointments.length}</strong></span><span>Comandas abertas: <strong>${orders.filter(o => o.status !== 'Paga').length}</strong></span><span>Receita paga: <strong>${currency(orders.filter(o => o.status === 'Paga').reduce((s, o) => s + Number(o.total || 0), 0))}</strong></span><span>Estoque crítico: <strong>${products.length}</strong></span>`;
    if (!root) return;
    const by = statuses => appointments.filter(a => statuses.includes(a.status));
    root.innerHTML = [
      column('Fila de chegada', by(['Agendado', 'Confirmado', 'Lista de espera']), 'Nenhum cliente na fila.', appointmentCard),
      column('Check-ins', by(['Check-in']), 'Aguardando check-ins.', appointmentCard),
      column('Em atendimento', by(['Em atendimento']), 'Nenhum atendimento em execução.', appointmentCard),
      column('Comandas abertas', orders.filter(o => o.status === 'Aberta'), 'Sem comandas abertas.', orderCard),
      column('Aguardando pagamento', orders.filter(o => o.status === 'Aguardando pagamento' || o.status === 'Em pagamento'), 'Sem pagamentos pendentes.', orderCard),
      column('Finalizados', orders.filter(o => o.status === 'Paga'), 'Nenhum atendimento finalizado.', orderCard),
      column('Alertas', products, 'Sem alertas críticos.', p => `<div class="operation-card"><strong>${esc(p.name)}</strong><span>${p.stock ?? p.quantity}/${p.minimumStock} unidades</span><button class="btn btn-light btn-sm" data-flow-replenish>Gerar reposição</button></div>`)
    ].join('');
  }

  function bind() {
    document.addEventListener('click', event => {
      const button = event.target.closest('[data-flow-appointment],[data-flow-open-order],[data-flow-pay-order],[data-flow-review],[data-flow-replenish],[data-flow-receipt],[data-flow-quick-client],[data-flow-quick-appointment],[data-flow-quick-campaign],[data-flow-reset],[data-flow-scenario]');
      if (!button || !ensureDemoStore()) return;
      if (button.dataset.flowAppointment) {
        if (button.dataset.flowPay === '1') { const order = store().createServiceOrderFromAppointment(button.dataset.flowAppointment); if (order) payOrder(order.id); }
        else updateAppointment(button.dataset.flowAppointment, button.dataset.nextStatus);
      }
      if (button.dataset.flowOpenOrder) { store().createServiceOrderFromAppointment(button.dataset.flowOpenOrder); toast('Comanda aberta automaticamente.'); renderOperations(); }
      if (button.dataset.flowPayOrder) payOrder(button.dataset.flowPayOrder, 'PIX');
      if (button.dataset.flowReview) { store().generateReview(button.dataset.flowReview, button.dataset.flowOrderId); toast('Avaliação demo registrada.'); renderOperations(); }
      if (button.hasAttribute('data-flow-replenish')) generateReplenishment();
      if (button.dataset.flowReceipt) toast('Recibo premium pronto para impressão/compartilhamento.');
      if (button.hasAttribute('data-flow-quick-client')) createQuickClient();
      if (button.hasAttribute('data-flow-quick-appointment')) createQuickAppointment(button.dataset.clientId);
      if (button.dataset.flowQuickCampaign) createQuickCampaign(button.dataset.flowQuickCampaign);
      if (button.hasAttribute('data-flow-reset')) { store().resetAll(); renderOperations(); }
      if (button.dataset.flowScenario) { store().loadScenario(button.dataset.flowScenario); renderOperations(); }
    });
    window.addEventListener('barbersync:demo-store-changed', renderOperations);
    document.addEventListener('DOMContentLoaded', renderOperations);
  }

  window.BarberSyncOperationFlow = { renderOperations, updateAppointment, payOrder, createQuickClient, createQuickAppointment, createQuickCampaign, generateReplenishment };
  bind();
})();
