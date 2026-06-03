(() => {
  const store = () => window.BarberSyncDemoStore;
  const money = v => Number(v||0).toLocaleString('pt-BR',{style:'currency',currency:'BRL'});
  function refreshDashboardFromStore(){ if(!store()) return; const s=store(); const summary=s.dashboardSummary(); document.querySelectorAll('[data-dashboard-kpi="revenue"]').forEach(e=>e.textContent=money(summary.revenue)); document.querySelectorAll('[data-dashboard-kpi="appointments"]').forEach(e=>e.textContent=summary.appointments); document.querySelectorAll('[data-dashboard-kpi="cashback"]').forEach(e=>e.textContent=money(summary.cashback)); document.querySelectorAll('[data-dashboard-kpi="reviews"]').forEach(e=>e.textContent=summary.reviews); const panel=document.querySelector('[data-dashboard-v5]'); if(panel){ const st=s.exportState(); panel.innerHTML=`<article class="kpi-card"><p class="kpi-label">Receita hoje</p><strong class="kpi-value">${money(summary.revenue)}</strong><span>Admin, PublicWeb, Totem e Mobile</span></article><article class="kpi-card"><p class="kpi-label">Fluxo de hoje</p><strong class="kpi-value">${summary.appointments}</strong><span>${summary.paidOrders} comandas pagas</span></article><article class="kpi-card"><p class="kpi-label">Conversão PublicWeb</p><strong class="kpi-value">${summary.publicConversion}%</strong><span>${summary.leads} leads</span></article><article class="kpi-card"><p class="kpi-label">Uso do Totem</p><strong class="kpi-value">${summary.kiosk}</strong><span>atendimentos</span></article><article class="kpi-card"><p class="kpi-label">Cashback</p><strong class="kpi-value">${money(summary.cashback)}</strong><span>gerado/acumulado</span></article><article class="kpi-card"><p class="kpi-label">Estoque crítico</p><strong class="kpi-value">${summary.criticalStock}</strong><span>${st.products.filter(p=>p.stock<=p.minStock).map(p=>p.name).join(', ')||'Sem alertas'}</span></article>`; } const ev=document.querySelector('[data-dashboard-events]'); if(ev) ev.innerHTML=(window.BarberSyncEventBus?.history().slice(-8).reverse()||[]).map(e=>`<div class="demo5-event"><strong>${e.name}</strong><small> ${new Date(e.at).toLocaleTimeString('pt-BR')}</small></div>`).join(''); }
  window.refreshDashboardFromStore = refreshDashboardFromStore;
  ['public:leadCreated','appointment:created','appointment:checkedIn','serviceOrder:paid','stock:changed','review:created','campaign:created','kiosk:attendanceCreated','dashboard:refresh'].forEach(ev=>window.BarberSyncEventBus?.on(ev, refreshDashboardFromStore));
  window.addEventListener('barbersync:store-changed', refreshDashboardFromStore); document.addEventListener('DOMContentLoaded', refreshDashboardFromStore);
})();

(() => {
  const store = () => window.BarberSyncDemoStore;
  const bus = () => window.BarberSyncEventBus;
  const row = (k,v) => `<div class="saas-row"><strong>${k}</strong><span>${v}</span></div>`;
  function renderOmnichannel(){
    const root=document.querySelector('[data-dashboard-omnichannel]'); if(!root) return;
    const s=store(); if(!s) return;
    const leads=s.get('leads'), appointments=s.get('appointments'), attendances=s.get('attendances'), payments=s.get('payments'), reviews=s.get('reviews');
    const origins=['PublicWeb','Totem','Manual','Mobile'];
    const leadHost=document.querySelector('[data-dashboard-leads-origin]'); if(leadHost) leadHost.innerHTML=origins.map(o=>row(o, leads.filter(l=>(l.source||'Manual')===o).length)).join('');
    const conv=document.querySelector('[data-dashboard-conversion]'); if(conv) conv.innerHTML=row('Leads → Agendamentos', `${appointments.length}/${Math.max(1,leads.length)}`)+row('Agendamentos → Atendimentos', `${attendances.length}/${Math.max(1,appointments.length)}`)+row('Atendimentos → Pagamentos', `${payments.length}/${Math.max(1,attendances.length)}`)+row('Pagamentos → Avaliações', `${reviews.length}/${Math.max(1,payments.length)}`);
    const alerts=document.querySelector('[data-dashboard-alerts]'); if(alerts) alerts.innerHTML=[...s.get('products').filter(p=>p.stock<=p.minStock).map(p=>`<li>Repor ${p.name}: ${p.stock}/${p.minStock}.</li>`),'<li>Clientes inativos prontos para campanha de retorno.</li>','<li>Totem online e captando autoatendimentos.</li>'].join('');
    const events=document.querySelector('[data-dashboard-events]'); if(events) events.innerHTML=(bus()?.last(8)||[]).map(e=>`<article class="saas-event ${e.severity}"><strong>${e.title}</strong><p>${e.module} • ${e.eventName}</p></article>`).join('');
  }
  bus()?.on('*', renderOmnichannel); document.addEventListener('DOMContentLoaded', renderOmnichannel);
})();
