(() => {
  const STORAGE_KEY = 'BarberSync:AdminDemoStore:v2';
  const SCENARIO_KEY = 'BarberSync:AdminDemoStore:scenario';
  const clone = value => JSON.parse(JSON.stringify(value));
  const today = '2026-06-02';

  const baseSeed = {
    dashboard: {
      kpis: { revenue: 18420, occupancy: 86, recurrence: 72, satisfaction: 94, stockHealth: 68, siteConversion: 18 },
      flow: { scheduled: 32, checkin: 9, service: 6, orders: 12, paid: 18, reviews: 11 },
      alerts: ['Pomada Modeladora abaixo do mínimo', '3 horários vazios entre 15h e 17h', '18 clientes sem retorno há 30 dias', '4 comandas abertas', 'Campanha RETORNO20 expira em 7 dias'],
      opportunities: ['Reativar clientes VIP inativos', 'Oferecer hidratação no combo premium', 'Aumentar exposição de produtos com margem alta', 'Usar totem para pré-check-in no pico']
    },
    clients: [
      { id: 'cli-001', name: 'Lucas Almeida', segment: 'VIP', personType: 'PF', document: '123.456.789-00', phone: '(11) 98888-1111', whatsapp: '(11) 98888-1111', email: 'lucas@demo.com', preferredService: 'Corte + Barba', preferredProfessional: 'Rafael Barber', status: 'VIP', vip: 'Sim', cashback: 48, totalSpent: 1860, score: 96, nextBestAction: 'Agendar retorno em 21 dias' },
      { id: 'cli-002', name: 'Marina Costa', segment: 'Aniversariante', personType: 'PF', document: '987.654.321-00', phone: '(11) 97777-2222', whatsapp: '(11) 97777-2222', email: 'marina@demo.com', preferredService: 'Design de sobrancelha', preferredProfessional: 'Camila Beauty', status: 'Ativo', cashback: 22, totalSpent: 740, score: 84, nextBestAction: 'Enviar cupom de aniversário' },
      { id: 'cli-003', name: 'André Martins', segment: 'Inativo', personType: 'PF', document: '456.789.123-00', phone: '(11) 96666-3333', email: 'andre@demo.com', preferredService: 'Barba Tradicional', preferredProfessional: 'Rafael Barber', status: 'Reativar', cashback: 12, totalSpent: 510, score: 58, nextBestAction: 'Campanha de reativação' },
      { id: 'cli-004', name: 'Paula Nunes', segment: 'Novo', phone: '(11) 95555-4444', email: 'paula@demo.com', preferredService: 'Hidratação Premium', status: 'Novo', cashback: 8, totalSpent: 160, score: 72, nextBestAction: 'Oferecer pacote recorrente' }
    ],
    professionals: [
      { id: 'pro-001', name: 'Rafael Barber', specialty: 'Corte degradê e barba', phone: '(11) 95555-1000', email: 'rafael@barbersync.demo', commission: 40, status: 'Disponível', monthlyRevenue: 18450, rating: 4.9, monthlyGoal: 22000, occupancy: 91, appointmentsToday: 8, topServices: 'Corte + Barba, Barba Tradicional' },
      { id: 'pro-002', name: 'Camila Beauty', specialty: 'Estética masculina', phone: '(11) 95555-2000', email: 'camila@barbersync.demo', commission: 35, status: 'Em atendimento', monthlyRevenue: 12680, rating: 4.8, monthlyGoal: 18000, occupancy: 78, appointmentsToday: 6, topServices: 'Sobrancelha, Limpeza de pele' },
      { id: 'pro-003', name: 'Diego Fade', specialty: 'Corte moderno', phone: '(11) 95555-3000', commission: 38, status: 'Disponível', monthlyRevenue: 9800, rating: 4.7, monthlyGoal: 16000, occupancy: 62, appointmentsToday: 4, topServices: 'Corte Masculino' }
    ],
    services: [
      { id: 'srv-001', name: 'Corte + Barba', category: 'Combo premium', description: 'Corte, barba quente e finalização.', price: 95, durationMinutes: 70, commission: 40, site: 'Sim', kiosk: 'Sim', mobile: 'Sim', status: 'Ativo', margin: 72, sold: 148, productsConsumed: 'Pomada, Balm' },
      { id: 'srv-002', name: 'Corte Masculino', category: 'Cabelo', description: 'Corte clássico ou degradê.', price: 65, durationMinutes: 45, commission: 35, site: 'Sim', kiosk: 'Sim', mobile: 'Sim', status: 'Ativo', margin: 76, sold: 210, productsConsumed: 'Pomada' },
      { id: 'srv-003', name: 'Barba Tradicional', category: 'Barba', description: 'Toalha quente, navalha e balm.', price: 45, durationMinutes: 30, commission: 35, site: 'Sim', kiosk: 'Sim', mobile: 'Sim', status: 'Ativo', margin: 68, sold: 132, productsConsumed: 'Balm, Lâmina' },
      { id: 'srv-004', name: 'Hidratação Premium', category: 'Tratamento', description: 'Tratamento capilar profissional.', price: 120, durationMinutes: 60, commission: 36, site: 'Sim', kiosk: 'Não', mobile: 'Sim', status: 'Ativo', margin: 61, sold: 44, productsConsumed: 'Máscara premium' }
    ],
    appointments: [
      { id: 'apt-001', client: 'Lucas Almeida', service: 'Corte + Barba', professional: 'Rafael Barber', date: today, time: '10:30', status: 'Confirmado' },
      { id: 'apt-002', client: 'Marina Costa', service: 'Design de sobrancelha', professional: 'Camila Beauty', date: today, time: '14:00', status: 'Check-in' },
      { id: 'apt-003', client: 'André Martins', service: 'Barba Tradicional', professional: 'Rafael Barber', date: today, time: '18:30', status: 'Agendado' },
      { id: 'apt-004', client: 'Paula Nunes', service: 'Hidratação Premium', professional: 'Camila Beauty', date: today, time: '16:00', status: 'Lista de espera' }
    ],
    serviceOrders: [
      { id: 'so-1001', name: 'Comanda #1001', client: 'Lucas Almeida', service: 'Corte + Barba', professional: 'Rafael Barber', items: 'Corte + Barba; Pomada Modeladora', subtotal: 130, discount: 10, cashback: 5, total: 115, paymentMethod: 'PIX', status: 'Aberta' },
      { id: 'so-1002', name: 'Comanda #1002', client: 'Marina Costa', service: 'Design de sobrancelha', professional: 'Camila Beauty', items: 'Design de sobrancelha', subtotal: 55, discount: 0, cashback: 0, total: 55, paymentMethod: 'Cartão', status: 'Em pagamento' },
      { id: 'so-1003', name: 'Comanda #1003', client: 'Paula Nunes', service: 'Hidratação Premium', professional: 'Camila Beauty', items: 'Hidratação Premium; Shampoo', subtotal: 168, discount: 15, cashback: 8, total: 145, paymentMethod: 'Misto', status: 'Paga' }
    ],
    products: [
      { id: 'prd-001', name: 'Pomada Modeladora', product: 'Pomada Modeladora', quantity: 4, stock: 4, minimumStock: 12, cost: 22, price: 39, status: 'Crítico', suggestion: 'Comprar 24 unidades', turnover: 'Alto', movement: 'Saída' },
      { id: 'prd-002', name: 'Balm para barba', product: 'Balm para barba', quantity: 18, stock: 18, minimumStock: 10, cost: 18, price: 34, status: 'Normal', suggestion: 'Estoque saudável', turnover: 'Médio', movement: 'Entrada' },
      { id: 'prd-003', name: 'Shampoo Antiqueda', product: 'Shampoo Antiqueda', quantity: 8, stock: 8, minimumStock: 10, cost: 30, price: 59, status: 'Atenção', suggestion: 'Comprar 12 unidades', turnover: 'Médio', movement: 'Ajuste' }
    ],
    campaigns: [{ id: 'cmp-001', name: 'Retorno em 21 dias', audience: 'Clientes sem visita há 30 dias', period: '02/06 a 30/06', channel: 'WhatsApp', status: 'Ativa', result: '18 reativações', impacted: 240, estimatedRevenue: 7200, funnel: '240 > 98 > 31 > 18' }],
    coupons: [{ id: 'cup-001', code: 'RETORNO20', discount: '20% no combo', validUntil: '2026-06-30', usage: '32/100', status: 'Ativo' }],
    reviews: [{ id: 'rev-001', name: 'Lucas Almeida', rating: 5, nps: 10, comment: 'Atendimento excelente e pontual.', status: 'Promotor' }, { id: 'rev-002', name: 'Cliente detrator demo', rating: 3, nps: 6, comment: 'Fila demorou no sábado.', status: 'Detrator' }],
    loyalty: [{ id: 'loy-001', name: 'Lucas Almeida', cashback: 48, expiring: 12, status: 'Saldo ativo' }, { id: 'loy-002', name: 'Marina Costa', cashback: 22, expiring: 0, status: 'Saldo ativo' }],
    copilot: [{ id: 'cop-001', category: 'Estoque', priority: 'Alta', summary: 'Pomada Modeladora está crítica.', metric: '4 unidades / mínimo 12', action: 'Abrir reposição', status: 'Pendente' }, { id: 'cop-002', category: 'Clientes', priority: 'Média', summary: '18 clientes VIP sem retorno.', metric: 'R$ 7,2 mil potencial', action: 'Criar campanha', status: 'Pendente' }]
  };

  const scenarioMutators = {
    default: s => s,
    busyDay: s => { s.dashboard.flow = { scheduled: 54, checkin: 18, service: 11, orders: 22, paid: 31, reviews: 19 }; s.appointments = [...s.appointments, ...Array.from({ length: 8 }, (_, i) => ({ id: `apt-busy-${i}`, client: `Cliente Movimento ${i + 1}`, service: i % 2 ? 'Corte Masculino' : 'Corte + Barba', professional: i % 2 ? 'Diego Fade' : 'Rafael Barber', date: today, time: `${12 + i}:00`, status: i % 3 ? 'Agendado' : 'Confirmado' }))]; return s; },
    criticalStock: s => { s.products = s.products.map(p => ({ ...p, stock: p.id === 'prd-002' ? 6 : p.stock, quantity: p.id === 'prd-002' ? 6 : p.quantity, status: p.id === 'prd-002' ? 'Crítico' : p.status })); s.dashboard.kpis.stockHealth = 42; return s; },
    highRevenue: s => { s.dashboard.kpis.revenue = 42680; s.dashboard.kpis.occupancy = 94; s.serviceOrders = s.serviceOrders.map(o => ({ ...o, status: 'Paga' })); return s; },
    campaignMode: s => { s.campaigns.push({ id: 'cmp-002', name: 'Cashback Relâmpago', audience: 'Clientes com saldo', period: '02/06 a 09/06', channel: 'WhatsApp', status: 'Ativa', result: 'R$ 4.800 previstos', impacted: 180, estimatedRevenue: 4800, funnel: '180 > 110 > 42 > 21' }); return s; },
    kioskDemo: s => { s.dashboard.flow.checkin += 12; s.dashboard.kpis.siteConversion = 24; s.appointments.unshift({ id: 'apt-kiosk-001', client: 'Walk-in Totem', service: 'Corte Masculino', professional: 'Diego Fade', date: today, time: 'Agora', status: 'Check-in' }); return s; }
  };

  const aliases = { Clients: 'clients', Professionals: 'professionals', Services: 'services', Appointments: 'appointments', ServiceOrders: 'serviceOrders', Stock: 'products', Products: 'products', Campaigns: 'campaigns', Coupons: 'coupons', Reviews: 'reviews', Loyalty: 'loyalty', Copilot: 'copilot', Dashboard: 'dashboard', Financial: 'dashboard', serviceorders: 'serviceOrders' };
  const moduleKey = moduleName => aliases[moduleName] || aliases[String(moduleName || '').toLowerCase()] || moduleName;
  function seedForScenario(name = localStorage.getItem(SCENARIO_KEY) || 'default') { const state = clone(baseSeed); return (scenarioMutators[name] || scenarioMutators.default)(state); }
  function readState() { try { const state = JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null'); return state && typeof state === 'object' ? { ...seedForScenario(), ...state } : seedForScenario(); } catch { return seedForScenario(); } }
  function writeState(state) { localStorage.setItem(STORAGE_KEY, JSON.stringify(state)); window.dispatchEvent(new CustomEvent('barbersync:demo-store-changed', { detail: state })); }
  function get(moduleName) { const value = readState()[moduleKey(moduleName)]; return clone(value ?? []); }
  function set(moduleName, data) { const key = moduleKey(moduleName); const state = readState(); state[key] = clone(data); writeState(state); return get(key); }
  function add(moduleName, item) { const key = moduleKey(moduleName); const state = readState(); const collection = Array.isArray(state[key]) ? state[key] : []; const next = { id: item?.id || `${key}-${Date.now()}`, status: item?.status || 'Ativo', ...(item || {}) }; state[key] = [next, ...collection]; writeState(state); return clone(next); }
  function update(moduleName, id, item) { const key = moduleKey(moduleName); const state = readState(); const normalizedId = String(id || item?.id || ''); state[key] = (Array.isArray(state[key]) ? state[key] : []).map(current => String(current.id) === normalizedId ? { ...current, ...(item || {}), id: current.id } : current); writeState(state); return get(key).find(current => String(current.id) === normalizedId) || null; }
  function patch(moduleName, id, patchValue) { return update(moduleName, id, patchValue || {}); }
  function remove(moduleName, id) { const key = moduleKey(moduleName); const state = readState(); state[key] = (Array.isArray(state[key]) ? state[key] : []).filter(item => String(item.id) !== String(id)); writeState(state); return get(key); }
  function reset(moduleName) { const key = moduleKey(moduleName); const state = readState(); state[key] = clone(seedForScenario()[key] ?? []); writeState(state); return get(key); }
  function resetAll() { const state = seedForScenario('default'); localStorage.setItem(SCENARIO_KEY, 'default'); writeState(state); return clone(state); }
  function loadScenario(scenarioName) { const name = scenarioMutators[scenarioName] ? scenarioName : 'default'; localStorage.setItem(SCENARIO_KEY, name); const state = seedForScenario(name); writeState(state); return clone(state); }
  function exportState() { return JSON.stringify(readState(), null, 2); }
  function importState(json) { const parsed = typeof json === 'string' ? JSON.parse(json) : json; if (!parsed || typeof parsed !== 'object') throw new Error('Estado demo inválido'); writeState({ ...seedForScenario(), ...parsed }); return readState(); }
  function updateStatus(moduleName, id, status) { return patch(moduleName, id, { status }); }

  window.BarberSyncDemoStore = { get, set, add, update, patch, remove, reset, resetAll, loadScenario, exportState, importState, updateStatus, seed: clone(baseSeed), moduleKey };
})();
