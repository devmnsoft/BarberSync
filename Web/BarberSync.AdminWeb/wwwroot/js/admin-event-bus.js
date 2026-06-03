(() => {
  const STORAGE_KEY = 'barbersync.demo.events';
  const listeners = new Map();
  const read = () => { try { return JSON.parse(localStorage.getItem(STORAGE_KEY) || '[]'); } catch { return []; } };
  const write = events => localStorage.setItem(STORAGE_KEY, JSON.stringify(events.slice(-200)));
  const toast = event => {
    const message = `${event.name.replaceAll(':', ' › ')} registrado`;
    if (window.AdminToast?.show) window.AdminToast.show(message, 'success');
    else if (window.BarberSyncToast?.show) window.BarberSyncToast.show(message, 'success');
    else window.dispatchEvent(new CustomEvent('barbersync:toast', { detail: { type: 'success', message } }));
  };
  const api = {
    on(eventName, callback) {
      if (!listeners.has(eventName)) listeners.set(eventName, new Set());
      listeners.get(eventName).add(callback);
      return () => api.off(eventName, callback);
    },
    off(eventName, callback) { listeners.get(eventName)?.delete(callback); },
    emit(eventName, payload = {}) {
      const event = { id: `evt-${Date.now()}-${Math.random().toString(16).slice(2, 8)}`, name: eventName, payload, at: new Date().toISOString() };
      const events = read(); events.push(event); write(events);
      window.dispatchEvent(new CustomEvent('barbersync:event', { detail: event }));
      window.dispatchEvent(new CustomEvent(`barbersync:${eventName}`, { detail: payload }));
      (listeners.get(eventName) || []).forEach(cb => { try { cb(payload, event); } catch (err) { console.error('[BarberSyncEventBus]', err); } });
      (listeners.get('*') || []).forEach(cb => { try { cb(payload, event); } catch (err) { console.error('[BarberSyncEventBus]', err); } });
      toast(event);
      return event;
    },
    history() { return read(); },
    clearHistory() { write([]); window.dispatchEvent(new CustomEvent('barbersync:event-history-cleared')); }
  };
  window.BarberSyncEventBus = api;
})();
