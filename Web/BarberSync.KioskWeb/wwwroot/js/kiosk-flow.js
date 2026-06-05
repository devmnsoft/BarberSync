(() => {
  const FLOW_KEY = 'kiosk-flow';
  const SUMMARY_KEY = 'barbersync.kiosk.summary';
  const STORE_KEY = 'barbersync.kiosk.demo.store.v1';
  const EVENTS_KEY = 'barbersync.kiosk.demo.events.v1';
  const readJson = (storage, key, fallback) => { try { return JSON.parse(storage.getItem(key) || JSON.stringify(fallback)); } catch { return fallback; } };
  const writeJson = (storage, key, value) => { try { storage.setItem(key, JSON.stringify(value)); } catch { /* kiosk storage can be disabled */ } };
  const emit = (eventName, payload = {}) => {
    const event = { id: `kiosk-evt-${Date.now()}`, eventName, payload, channel: 'Kiosk', createdAt: new Date().toISOString() };
    const events = readJson(localStorage, EVENTS_KEY, []); events.unshift(event); writeJson(localStorage, EVENTS_KEY, events.slice(0, 150));
    window.dispatchEvent(new CustomEvent(`barbersync:${eventName}`, { detail: payload }));
    return event;
  };
  const persist = (collection, item) => {
    const state = readJson(localStorage, STORE_KEY, { attendances: [], payments: [], reviews: [], serviceOrders: [] });
    state[collection] = Array.isArray(state[collection]) ? state[collection] : [];
    state[collection].unshift({ ...item, updatedAt: new Date().toISOString() });
    writeJson(localStorage, STORE_KEY, state);
    return state[collection][0];
  };

  window.BarberSyncEventBus = window.BarberSyncEventBus || { emit, history: () => readJson(localStorage, EVENTS_KEY, []) };
  window.BarberSyncDemoStore = window.BarberSyncDemoStore || { add: persist, get: collection => readJson(localStorage, STORE_KEY, {})[collection] || [], exportState: () => readJson(localStorage, STORE_KEY, {}) };

  window.KioskFlow = {
    deviceCode: 'KIOSK-DEMO-001',
    get state(){ return readJson(sessionStorage, FLOW_KEY, {}); },
    setState(patch){
      const next = { ...this.state, ...patch, updatedAt: new Date().toISOString(), deviceCode: this.deviceCode };
      writeJson(sessionStorage, FLOW_KEY, next);
      if(next.serviceId || next.serviceName) sessionStorage.setItem('selectedService', JSON.stringify({ id: next.serviceId, name: next.serviceName, amount: next.amount }));
      if(next.client) sessionStorage.setItem('selectedClient', JSON.stringify(next.client));
      if(next.professionalId || next.professionalName) sessionStorage.setItem('selectedProfessional', JSON.stringify({ id: next.professionalId, name: next.professionalName }));
      if(next.paymentMethod || next.payment) sessionStorage.setItem('selectedPayment', JSON.stringify(next.payment || { method: next.paymentMethod, status: next.status }));
      return next;
    },
    async post(url, body, fallback){
      try{
        const r = await fetch(url,{method:'POST',headers:{'Content-Type':'application/json',Accept:'application/json'},body:JSON.stringify(body)});
        if(!r.ok) throw new Error(`HTTP ${r.status}`);
        return await r.json();
      } catch (error) {
        console.warn('[BarberSync KioskFlow] fallback demo', error?.message || error);
        window.BarberSyncEventBus.emit('kiosk:fallbackUsed', { url, message: error?.message || String(error) });
        return fallback;
      }
    },
    finish(paymentMethod='PIX'){
      const attendanceId = `KIOSK-${Date.now().toString().slice(-6)}`;
      const serviceOrderNumber = `TK-${Math.floor(1000+Math.random()*9000)}`;
      const payment = { id:`PAY-${Date.now().toString().slice(-6)}`, method:paymentMethod, status:'APPROVED', amount:this.state.amount||70, at:new Date().toISOString() };
      const summary = { ...this.state, attendanceId, serviceOrderNumber, payment, paymentMethod, message:'Atendimento registrado. A comanda foi enviada para o painel.', channel:'Totem', status:'Atendimento registrado com sucesso' };
      writeJson(sessionStorage, SUMMARY_KEY, summary);
      writeJson(sessionStorage, 'kiosk-summary', summary);
      this.setState(summary);
      window.BarberSyncDemoStore.add('attendances', summary);
      window.BarberSyncDemoStore.add('serviceOrders', { id: serviceOrderNumber, number: serviceOrderNumber, attendanceId, client: summary.client?.name || 'Cliente demo', service: summary.serviceName || 'Serviço demo', professional: summary.professionalName || 'Primeiro disponível', total: payment.amount, status: 'Paid', channel: 'Kiosk' });
      window.BarberSyncDemoStore.add('payments', payment);
      window.BarberSyncEventBus.emit('kiosk:attendanceCreated', summary);
      window.BarberSyncEventBus.emit('kiosk:paymentCompleted', { summary, payment });
      return summary;
    },
    saveReview(review){
      const summary = { ...this.state, review, reviewedAt: new Date().toISOString() };
      writeJson(sessionStorage, SUMMARY_KEY, summary);
      writeJson(sessionStorage, 'kiosk-summary', summary);
      this.setState(summary);
      window.BarberSyncDemoStore.add('reviews', { ...review, attendanceId: summary.attendanceId, serviceOrderNumber: summary.serviceOrderNumber, channel: 'Kiosk' });
      window.BarberSyncEventBus.emit('kiosk:reviewCreated', { review, summary });
      return summary;
    }
  };

  (() => { const stateKey='barbersync.kiosk.v5.session'; const read=()=>readJson(sessionStorage,stateKey,{}); const set=(patch)=>{ const s=read(); const next={...s,...patch,updatedAt:new Date().toISOString()}; writeJson(sessionStorage,stateKey,next); return next; }; const messages={service:'Serviço selecionado: Corte Masculino • R$ 65 • 45 min.',combo:'Combo sugerido: adicionar Barba Tradicional por R$ 45.',client:'Cliente identificado com telefone visual: (11) 95555-0000.',professional:'Profissional: próximo disponível ou Rafael Barber.',confirm:'Confirmação: Corte + Barba • Rafael Barber • R$ 110.',payment:'Pagamento: PIX QR fake, cartão ou dinheiro habilitado.',success:'Sucesso: Comanda KSK-'+Math.floor(Math.random()*900+100)+' • espera estimada 12 min.',review:'Avaliação registrada: 5 estrelas.'}; document.addEventListener('click',e=>{ const step=e.target.closest('[data-kiosk-demo]')?.dataset.kioskDemo; const host=document.querySelector('[data-kiosk-v5-summary]'); if(step&&host){ const s=set({step,description:messages[step],deviceCode:'KIOSK-DEMO-001'}); host.innerHTML=`<strong>${messages[step]}</strong><br><small>Salvo em sessionStorage • ${s.deviceCode}</small>`; window.BarberSyncEventBus.emit('kiosk:demoStep', s); } if(e.target.closest('[data-kiosk-print]')) window.print(); if(e.target.closest('[data-kiosk-restart]')){ sessionStorage.removeItem(stateKey); if(host) host.textContent='Fluxo reiniciado. Escolha um novo serviço.'; window.BarberSyncEventBus.emit('kiosk:flowRestarted'); } }); })();
})();
