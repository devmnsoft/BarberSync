(() => {
  const STORE_KEY = 'barbersync.public.demo.store.v1';
  const EVENTS_KEY = 'barbersync.public.demo.events.v1';
  const readJson = (key, fallback) => { try { return JSON.parse(localStorage.getItem(key) || JSON.stringify(fallback)); } catch { return fallback; } };
  const writeJson = (key, value) => { try { localStorage.setItem(key, JSON.stringify(value)); } catch { /* demo storage may be unavailable */ } };
  const eventBus = {
    emit(eventName, payload = {}) {
      const event = { id: `pub-evt-${Date.now()}`, eventName, payload, channel: 'PublicWeb', createdAt: new Date().toISOString() };
      const events = readJson(EVENTS_KEY, []); events.unshift(event); writeJson(EVENTS_KEY, events.slice(0, 150));
      window.dispatchEvent(new CustomEvent(`barbersync:${eventName}`, { detail: payload }));
      return event;
    },
    history() { return readJson(EVENTS_KEY, []); }
  };
  const demoStore = {
    add(collection, item) {
      const state = readJson(STORE_KEY, { leads: [], appointments: [], roiCalculations: [] });
      state[collection] = Array.isArray(state[collection]) ? state[collection] : [];
      state[collection].unshift({ ...item, updatedAt: new Date().toISOString() });
      writeJson(STORE_KEY, state);
      return state[collection][0];
    },
    get(collection) { return readJson(STORE_KEY, {})[collection] || []; },
    exportState() { return readJson(STORE_KEY, {}); }
  };
  window.BarberSyncEventBus = window.BarberSyncEventBus || eventBus;
  window.BarberSyncDemoStore = window.BarberSyncDemoStore || demoStore;

  const cfg = (() => { try { return JSON.parse(localStorage.getItem('barbersync.demo.state.v9') || '{}').settings || {}; } catch { return {}; } })();
  const params = new URLSearchParams(location.search);
  const branding = { ...(cfg.branding || {}), publicName: params.get('company') || cfg.branding?.publicName, slogan: params.get('slogan') || cfg.branding?.slogan, primary: params.get('primary') || cfg.branding?.primary, secondary: params.get('secondary') || cfg.branding?.secondary };
  const publicWeb = { ...(cfg.publicWeb || {}), heroTitle: params.get('title') || cfg.publicWeb?.heroTitle, heroSubtitle: params.get('subtitle') || cfg.publicWeb?.heroSubtitle, ctaPrimary: params.get('cta') || cfg.publicWeb?.ctaPrimary };
  const fallbackServices = publicWeb.services || [{name:'Corte Masculino',price:45,durationMinutes:40,category:'Barbearia',description:'Corte premium com finalização.'},{name:'Combo Corte + Barba',price:78,durationMinutes:70,category:'Combo',description:'Experiência completa com cashback.'},{name:'Barboterapia',price:65,durationMinutes:50,category:'Premium',description:'Relaxamento e cuidado da barba.'}];
  const fallbackPros = publicWeb.professionals || [{name:'Rafael Barber',specialty:'Corte e barba',rating:4.9},{name:'Bruna Estética',specialty:'Estética premium',rating:4.8},{name:'Marcos Fade',specialty:'Degradê e desenho',rating:4.7}];
  if (branding.primary) document.documentElement.style.setProperty('--public-primary', branding.primary);
  if (branding.secondary) document.documentElement.style.setProperty('--public-gold', branding.secondary);
  const setText = (selector, value) => { const el = document.querySelector(selector); if (el && value) el.textContent = value; };
  setText('[data-public-title]', publicWeb.heroTitle);
  setText('[data-public-subtitle]', publicWeb.heroSubtitle);
  setText('[data-public-cta-primary]', publicWeb.ctaPrimary);
  setText('[data-public-cta-secondary]', publicWeb.ctaSecondary);
  setText('[data-public-company]', branding.publicName);
  setText('[data-public-slogan]', branding.slogan);
  const toast = (message) => { const el = document.getElementById('publicToast'); if (!el) return; el.textContent = message; el.hidden = false; setTimeout(() => el.hidden = true, 4200); };
  const render=(id,arr,fn)=>{const e=document.getElementById(id); if(e) { e.classList.remove('skeleton-grid'); e.innerHTML=(Array.isArray(arr)?arr:[]).map(fn).join(''); }};
  const load = async (url, fallback) => {
    try {
      const r = await fetch(url, { headers: { Accept: 'application/json' } });
      if (!r.ok) throw new Error(`HTTP ${r.status}`);
      const j = await r.json(); const data = j.data || j.items || j;
      return Array.isArray(data) && data.length ? data : fallback;
    } catch (error) {
      console.warn('[BarberSync PublicWeb] fallback demo', error?.message || error);
      window.BarberSyncEventBus.emit('public:fallbackUsed', { url, message: error?.message || String(error) });
      return fallback;
    }
  };
  Promise.all([load('/PublicApi/services', fallbackServices), load('/PublicApi/professionals', fallbackPros)]).then(([services, pros]) => {
    render('services', services.slice(0,6), s=>`<article class='panel service-card'><span class='public-badge'>${s.category||'Publicado'}</span><h3>${s.name||'Serviço'}</h3><p>${s.description||'Atendimento premium publicado no Admin.'}</p><div><strong>R$ ${Number(s.price??45).toFixed(2).replace('.',',')}</strong><small>${s.durationMinutes||40} min</small></div></article>`);
    render('pros', pros.slice(0,5), p=>`<article class='panel pro-card'><div class='avatar'>${(p.name||'B').substring(0,1)}</div><h3>${p.name||'Profissional'}</h3><p>${p.specialty||'Especialista BarberSync'}</p><span class='public-badge'>⭐ ${p.rating||4.8} • Publicado</span></article>`);
    window.BarberSyncEventBus.emit('public:catalogLoaded', { services: services.length, professionals: pros.length });
  });
  document.getElementById('publicAppointment')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!e.target.checkValidity()) { e.target.reportValidity(); return; }
    const result = document.getElementById('publicAppointmentResult');
    const body = Object.fromEntries(new FormData(e.target).entries());
    result.textContent = 'Enviando solicitação segura via /PublicApi...';
    const protocol = `PUB-2026-${String(window.BarberSyncDemoStore.get('appointments').length + 1).padStart(4, '0')}`;
    let responsePayload = { data: { protocol } };
    let status = 'Solicitado';
    try {
      const r = await fetch('/PublicApi/appointments', { method:'POST', headers:{'Content-Type':'application/json', Accept:'application/json'}, body:JSON.stringify(body)});
      responsePayload = await r.json().catch(()=>responsePayload);
      if (!r.ok) status = 'Demo local';
    } catch (error) {
      status = 'Demo local';
      responsePayload = { message: error?.message || 'Fallback local' };
    }
    const responseProtocol = responsePayload?.data?.protocol || responsePayload?.protocol || protocol;
    const isDemo = !!responsePayload?.isDemo || !!responsePayload?.data?.isDemo || status === 'Demo local';
    const appointment = { id: responseProtocol, protocol: responseProtocol, ...body, status: isDemo ? 'Demo local' : 'Persistido', response: responsePayload, channel:'PublicWeb', createdAt: new Date().toISOString() };
    if (isDemo) {
      window.BarberSyncDemoStore.add('appointments', appointment);
      window.BarberSyncDemoStore.add('leads', { ...appointment, source: 'PublicWeb' });
    }
    window.BarberSyncEventBus.emit('public:leadCreated', appointment);
    window.BarberSyncEventBus.emit('public:appointmentRequested', appointment);
    result.innerHTML = `<strong>Protocolo ${responseProtocol}</strong><br>${isDemo ? 'API indisponível. Solicitação simulada em modo demonstração.' : 'Solicitação persistida via /PublicApi/appointments.'}<div class='hero-actions' style='margin-top:12px'><a class='btn-primary' href='http://localhost:8081/Admin/LeadToCash' target='_blank' rel='noopener'>Ver no Admin</a><a class='btn-secondary' href='http://localhost:8081/Admin/FullServiceFlow' target='_blank' rel='noopener'>Executar fluxo completo</a></div>`;
    toast(isDemo ? 'Operação simulada em modo demonstração.' : 'Solicitação enviada e persistida com sucesso.'); e.target.reset();
  });
})();

(() => { const f=document.getElementById('roiCalculator'); if(!f)return; const calc=()=>{const d=new FormData(f); const att=+d.get('att')||0,t=+d.get('ticket')||0,pros=+d.get('pros')||0,days=+d.get('days')||0; const revenue=att*t*days; const recurrence=revenue*.12; const noShow=revenue*.06; const cashback=revenue*.04; const payload={att,ticket:t,pros,days,revenue,recurrence,noShow,cashback,createdAt:new Date().toISOString()}; window.BarberSyncDemoStore?.add('roiCalculations', payload); window.BarberSyncEventBus?.emit('public:roiCalculated', payload); document.getElementById('roiResult').innerHTML=`Receita mensal estimada: <strong>R$ ${revenue.toLocaleString('pt-BR')}</strong><br>Ganho com recorrência: R$ ${recurrence.toLocaleString('pt-BR',{maximumFractionDigits:0})}<br>Redução de faltas: R$ ${noShow.toLocaleString('pt-BR',{maximumFractionDigits:0})}<br>Cashback/campanhas: R$ ${cashback.toLocaleString('pt-BR',{maximumFractionDigits:0})}<br>Profissionais: ${pros}`;}; f.addEventListener('submit',e=>{e.preventDefault();calc();}); calc(); })();
