(() => {
  const store = () => window.BarberSyncDemoStore;
  const stars = n => '★★★★★'.slice(0, Number(n || 0)) + '☆☆☆☆☆'.slice(Number(n || 0));
  function render() {
    const s = store(); if (!s) return;
    const reviews = s.get('reviews');
    const avg = reviews.length ? reviews.reduce((a,r)=>a+Number(r.rating||0),0)/reviews.length : 0;
    const nps = reviews.length ? Math.round(reviews.reduce((a,r)=>a+Number(r.nps ?? ((r.rating||0)>=5?10:7)),0)/reviews.length*10) : 0;
    const kpis = document.getElementById('qualityKpis');
    if (kpis) kpis.innerHTML = `<article class="kpi-card"><p class="kpi-label">Nota média</p><strong class="kpi-value">${avg.toFixed(1)}</strong><span>${reviews.length} avaliação(ões)</span></article><article class="kpi-card"><p class="kpi-label">NPS demo</p><strong class="kpi-value">${nps}</strong><span>Atualizado pelo fluxo completo</span></article><article class="kpi-card"><p class="kpi-label">Promotores</p><strong class="kpi-value">${reviews.filter(r=>(r.rating||0)>=5).length}</strong></article><article class="kpi-card"><p class="kpi-label">Detratores</p><strong class="kpi-value">${reviews.filter(r=>(r.rating||0)<=3).length}</strong></article>`;
    const dist = document.getElementById('starDistribution');
    if (dist) dist.innerHTML = [5,4,3,2,1].map(n => `<div class="summary-row"><strong>${n} estrelas</strong><span>${reviews.filter(r=>Number(r.rating)===n).length}</span></div>`).join('');
    const plan = document.getElementById('recoveryPlan');
    if (plan) plan.innerHTML = `<div class="flow-event"><strong>Plano automático</strong><p>Promotores recebem pedido de indicação; neutros recebem mensagem de retorno; detratores abrem tarefa de recuperação.</p></div>`;
    const rows = document.getElementById('qualityRows');
    if (rows) rows.innerHTML = reviews.slice().reverse().map(r => `<tr><td>${r.client || r.clientId || 'Cliente'}</td><td>${stars(r.rating)}</td><td>${r.nps ?? '-'}</td><td>${r.comment || 'Sem comentário'}</td><td><a class="btn btn-light btn-sm" href="/Admin/Clients">Cliente 360</a></td></tr>`).join('');
  }
  window.addEventListener('barbersync:store-changed', render);
  window.BarberSyncEventBus?.on('*', render);
  document.addEventListener('DOMContentLoaded', render);
})();
