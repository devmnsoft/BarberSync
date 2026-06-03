(() => {
  const STORAGE_KEY = 'barbersync.demo.state.v4';
  const SCHEMA_VERSION = 4;
  const today = new Date().toISOString().slice(0, 10);
  const clone = value => JSON.parse(JSON.stringify(value));
  const uid = prefix => `${prefix}-${Date.now()}-${Math.random().toString(16).slice(2, 8)}`;
  const money = value => Number(value || 0);
  const modules = ['clients','professionals','services','appointments','serviceOrders','products','stockMovements','payments','campaigns','coupons','reviews','loyalty','copilotSuggestions','dashboardEvents','publicLeads','kioskAttendances'];
  const statusMap = { Scheduled: 'Scheduled', Agendado: 'Scheduled', Confirmado: 'Confirmed', Confirmed: 'Confirmed', 'Check-in': 'CheckedIn', CheckedIn: 'CheckedIn', InService: 'InService', 'Em atendimento': 'InService', Finished: 'Finished', Finalizado: 'Finished', Cancelled: 'Cancelled', NoShow: 'NoShow' };

  const seed = () => ({
    schemaVersion: SCHEMA_VERSION,
    scenario: 'default',
    updatedAt: new Date().toISOString(),
    clients: [
      { id:'cli-001', name:'Lucas Almeida', phone:'(11) 98888-1111', email:'lucas@demo.com', segment:'VIP', status:'Ativo', cashback:48, totalSpent:1860, preferredService:'Corte + Barba', preferredProfessional:'Rafael Barber', notes:['Prefere fade baixo e pomada matte.'] },
      { id:'cli-002', name:'Marina Costa', phone:'(11) 97777-2222', email:'marina@demo.com', segment:'Aniversariante', status:'Ativo', cashback:22, totalSpent:740, preferredService:'Hidratação Premium', preferredProfessional:'Camila Beauty', notes:['Contato por WhatsApp à tarde.'] },
      { id:'cli-003', name:'André Martins', phone:'(11) 96666-3333', email:'andre@demo.com', segment:'Inativo', status:'Reativar', cashback:12, totalSpent:510, preferredService:'Barba Tradicional', preferredProfessional:'Rafael Barber', notes:['Prefere PIX.'] }
    ],
    professionals: [
      { id:'pro-001', name:'Rafael Barber', specialty:'Corte degradê e barba', status:'Disponível', rating:4.9 },
      { id:'pro-002', name:'Camila Beauty', specialty:'Estética masculina', status:'Disponível', rating:4.8 },
      { id:'pro-003', name:'Diego Fade', specialty:'Cortes modernos', status:'Disponível', rating:4.7 }
    ],
    services: [
      { id:'srv-001', name:'Corte + Barba', price:95, durationMinutes:70, channel:'Admin/PublicWeb/Totem', productsConsumed:[{ productId:'prd-001', quantity:1 }, { productId:'prd-002', quantity:1 }] },
      { id:'srv-002', name:'Corte Masculino', price:65, durationMinutes:45, channel:'Admin/PublicWeb/Totem', productsConsumed:[{ productId:'prd-001', quantity:1 }] },
      { id:'srv-003', name:'Barba Tradicional', price:45, durationMinutes:30, channel:'Admin/PublicWeb/Totem', productsConsumed:[{ productId:'prd-002', quantity:1 }] },
      { id:'srv-004', name:'Hidratação Premium', price:120, durationMinutes:60, channel:'Admin/PublicWeb/Mobile', productsConsumed:[{ productId:'prd-003', quantity:1 }] }
    ],
    appointments: [
      { id:'apt-001', clientId:'cli-001', serviceId:'srv-001', professionalId:'pro-001', client:'Lucas Almeida', service:'Corte + Barba', professional:'Rafael Barber', date:today, time:'10:30', status:'Confirmed', channel:'Admin', value:95, timeline:['Agendamento confirmado'] },
      { id:'apt-002', clientId:'cli-002', serviceId:'srv-004', professionalId:'pro-002', client:'Marina Costa', service:'Hidratação Premium', professional:'Camila Beauty', date:today, time:'14:00', status:'CheckedIn', channel:'Totem', value:120, timeline:['Check-in feito no Totem'] },
      { id:'apt-003', clientId:'cli-003', serviceId:'srv-003', professionalId:'pro-001', client:'André Martins', service:'Barba Tradicional', professional:'Rafael Barber', date:today, time:'18:30', status:'Scheduled', channel:'PublicWeb', value:45, protocol:'PUB-2026-0001', timeline:['Solicitado pelo PublicWeb'] }
    ],
    serviceOrders: [
      { id:'so-1001', number:'1023', appointmentId:'apt-001', clientId:'cli-001', professionalId:'pro-001', client:'Lucas Almeida', professional:'Rafael Barber', status:'Paid', channel:'Admin', items:[{ type:'service', name:'Corte + Barba', amount:95 }, { type:'product', productId:'prd-001', name:'Pomada Modeladora', quantity:1, amount:39 }], total:134, paid:134, receipt:'REC-1023' }
    ],
    products: [
      { id:'prd-001', name:'Pomada Modeladora', sku:'POM-MATTE', stock:6, minStock:8, price:39, status:'Crítico' },
      { id:'prd-002', name:'Balm Pós-Barba', sku:'BALM-01', stock:14, minStock:6, price:32, status:'OK' },
      { id:'prd-003', name:'Máscara Hidratação', sku:'HID-01', stock:9, minStock:4, price:59, status:'OK' }
    ],
    stockMovements: [{ id:'mov-001', productId:'prd-001', product:'Pomada Modeladora', quantity:-1, type:'Saída por comanda', reason:'Comanda #1023', at:new Date().toISOString() }],
    payments: [{ id:'pay-001', orderId:'so-1001', amount:134, method:'PIX', status:'Paid', channel:'Admin', at:new Date().toISOString() }],
    campaigns: [{ id:'cmp-001', name:'Retorno em 21 dias', audience:'Clientes inativos', status:'Ativa', couponId:'cup-001', sent:42, conversions:8 }],
    coupons: [{ id:'cup-001', code:'VOLTE20', discount:20, status:'Ativo', campaignId:'cmp-001' }],
    reviews: [{ id:'rev-001', clientId:'cli-001', client:'Lucas Almeida', rating:5, nps:10, comment:'Atendimento excelente.', at:new Date().toISOString() }],
    loyalty: [{ id:'loy-001', clientId:'cli-001', balance:48, history:['Cashback inicial R$ 48,00'] }],
    copilotSuggestions: [
      { id:'sug-001', type:'campaign', title:'Gerar campanha de retorno', description:'Clientes inativos há mais de 45 dias.', action:'createCampaign' },
      { id:'sug-002', type:'stock', title:'Gerar reposição de estoque', description:'Pomada Modeladora abaixo do mínimo.', action:'stockReplenishment' },
      { id:'sug-003', type:'agenda', title:'Preencher horários vazios', description:'Há janelas entre 15h e 17h.', action:'fillSchedule' }
    ],
    dashboardEvents: [{ id:'evt-seed', eventName:'demo:scenarioLoaded', label:'Cenário default carregado', at:new Date().toISOString() }],
    publicLeads: [{ id:'lead-001', protocol:'PUB-2026-0001', client:'André Martins', service:'Barba Tradicional', time:'18:30', status:'Importado' }],
    kioskAttendances: []
  });

  let state = load();
  function migrate(raw) { const next = { ...seed(), ...(raw || {}) }; next.schemaVersion = SCHEMA_VERSION; modules.forEach(m => { if (!Array.isArray(next[m])) next[m] = []; }); return next; }
  function load() { try { return migrate(JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null')); } catch { return seed(); } }
  function save() { state.updatedAt = new Date().toISOString(); localStorage.setItem(STORAGE_KEY, JSON.stringify(state)); window.dispatchEvent(new CustomEvent('barbersync:store-changed', { detail: clone(state) })); window.dispatchEvent(new CustomEvent('barbersync:demo-store-changed', { detail: clone(state) })); return state; }
  function event(name, payload) { window.BarberSyncEventBus?.emit?.(name, payload); }
  function resolveNames(data) {
    const client = state.clients.find(c => c.id === data.clientId) || state.clients.find(c => c.name === data.client);
    const service = state.services.find(s => s.id === data.serviceId) || state.services.find(s => s.name === data.service);
    const pro = state.professionals.find(p => p.id === data.professionalId) || state.professionals.find(p => p.name === data.professional);
    return { client, service, pro };
  }
  function applyScenario(name) {
    state = seed(); state.scenario = name;
    if (name === 'emptyDay') state.appointments = [];
    if (name === 'busyDay') ['15:00','15:45','16:30','17:15'].forEach((time, i) => api.createAppointment({ clientId: state.clients[i % 3].id, serviceId: state.services[i % 4].id, professionalId: state.professionals[i % 3].id, date: today, time, channel: i % 2 ? 'Mobile' : 'PublicWeb' }, false));
    if (name === 'criticalStock') state.products.forEach((p, i) => { p.stock = i + 1; p.status = 'Crítico'; });
    if (name === 'highRevenue') { for (let i = 0; i < 5; i++) { const order = api.openServiceOrder({ clientId:'cli-001', professionalId:'pro-001', channel:'Admin' }, false); api.addServiceOrderItem(order.id, { type:'service', name:'Corte + Barba', amount:95 }, false); api.payServiceOrder(order.id, { method:'PIX', amount:95 }, false); } }
    if (name === 'campaignMode') api.createCampaign({ name:'Semana VIP BarberSync', audience:'VIP', createCoupon:true }, false);
    if (name === 'kioskDemo') api.importKioskAttendance(false);
    if (name === 'newSalon') { state.clients = []; state.appointments = []; state.serviceOrders = []; state.payments = []; }
    if (name === 'franchiseDemo') state.dashboardEvents.unshift({ id:uid('evt'), label:'Comparativo de 3 unidades carregado', eventName:'demo:scenarioLoaded', at:new Date().toISOString() });
    save();
  }

  const api = {
    get: moduleName => clone(state[moduleName] || []),
    set(moduleName, data) { state[moduleName] = Array.isArray(data) ? data : []; return save(); },
    add(moduleName, item) { const saved = { id: item.id || uid(moduleName.slice(0,3)), ...item }; state[moduleName] = [...(state[moduleName] || []), saved]; save(); return clone(saved); },
    update(moduleName, id, item) { state[moduleName] = (state[moduleName] || []).map(x => x.id === id ? { ...item, id } : x); save(); return api.find(moduleName, id); },
    patch(moduleName, id, patch) { state[moduleName] = (state[moduleName] || []).map(x => x.id === id ? { ...x, ...patch } : x); save(); return api.find(moduleName, id); },
    remove(moduleName, id) { state[moduleName] = (state[moduleName] || []).filter(x => x.id !== id); return save(); },
    find: (moduleName, id) => clone((state[moduleName] || []).find(x => x.id === id)),
    filter(moduleName, mode) { const data = state[moduleName] || []; if (mode === 'critical') return clone(data.filter(x => Number(x.stock ?? x.quantity ?? 0) <= Number(x.minStock ?? x.minimumStock ?? 0))); return clone(data.filter(x => String(x.status || '').toLowerCase().includes(String(mode || '').toLowerCase()))); },
    reset(moduleName) { state[moduleName] = seed()[moduleName] || []; return save(); },
    resetAll() { state = seed(); save(); event('demo:scenarioLoaded', { message:'Dados demo resetados', scenario:'default' }); return clone(state); },
    loadScenario(scenarioName) { applyScenario(scenarioName || 'default'); event('demo:scenarioLoaded', { scenario: scenarioName, message:`Cenário ${scenarioName} carregado` }); return clone(state); },
    exportState: () => JSON.stringify(state, null, 2),
    importState(json) { state = migrate(typeof json === 'string' ? JSON.parse(json) : json); save(); event('demo:scenarioLoaded', { message:'Estado demo importado' }); return clone(state); },
    recordEvent(entry) { if (!state.dashboardEvents.some(e => e.id === entry.id)) state.dashboardEvents.unshift(entry); state.dashboardEvents = state.dashboardEvents.slice(0, 80); save(); },
    createClient(data = {}, emit = true) { const client = api.add('clients', { name:data.name || 'Cliente Demo 4.0', phone:data.phone || '(11) 99999-2026', email:data.email || 'cliente@barbersync.demo', segment:data.segment || 'Novo', status:'Ativo', cashback:0, totalSpent:0, notes:['Cliente cadastrado na demonstração integrada.'] }); if (emit) event('client:created', { client, message:`Cliente ${client.name} criado` }); return client; },
    createAppointment(data = {}, emit = true) { const names = resolveNames(data); const service = names.service || state.services[0]; const client = names.client || state.clients[0] || api.createClient({}, false); const pro = names.pro || state.professionals[0]; const appt = api.add('appointments', { clientId:client.id, serviceId:service.id, professionalId:pro.id, client:client.name, service:service.name, professional:pro.name, date:data.date || today, time:data.time || '16:00', status:statusMap[data.status] || 'Scheduled', channel:data.channel || 'Admin', value:money(data.value || service.price), protocol:data.protocol, timeline:['Agendamento criado'] }); if (emit) event('appointment:created', { appointment:appt, message:`Agendamento de ${appt.client} criado` }); return appt; },
    confirmAppointment(id) { const appt = api.patch('appointments', id, { status:'Confirmed', timeline:[...(api.find('appointments', id)?.timeline || []), 'Agendamento confirmado'] }); event('appointment:confirmed', { appointment:appt }); return appt; },
    checkInAppointment(id) { const appt = api.patch('appointments', id, { status:'CheckedIn', checkInAt:new Date().toISOString(), timeline:[...(api.find('appointments', id)?.timeline || []), 'Check-in realizado'] }); event('appointment:checkedIn', { appointment:appt }); return appt; },
    startAppointment(id) { const appt = api.patch('appointments', id, { status:'InService', startedAt:new Date().toISOString(), timeline:[...(api.find('appointments', id)?.timeline || []), 'Atendimento iniciado'] }); event('appointment:started', { appointment:appt }); return appt; },
    finishAppointment(id) { const appt = api.patch('appointments', id, { status:'Finished', finishedAt:new Date().toISOString(), timeline:[...(api.find('appointments', id)?.timeline || []), 'Atendimento finalizado'] }); event('appointment:finished', { appointment:appt }); return appt; },
    openServiceOrder(data = {}, emit = true) { const appt = data.appointmentId ? state.appointments.find(a => a.id === data.appointmentId) : null; const names = resolveNames(data); const client = names.client || state.clients.find(c => c.id === appt?.clientId) || state.clients[0]; const pro = names.pro || state.professionals.find(p => p.id === appt?.professionalId) || state.professionals[0]; const order = api.add('serviceOrders', { number:String(1024 + state.serviceOrders.length), appointmentId:appt?.id || data.appointmentId, clientId:client?.id, professionalId:pro?.id, client:client?.name || data.client || 'Cliente balcão', professional:pro?.name || data.professional || 'Equipe', status:'Open', channel:data.channel || appt?.channel || 'Admin', items:[], total:0, paid:0 }); if (emit) event('serviceOrder:opened', { order, message:`Comanda #${order.number} aberta` }); return order; },
    addServiceOrderItem(orderId, item = {}, emit = true) { const order = state.serviceOrders.find(o => o.id === orderId); if (!order) return null; const newItem = { id:uid('item'), quantity:1, ...item, amount:money(item.amount || item.price || 0) }; order.items.push(newItem); order.total = order.items.reduce((sum, x) => sum + money(x.amount) * money(x.quantity || 1), 0); save(); if (emit) event('serviceOrder:itemAdded', { order:clone(order), item:newItem }); return clone(order); },
    payServiceOrder(orderId, payment = {}, emit = true) { const order = state.serviceOrders.find(o => o.id === orderId); if (!order) return null; order.status = 'Paid'; order.paid = money(payment.amount || order.total); order.receipt = order.receipt || `REC-${order.number}`; const pay = api.add('payments', { orderId, amount:order.paid, method:payment.method || 'PIX', status:'Paid', channel:order.channel, at:new Date().toISOString() }); order.items.filter(i => i.type === 'product' && i.productId).forEach(i => api.moveStock(i.productId, -Math.abs(i.quantity || 1), 'Saída por comanda', `Comanda #${order.number}`, false)); const cashback = api.generateCashback(order.clientId, Math.round(order.paid * 0.05), false); save(); if (emit) event('serviceOrder:paid', { order:clone(order), payment:pay, cashback, message:`Comanda #${order.number} paga via ${pay.method}` }); return clone(order); },
    closeServiceOrder(orderId) { const order = api.patch('serviceOrders', orderId, { status:'Closed', closedAt:new Date().toISOString() }); event('serviceOrder:closed', { order }); return order; },
    moveStock(productId, quantity, type = 'Ajuste', reason = 'Movimentação manual', emit = true) { const product = state.products.find(p => p.id === productId); if (!product) return null; product.stock = money(product.stock) + money(quantity); product.status = product.stock <= product.minStock ? 'Crítico' : 'OK'; const movement = api.add('stockMovements', { productId, product:product.name, quantity, type, reason, at:new Date().toISOString() }); if (emit) event('stock:changed', { product:clone(product), movement, message:`Estoque de ${product.name} atualizado` }); return movement; },
    generateCashback(clientId, amount, emit = true) { if (!clientId) return null; const client = state.clients.find(c => c.id === clientId); if (client) client.cashback = money(client.cashback) + money(amount); let loyalty = state.loyalty.find(l => l.clientId === clientId); if (!loyalty) loyalty = api.add('loyalty', { clientId, balance:0, history:[] }); loyalty.balance = money(loyalty.balance) + money(amount); loyalty.history.push(`Cashback gerado R$ ${money(amount).toFixed(2)}`); save(); if (emit) event('cashback:generated', { client:clone(client), amount }); return clone(loyalty); },
    createReview(data = {}) { const client = state.clients.find(c => c.id === data.clientId) || state.clients[0]; const review = api.add('reviews', { clientId:client?.id, client:client?.name || 'Cliente Demo', rating:data.rating || 5, nps:data.nps || 10, comment:data.comment || 'Atendimento excelente em modo demo.', at:new Date().toISOString() }); event('review:created', { review }); return review; },
    createCampaign(data = {}, emit = true) { const campaign = api.add('campaigns', { name:data.name || 'Campanha de retorno', audience:data.audience || 'Clientes inativos', status:'Ativa', sent:data.sent || state.clients.length, conversions:0 }); if (data.createCoupon !== false) api.createCoupon({ campaignId:campaign.id, code:data.code || `DEMO${String(state.coupons.length + 1).padStart(2, '0')}`, discount:data.discount || 15 }, emit); if (emit) event('campaign:created', { campaign, message:`Campanha ${campaign.name} criada` }); return campaign; },
    createCoupon(data = {}, emit = true) { const coupon = api.add('coupons', { campaignId:data.campaignId, code:data.code || `CUPOM${Date.now().toString().slice(-4)}`, discount:data.discount || 10, status:'Ativo' }); if (emit) event('coupon:created', { coupon }); return coupon; },
    importPublicLeads() { const lead = { id:uid('lead'), protocol:`PUB-2026-${String(state.publicLeads.length + 1).padStart(4,'0')}`, client:'Bianca PublicWeb', service:'Corte Masculino', time:'17:30', status:'Importado' }; state.publicLeads.push(lead); api.createAppointment({ client:lead.client, service:lead.service, time:lead.time, channel:'PublicWeb', protocol:lead.protocol }, false); save(); event('appointment:created', { lead, message:'Leads públicos demo importados' }); return lead; },
    importKioskAttendance(emit = true) { const att = { id:uid('kiosk'), attendanceId:`KIOSK-${Date.now().toString().slice(-5)}`, serviceOrderNumber:String(2000 + state.kioskAttendances.length), client:'Cliente Totem', service:'Corte Masculino', amount:65, payment:{ method:'PIX', status:'Paid' }, at:new Date().toISOString() }; state.kioskAttendances.push(att); const client = api.createClient({ name:att.client, segment:'Totem' }, false); const order = api.openServiceOrder({ clientId:client.id, channel:'Totem' }, false); api.addServiceOrderItem(order.id, { type:'service', name:att.service, amount:att.amount }, false); api.payServiceOrder(order.id, { method:'PIX', amount:att.amount }, false); save(); if (emit) event('serviceOrder:paid', { attendance:att, message:'Atendimento importado do Totem' }); return att; },
    createServiceOrderFromAppointment(appointmentId) { const appt = state.appointments.find(a => a.id === appointmentId); if (!appt) return null; const order = api.openServiceOrder({ appointmentId, clientId:appt.clientId, professionalId:appt.professionalId, channel:appt.channel }, false); api.addServiceOrderItem(order.id, { type:'service', name:appt.service, amount:appt.value || 65 }, false); event('serviceOrder:opened', { order, message:`Comanda #${order.number} aberta` }); return order; },
    generateReview(clientId, orderId) { return api.createReview({ clientId, orderId, rating:5, nps:10 }); },
    refreshDashboard() { event('dashboard:refresh', { summary: api.dashboardSummary(), message:'Dashboard atualizado' }); return api.dashboardSummary(); },
    dashboardSummary() { const paid = state.serviceOrders.filter(o => ['Paid','Closed'].includes(o.status)); const revenue = paid.reduce((s,o) => s + money(o.paid || o.total), 0); const cashback = state.loyalty.reduce((s,l) => s + money(l.balance), 0); const finished = state.appointments.filter(a => ['Finished','InService','CheckedIn'].includes(a.status)).length; return { revenue, appointments:state.appointments.length, clients:state.clients.length, paidOrders:paid.length, cashback, reviews:state.reviews.length, publicLeads:state.publicLeads.length, kioskAttendances:state.kioskAttendances.length, criticalStock:state.products.filter(p => p.stock <= p.minStock).length, conversion: state.appointments.length ? Math.round((finished / state.appointments.length) * 100) : 0, channels: ['Admin','PublicWeb','Totem','Mobile'].map(ch => ({ channel:ch, revenue: paid.filter(o => o.channel === ch).reduce((s,o) => s + money(o.paid || o.total), 0) })) }; }
  };
  window.BarberSyncDemoStore = api;
  save();
})();
