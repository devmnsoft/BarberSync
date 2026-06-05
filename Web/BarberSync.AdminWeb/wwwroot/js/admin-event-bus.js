(() => {
  const STORAGE_KEY = 'barbersync.demo.events.v6';
  const LEGACY_KEYS = ['barbersync.demo.events'];
  const listeners = new Map();
  const titles = {
    'demo:scenarioLoaded':['Cenário demo carregado','Demo','success'], 'demo:reset':['Demo resetada','Demo','warning'],
    'public:leadCreated':['Lead público criado','PublicWeb','success'], 'public:appointmentRequested':['Agendamento público solicitado','PublicWeb','success'],
    'kiosk:attendanceCreated':['Atendimento criado no Totem','Totem','success'], 'kiosk:paymentCompleted':['Pagamento concluído no Totem','Totem','success'],
    'client:created':['Cliente criado','Clientes 360','success'], 'client:updated':['Cliente atualizado','Clientes 360','info'],
    'appointment:created':['Agendamento criado','Agenda','success'], 'appointment:confirmed':['Agendamento confirmado','Agenda','success'],
    'appointment:checkedIn':['Check-in realizado','Operação','success'], 'appointment:started':['Atendimento iniciado','Operação','info'],
    'appointment:finished':['Atendimento finalizado','Operação','success'], 'appointment:cancelled':['Agendamento cancelado','Operação','warning'],
    'serviceOrder:opened':['Comanda aberta','PDV','success'], 'serviceOrder:itemAdded':['Item adicionado à comanda','PDV','info'],
    'serviceOrder:paid':['Comanda paga','Caixa','success'], 'serviceOrder:closed':['Comanda fechada','PDV','success'],
    'payment:created':['Pagamento criado','Pagamentos','success'], 'stock:changed':['Estoque movimentado','Estoque','info'], 'stock:critical':['Estoque crítico','Estoque','danger'],
    'cashback:generated':['Cashback gerado','Fidelidade','success'], 'review:created':['Avaliação criada','Avaliações','success'],
    'campaign:created':['Campanha criada','Campanhas','success'], 'coupon:created':['Cupom criado','Cupons','success'],
    'copilot:actionExecuted':['Ação Copilot executada','Copilot','success'], 'dashboard:refresh':['Dashboard atualizado','Dashboard','info'],
    'commercialFlow:started':['Fluxo comercial iniciado','Fluxo Comercial','info'],
    'commercialFlow:originSelected':['Origem selecionada','Fluxo Comercial','success'],
    'commercialFlow:clientSelected':['Cliente selecionado','Fluxo Comercial','success'],
    'commercialFlow:appointmentCreated':['Agendamento criado no fluxo comercial','Fluxo Comercial','success'],
    'commercialFlow:checkInDone':['Check-in realizado no fluxo comercial','Fluxo Comercial','success'],
    'commercialFlow:attendanceStarted':['Atendimento iniciado no fluxo comercial','Fluxo Comercial','info'],
    'commercialFlow:attendanceFinished':['Atendimento finalizado no fluxo comercial','Fluxo Comercial','success'],
    'commercialFlow:serviceOrderOpened':['Comanda aberta no fluxo comercial','Fluxo Comercial','success'],
    'commercialFlow:paymentDone':['Pagamento concluído no fluxo comercial','Fluxo Comercial','success'],
    'commercialFlow:receiptGenerated':['Recibo gerado no fluxo comercial','Fluxo Comercial','success'],
    'commercialFlow:cashbackGenerated':['Cashback confirmado no fluxo comercial','Fluxo Comercial','success'],
    'commercialFlow:reviewCreated':['Avaliação criada no fluxo comercial','Fluxo Comercial','success'],
    'commercialFlow:completed':['Fluxo comercial concluído','Fluxo Comercial','success'],
    'flow:started':['Fluxo completo iniciado','Atendimento Completo','info'],
    'flow:clientCreated':['Cliente criado no fluxo','Atendimento Completo','success'],
    'flow:appointmentCreated':['Agendamento criado no fluxo','Atendimento Completo','success'],
    'flow:appointmentConfirmed':['Agendamento confirmado no fluxo','Atendimento Completo','success'],
    'flow:checkInDone':['Check-in realizado no fluxo','Atendimento Completo','success'],
    'flow:attendanceStarted':['Atendimento iniciado no fluxo','Atendimento Completo','info'],
    'flow:attendanceFinished':['Atendimento finalizado no fluxo','Atendimento Completo','success'],
    'flow:serviceOrderOpened':['Comanda aberta no fluxo','Atendimento Completo','success'],
    'flow:itemAdded':['Item adicionado no fluxo','Atendimento Completo','info'],
    'flow:paymentDone':['Pagamento realizado no fluxo','Atendimento Completo','success'],
    'flow:receiptGenerated':['Recibo gerado no fluxo','Atendimento Completo','success'],
    'flow:stockUpdated':['Estoque atualizado no fluxo','Atendimento Completo','success'],
    'flow:apiSynced':['API sincronizada no fluxo','Atendimento Completo','success'],
    'flow:apiSyncFallback':['API indisponível no fluxo','Atendimento Completo','warning'],
    'flow:cashbackGenerated':['Cashback gerado no fluxo','Atendimento Completo','success'],
    'flow:reviewCreated':['Avaliação criada no fluxo','Atendimento Completo','success'],
    'flow:completed':['Fluxo completo concluído','Atendimento Completo','success']
  };
  const clone = value => JSON.parse(JSON.stringify(value));
  const read = () => {
    try {
      const current = JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]');
      if (Array.isArray(current) && current.length) return current;
      for (const key of LEGACY_KEYS) {
        const legacy = JSON.parse(localStorage.getItem(key) || '[]');
        if (Array.isArray(legacy) && legacy.length) {
          const migrated = legacy.map(normalizeEvent);
          write(migrated);
          localStorage.removeItem(key);
          return migrated;
        }
      }
    } catch { /* localStorage unavailable: return empty history */ }
    return [];
  };
  const write = events => {
    try { localStorage.setItem(STORAGE_KEY, JSON.stringify(events.slice(-500))); } catch { /* ignore quota errors in demo */ }
  };
  const normalizeEvent = (raw = {}) => {
    const eventName = raw.eventName || raw.name || 'demo:event';
    const meta = titles[eventName] || [eventName.replaceAll(':', ' › '), raw.module || 'BarberSync', raw.severity || 'info'];
    const payload = raw.payload || {};
    return {
      id: raw.id || `evt-${Date.now()}-${Math.random().toString(16).slice(2, 8)}`,
      eventName,
      title: raw.title || payload.title || meta[0],
      description: raw.description || payload.description || payload.message || meta[0],
      module: raw.module || payload.module || meta[1],
      payload,
      createdAt: raw.createdAt || raw.at || new Date().toISOString(),
      severity: raw.severity || payload.severity || meta[2]
    };
  };
  const notify = event => {
    const text = `${event.module}: ${event.title}`;
    if (window.AdminToast?.show) window.AdminToast.show(text, event.severity === 'danger' ? 'error' : event.severity);
    else if (window.BarberSyncToast?.show) window.BarberSyncToast.show(text, event.severity);
  };
  const call = (eventName, payload, event) => (listeners.get(eventName) || []).forEach(cb => {
    try { cb(payload, event); } catch (err) { console.error('[BarberSyncEventBus]', err); }
  });
  const api = {
    on(eventName, callback) { if (!listeners.has(eventName)) listeners.set(eventName, new Set()); listeners.get(eventName).add(callback); return () => api.off(eventName, callback); },
    off(eventName, callback) { listeners.get(eventName)?.delete(callback); },
    emit(eventName, payload = {}) {
      const event = normalizeEvent({ eventName, payload });
      const events = read(); events.push(event); write(events);
      window.dispatchEvent(new CustomEvent('barbersync:event', { detail: event }));
      window.dispatchEvent(new CustomEvent(`barbersync:${eventName}`, { detail: payload }));
      call(eventName, payload, event); call('*', payload, event);
      notify(event);
      if ((eventName.startsWith('flow:') || eventName.startsWith('commercialFlow:')) && window.BarberSyncDemoStore) {
        try {
          const store = window.BarberSyncDemoStore;
          store.add('dashboardEvents', { type:'flow', title:event.title, module:event.module, eventName:event.eventName, at:event.createdAt });
          const settings = store.getSettings ? store.getSettings() : {};
          const notifications = settings.notifications || {};
          notifications.items = [{ title:event.title, module:event.module, type:event.severity, read:false, route:eventName.startsWith('commercialFlow:') ? '/Admin/CommercialFlow' : '/Admin/FullServiceFlow' }, ...(notifications.items || [])].slice(0, 20);
          notifications.unread = (notifications.items || []).filter(x => !x.read).length;
          store.updateSettings?.('notifications', notifications);
          store.add('reports', { name:'Auditoria demo - ' + event.title, status:'Registrado', module:event.module, eventName:event.eventName, at:event.createdAt });
          store.refreshDashboard?.();
        } catch (err) { console.warn('[BarberSyncEventBus] fluxo demo auditável indisponível', err); }
      }
      return clone(event);
    },
    history() { return clone(read()); },
    clearHistory() { write([]); window.dispatchEvent(new CustomEvent('barbersync:event-history-cleared')); },
    last(count = 10) { return clone(read().slice(-Number(count || 10)).reverse()); },
    byType(eventName) { return clone(read().filter(e => e.eventName === eventName)); },
    exportHistory() { return JSON.stringify(read(), null, 2); }
  };
  window.BarberSyncEventBus = api;
})();
