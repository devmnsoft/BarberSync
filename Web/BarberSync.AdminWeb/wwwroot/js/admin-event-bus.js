(() => {
  const listeners = new Map();
  const recentKey = 'barbersync.demo.events.v4';
  const labels = {
    'client:created': 'Cliente criado', 'client:updated': 'Cliente atualizado', 'appointment:created': 'Agendamento criado',
    'appointment:confirmed': 'Agendamento confirmado', 'appointment:checkedIn': 'Check-in realizado', 'appointment:started': 'Atendimento iniciado',
    'appointment:finished': 'Atendimento finalizado', 'serviceOrder:opened': 'Comanda aberta', 'serviceOrder:itemAdded': 'Item adicionado na comanda',
    'serviceOrder:paid': 'Comanda paga', 'serviceOrder:closed': 'Comanda fechada', 'stock:changed': 'Estoque alterado',
    'campaign:created': 'Campanha criada', 'coupon:created': 'Cupom criado', 'review:created': 'Avaliação recebida',
    'cashback:generated': 'Cashback gerado', 'dashboard:refresh': 'Dashboard atualizado', 'demo:scenarioLoaded': 'Cenário carregado'
  };
  const readRecent = () => {
    try { return JSON.parse(localStorage.getItem(recentKey) || '[]'); } catch { return []; }
  };
  const writeRecent = events => localStorage.setItem(recentKey, JSON.stringify(events.slice(0, 60)));
  const toast = (message, type = 'success') => {
    const t = window.AdminToast;
    if (t?.show) t.show(message, type);
    else if (t?.showInfo) t.showInfo(message);
  };
  const api = {
    on(eventName, callback) {
      if (!listeners.has(eventName)) listeners.set(eventName, new Set());
      listeners.get(eventName).add(callback);
      return () => api.off(eventName, callback);
    },
    off(eventName, callback) { listeners.get(eventName)?.delete(callback); },
    emit(eventName, payload = {}) {
      const entry = { id: `evt-${Date.now()}-${Math.random().toString(16).slice(2, 7)}`, eventName, label: labels[eventName] || eventName, payload, at: new Date().toISOString() };
      writeRecent([entry, ...readRecent()]);
      try { window.BarberSyncDemoStore?.recordEvent?.(entry); } catch (err) { console.warn('Demo event store update failed', err); }
      toast(entry.payload?.message || entry.label, eventName.includes('stock') ? 'warning' : 'success');
      [...(listeners.get(eventName) || [])].forEach(cb => { try { cb(payload, entry); } catch (err) { console.error('EventBus listener error', err); } });
      [...(listeners.get('*') || [])].forEach(cb => { try { cb(payload, entry); } catch (err) { console.error('EventBus wildcard error', err); } });
      if (eventName !== 'dashboard:refresh') window.dispatchEvent(new CustomEvent('barbersync:dashboard-refresh', { detail: entry }));
      window.dispatchEvent(new CustomEvent('barbersync:event', { detail: entry }));
      return entry;
    },
    recent() { return readRecent(); }
  };
  window.BarberSyncEventBus = api;
})();
