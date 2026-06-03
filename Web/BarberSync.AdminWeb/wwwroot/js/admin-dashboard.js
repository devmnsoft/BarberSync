(() => {
  const fallback = { kpis:[
    {label:'Receita hoje',value:'R$ 4.850',trend:'+12%'},{label:'Agendamentos hoje',value:'26',trend:'+6'},{label:'Clientes ativos',value:'412',trend:'+31'},{label:'Comandas abertas',value:'9',trend:'R$ 780'},
    {label:'Estoque crítico',value:'4',trend:'repor'},{label:'Avaliação média',value:'4,8',trend:'⭐'},{label:'Campanhas ativas',value:'3',trend:'CRM'},{label:'Cashback gerado',value:'R$ 380',trend:'fidelidade'},
    {label:'Conversão PublicWeb',value:'18%',trend:'leads'},{label:'Atendimentos pelo Totem',value:'7',trend:'fila menor'},{label:'Clientes reativados',value:'8',trend:'recorrência'},{label:'Receita por canal',value:'A 1800|P 700|T 420|M 380',trend:'mix'}],
    topServices:[{name:'Corte Masculino',price:65},{name:'Corte + Barba',price:95},{name:'Barba Tradicional',price:45}],
    appointments:[{client:'Cliente Demo',service:'Corte + Barba',time:'10:00',status:'Confirmado'}], stockCritical:[{name:'Pomada Modeladora',current:4,minimum:12}], copilotSuggestions:[{title:'Retenção',description:'Acionar clientes inativos há 45 dias.'},{title:'Estoque',description:'Repor Pomada Modeladora.'}]
  };
  const icons=['💰','📅','👥','🧾','📦','⭐','📣','💎','🌐','🖥️','🔁','📊'];
  const unwrap = payload => payload?.data && !Array.isArray(payload) ? payload.data : payload;
  const list=(arr,fn)=> (Array.isArray(arr)?arr:[]).slice(0,5).map(fn).join('');
  const storeData=()=> {
    const store=window.BarberSyncDemoStore; if(!store) return null;
    const state=store.exportState ? JSON.parse(store.exportState()) : null; if(!state) return null;
    return { kpis: state.dashboard?.kpis || fallback.kpis, topServices: state.services || fallback.topServices, appointments: state.appointments || [], stockCritical: (state.products || []).filter(p => p.status === 'Crítico' || Number(p.stock ?? p.quantity) <= Number(p.minimumStock)), copilotSuggestions: state.copilot || fallback.copilotSuggestions };
  };
  const render=(payload)=>{
    const data = storeData() || unwrap(payload) || fallback;
    const kpis = Array.isArray(data.kpis) && data.kpis.length ? data.kpis : fallback.kpis;
    const kpiEl=document.querySelector('[data-dashboard-kpis]');
    if(kpiEl) kpiEl.innerHTML=kpis.map((k,i)=>`<article class='kpi-card'><div class='kpi-icon'>${icons[i]||'📊'}</div><div><p class='kpi-label'>${k.label}</p><strong class='kpi-value'>${k.value}</strong><span class='kpi-variation'>${k.trend||'demo'}</span></div></article>`).join('');
    const svc=document.querySelector('[data-top-services]'); if(svc) svc.innerHTML=list(data.topServices||data.services||fallback.topServices,s=>`<li><strong>${s.name}</strong><span>R$ ${Number(s.price||0).toFixed(2).replace('.',',')}</span></li>`);
    const ap=document.querySelector('[data-next-appointments]'); if(ap) ap.innerHTML=list(data.appointments||fallback.appointments,a=>`<li><strong>${a.time||'--:--'} • ${a.client || a.clientName || 'Cliente'}</strong><span>${a.service || a.serviceName || a.status}</span></li>`);
    const st=document.querySelector('[data-stock-critical]'); if(st) st.innerHTML=list(data.stockCritical||fallback.stockCritical,s=>`<li><strong>${s.name}</strong><span>${s.current ?? s.currentStock ?? s.stock ?? s.quantity}/${s.minimum ?? s.minimumStock ?? s.minStock}</span></li>`);
    const co=document.querySelector('[data-copilot-list]'); if(co) co.innerHTML=list(data.copilotSuggestions||fallback.copilotSuggestions,c=>`<li><strong>${c.title||c.category||'Copilot'}</strong><span>${c.description||c.summary||c}</span></li>`);
  };
  document.addEventListener('DOMContentLoaded',async()=>{
    document.querySelectorAll('[data-demo-action]').forEach(btn => btn.addEventListener('click', () => window.AdminToast?.show?.(btn.dataset.demoAction || 'Ação executada em modo demonstração.', 'success')));
    document.querySelectorAll('[data-quick-action]').forEach(btn => btn.addEventListener('click', () => window.sessionStorage.setItem('BarberSync:lastQuickAction', btn.dataset.quickAction || 'Ação rápida')));
    document.querySelector('[data-dashboard-refresh]')?.addEventListener('click', () => { window.BarberSyncDemoStore?.updateDashboardCounters(); render(); window.AdminToast?.show?.('Indicadores atualizados pelo demo store.', 'success'); });
    window.addEventListener('barbersync:demo-store-changed', () => render());
    if(!document.querySelector('[data-dashboard-kpis]')) return;
    render();
    const [dashboard, services, appointments, stock, copilot] = await Promise.all([
      adminApiClient.get('/AdminApi/dashboard', fallback), adminApiClient.get('/AdminApi/services', fallback.topServices), adminApiClient.get('/AdminApi/appointments', fallback.appointments), adminApiClient.get('/AdminApi/stock-critical', fallback.stockCritical), adminApiClient.get('/AdminApi/copilot-suggestions', fallback.copilotSuggestions)
    ]);
    const data = { ...(unwrap(dashboard.data) || fallback), topServices: unwrap(services.data) || fallback.topServices, appointments: unwrap(appointments.data) || fallback.appointments, stockCritical: unwrap(stock.data) || fallback.stockCritical, copilotSuggestions: unwrap(copilot.data) || fallback.copilotSuggestions };
    if(!window.BarberSyncDemoStore) render(data);
  });
})();
