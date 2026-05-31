(() => {
  const fallback = { kpis:[
    {label:'Receita hoje',value:'R$ 4.850',trend:'+12%'},{label:'Receita mês',value:'R$ 97.800',trend:'+18%'},{label:'Agendamentos hoje',value:'26',trend:'+6'},{label:'Clientes ativos',value:'412',trend:'+31'},
    {label:'Atendimentos em andamento',value:'7',trend:'agora'},{label:'Comandas abertas',value:'9',trend:'R$ 780'},{label:'Ticket médio',value:'R$ 83',trend:'+9%'},{label:'Estoque crítico',value:'4',trend:'repor'},
    {label:'Avaliação média',value:'4,8',trend:'⭐'},{label:'Campanhas ativas',value:'3',trend:'CRM'},{label:'Totem online',value:'1',trend:'online'},{label:'Profissionais disponíveis',value:'5',trend:'escala'}],
    topServices:[{name:'Corte Masculino',price:45},{name:'Corte + Barba',price:70},{name:'Barba Tradicional',price:35}],
    appointments:[{client:'Cliente Demo',service:'Corte + Barba',time:'10:00',status:'Confirmado'}],
    stockCritical:[{name:'Lâmina Platinum',current:6,minimum:20}],
    copilotSuggestions:[{title:'Retenção',description:'Acionar clientes inativos há 45 dias.'}]
  };
  const icons=['💰','📈','📅','👥','✂️','🧾','🎯','📦','⭐','📣','🖥️','💈'];
  const list=(arr,fn)=> (Array.isArray(arr)?arr:[]).slice(0,5).map(fn).join('');
  const unwrap = payload => payload?.data && !Array.isArray(payload) ? payload.data : payload;
  const render=(payload)=>{
    const data = unwrap(payload) || fallback;
    const kpis = Array.isArray(data.kpis) && data.kpis.length ? data.kpis : fallback.kpis;
    const kpiEl=document.querySelector('[data-dashboard-kpis]');
    if(kpiEl) kpiEl.innerHTML=kpis.map((k,i)=>`<article class='kpi-card'><div class='kpi-icon'>${icons[i]||'📊'}</div><div><p class='kpi-label'>${k.label}</p><strong class='kpi-value'>${k.value}</strong><span class='kpi-variation'>${k.trend||'demo'}</span></div></article>`).join('');
    const svc=document.querySelector('[data-top-services]'); if(svc) svc.innerHTML=list(data.topServices||data.services||fallback.topServices,s=>`<li><strong>${s.name}</strong><span>R$ ${Number(s.price||0).toFixed(2).replace('.',',')}</span></li>`);
    const ap=document.querySelector('[data-next-appointments]'); if(ap) ap.innerHTML=list(data.appointments||fallback.appointments,a=>`<li><strong>${a.time||'--:--'} • ${a.client}</strong><span>${a.service||a.status}</span></li>`);
    const st=document.querySelector('[data-stock-critical]'); if(st) st.innerHTML=list(data.stockCritical||fallback.stockCritical,s=>`<li><strong>${s.name}</strong><span>${s.current}/${s.minimum}</span></li>`);
    const co=document.querySelector('[data-copilot-list]'); if(co) co.innerHTML=list(data.copilotSuggestions||fallback.copilotSuggestions,c=>`<li><strong>${c.title||'Copilot'}</strong><span>${c.description||c}</span></li>`);
  };
  document.addEventListener('DOMContentLoaded',async()=>{
    if(!document.querySelector('[data-dashboard-kpis]')) return;
    const [dashboard, services, professionals, appointments, orders, products, stock, copilot] = await Promise.all([
      adminApiClient.get('/AdminApi/dashboard', fallback),
      adminApiClient.get('/AdminApi/services', fallback.topServices),
      adminApiClient.get('/AdminApi/professionals', []),
      adminApiClient.get('/AdminApi/appointments', fallback.appointments),
      adminApiClient.get('/AdminApi/service-orders', []),
      adminApiClient.get('/AdminApi/products', []),
      adminApiClient.get('/AdminApi/stock-critical', fallback.stockCritical),
      adminApiClient.get('/AdminApi/copilot-suggestions', fallback.copilotSuggestions)
    ]);
    const data = { ...(unwrap(dashboard.data) || fallback) };
    data.topServices = unwrap(services.data) || data.topServices;
    data.featuredProfessionals = unwrap(professionals.data) || data.featuredProfessionals;
    data.appointments = unwrap(appointments.data) || data.appointments;
    data.serviceOrders = unwrap(orders.data) || data.serviceOrders;
    data.products = unwrap(products.data) || data.products;
    data.stockCritical = unwrap(stock.data) || data.stockCritical;
    data.copilotSuggestions = unwrap(copilot.data) || data.copilotSuggestions;
    render(data);
  });
})();
