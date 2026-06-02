(() => {
  const STORAGE_KEY = 'BarberSync:AdminDemoStore:v2';
  const clone = value => JSON.parse(JSON.stringify(value));

  const seed = {
    clients: [
      { id: 'cli-001', name: 'Lucas Almeida', personType: 'PF', document: '123.456.789-00', phone: '(11) 98888-1111', whatsapp: '(11) 98888-1111', email: 'lucas@demo.com', preferredService: 'Corte + Barba', preferredProfessional: 'Rafael Barber', status: 'VIP', vip: 'Sim', cashback: 48, totalSpent: 1860 },
      { id: 'cli-002', name: 'Marina Costa', personType: 'PF', document: '987.654.321-00', phone: '(11) 97777-2222', whatsapp: '(11) 97777-2222', email: 'marina@demo.com', preferredService: 'Design de sobrancelha', preferredProfessional: 'Camila Beauty', status: 'Ativo', cashback: 22, totalSpent: 740 },
      { id: 'cli-003', name: 'André Martins', personType: 'PF', document: '456.789.123-00', phone: '(11) 96666-3333', email: 'andre@demo.com', preferredService: 'Barba Tradicional', preferredProfessional: 'Rafael Barber', status: 'Reativar', cashback: 12, totalSpent: 510 }
    ],
    professionals: [
      { id: 'pro-001', name: 'Rafael Barber', specialty: 'Corte degradê e barba', phone: '(11) 95555-1000', email: 'rafael@barbersync.demo', commission: 40, status: 'Disponível', monthlyRevenue: 18450, rating: 4.9, monthlyGoal: 22000 },
      { id: 'pro-002', name: 'Camila Beauty', specialty: 'Estética masculina', phone: '(11) 95555-2000', email: 'camila@barbersync.demo', commission: 35, status: 'Em atendimento', monthlyRevenue: 12680, rating: 4.8, monthlyGoal: 18000 }
    ],
    services: [
      { id: 'srv-001', name: 'Corte + Barba', category: 'Combo premium', description: 'Corte, barba quente e finalização.', price: 95, durationMinutes: 70, commission: 40, site: 'Sim', kiosk: 'Sim', mobile: 'Sim', status: 'Ativo' },
      { id: 'srv-002', name: 'Corte Masculino', category: 'Cabelo', description: 'Corte clássico ou degradê.', price: 65, durationMinutes: 45, commission: 35, site: 'Sim', kiosk: 'Sim', mobile: 'Sim', status: 'Ativo' },
      { id: 'srv-003', name: 'Barba Tradicional', category: 'Barba', description: 'Toalha quente, navalha e balm.', price: 45, durationMinutes: 30, commission: 35, site: 'Sim', kiosk: 'Sim', mobile: 'Sim', status: 'Ativo' }
    ],
    appointments: [
      { id: 'apt-001', client: 'Lucas Almeida', service: 'Corte + Barba', professional: 'Rafael Barber', date: '2026-06-02', time: '10:30', status: 'Confirmado' },
      { id: 'apt-002', client: 'Marina Costa', service: 'Design de sobrancelha', professional: 'Camila Beauty', date: '2026-06-02', time: '14:00', status: 'Check-in' },
      { id: 'apt-003', client: 'André Martins', service: 'Barba Tradicional', professional: 'Rafael Barber', date: '2026-06-02', time: '18:30', status: 'Agendado' }
    ],
    serviceOrders: [
      { id: 'so-1001', name: 'Comanda #1001', client: 'Lucas Almeida', service: 'Corte + Barba', professional: 'Rafael Barber', items: 'Corte + Barba; Pomada Modeladora', subtotal: 130, discount: 10, cashback: 5, total: 115, paymentMethod: 'PIX', status: 'Aberta' },
      { id: 'so-1002', name: 'Comanda #1002', client: 'Marina Costa', service: 'Design de sobrancelha', professional: 'Camila Beauty', items: 'Design de sobrancelha', subtotal: 55, discount: 0, cashback: 0, total: 55, paymentMethod: 'Cartão', status: 'Em pagamento' }
    ],
    products: [
      { id: 'prd-001', name: 'Pomada Modeladora', product: 'Pomada Modeladora', quantity: 4, stock: 18, minimumStock: 12, status: 'Crítico', suggestion: 'Comprar 24 unidades' },
      { id: 'prd-002', name: 'Balm para barba', product: 'Balm para barba', quantity: 18, stock: 72, minimumStock: 10, status: 'Normal', suggestion: 'Estoque saudável' },
      { id: 'prd-003', name: 'Shampoo Antiqueda', product: 'Shampoo Antiqueda', quantity: 8, stock: 35, minimumStock: 10, status: 'Atenção', suggestion: 'Comprar 12 unidades' }
    ],
    campaigns: [
      { id: 'cmp-001', name: 'Retorno em 21 dias', audience: 'Clientes sem visita há 30 dias', period: '02/06 a 30/06', channel: 'WhatsApp', status: 'Ativa', result: '18 reativações' }
    ],
    coupons: [
      { id: 'cup-001', code: 'RETORNO20', discount: '20% no combo', validUntil: '2026-06-30', usage: '32/100', status: 'Ativo' }
    ],
    reviews: [
      { id: 'rev-001', name: 'Lucas Almeida', rating: 5, nps: 10, comment: 'Atendimento excelente e pontual.', status: 'Promotor' },
      { id: 'rev-002', name: 'Cliente detrator demo', rating: 3, nps: 6, comment: 'Fila demorou no sábado.', status: 'Detrator' }
    ],
    loyalty: [
      { id: 'loy-001', name: 'Lucas Almeida', cashback: 48, expiring: 12, status: 'Saldo ativo' },
      { id: 'loy-002', name: 'Marina Costa', cashback: 22, expiring: 0, status: 'Saldo ativo' }
    ]
  };

  const aliases = {
    Clients: 'clients', Professionals: 'professionals', Services: 'services', Appointments: 'appointments',
    ServiceOrders: 'serviceOrders', Stock: 'products', Products: 'products', Campaigns: 'campaigns', Coupons: 'coupons',
    Reviews: 'reviews', Loyalty: 'loyalty', serviceorders: 'serviceOrders', serviceOrders: 'serviceOrders'
  };

  function moduleKey(moduleName) {
    return aliases[moduleName] || aliases[String(moduleName || '').toLowerCase()] || moduleName;
  }

  function readState() {
    try {
      const state = JSON.parse(localStorage.getItem(STORAGE_KEY) || 'null');
      return state && typeof state === 'object' ? { ...clone(seed), ...state } : clone(seed);
    } catch (_) {
      return clone(seed);
    }
  }

  function writeState(state) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(state));
    window.dispatchEvent(new CustomEvent('barbersync:demo-store-changed', { detail: state }));
  }

  function get(moduleName) {
    const key = moduleKey(moduleName);
    return clone(readState()[key] || []);
  }

  function set(moduleName, data) {
    const key = moduleKey(moduleName);
    const state = readState();
    state[key] = Array.isArray(data) ? clone(data) : [];
    writeState(state);
    return get(key);
  }

  function add(moduleName, item) {
    const key = moduleKey(moduleName);
    const state = readState();
    const next = { id: item?.id || `${key}-${Date.now()}`, status: item?.status || 'Ativo', ...(item || {}) };
    state[key] = [next, ...(state[key] || [])];
    writeState(state);
    return clone(next);
  }

  function update(moduleName, id, item) {
    const key = moduleKey(moduleName);
    const state = readState();
    const normalizedId = String(id || item?.id || '');
    state[key] = (state[key] || []).map(current => String(current.id) === normalizedId ? { ...current, ...(item || {}), id: current.id } : current);
    writeState(state);
    return get(key).find(current => String(current.id) === normalizedId) || null;
  }

  function remove(moduleName, id) {
    const key = moduleKey(moduleName);
    const state = readState();
    state[key] = (state[key] || []).filter(item => String(item.id) !== String(id));
    writeState(state);
    return get(key);
  }

  function patch(moduleName, id, patchValue) {
    return update(moduleName, id, patchValue || {});
  }

  function updateStatus(moduleName, id, status) {
    return patch(moduleName, id, { status });
  }

  function reset(moduleName) {
    const key = moduleKey(moduleName);
    const state = readState();
    state[key] = clone(seed[key] || []);
    writeState(state);
    return get(key);
  }

  function resetAll() {
    writeState(clone(seed));
    return readState();
  }

  window.BarberSyncDemoStore = { get, set, add, update, remove, patch, updateStatus, reset, resetAll, seed: clone(seed), moduleKey };
})();
