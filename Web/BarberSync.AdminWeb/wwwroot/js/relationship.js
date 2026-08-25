(() => {
  'use strict';
  const root = document.querySelector('[data-relationship-root]');
  if (!root) return;
  const api = async path => {
    const response = await fetch(`/AdminApi${path}`, { credentials: 'same-origin', headers: { Accept: 'application/json' } });
    const body = await response.json().catch(() => ({}));
    if (!response.ok || body.success === false) throw Object.assign(new Error(body.message || 'Falha na consulta.'), { traceId: body.traceId || response.headers.get('X-Trace-Id') });
    return body.data;
  };
  const labels = { activeClients: 'Clientes ativos', recurringClients: 'Recorrentes', inactiveClients: 'Inativos', birthdaysThisMonth: 'Aniversários', churnRiskClients: 'Risco de churn', revenuePerClient: 'Receita / cliente', averageTicket: 'Ticket médio', packagesSold: 'Pacotes vendidos', couponsUsed: 'Cupons usados', pointsBalance: 'Pontos em conta', cashbackBalance: 'Cashback disponível' };
  const money = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
  const escape = value => String(value ?? '').replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[char]));
  async function load() {
    const loading = root.querySelector('[data-relationship-loading]'); const error = root.querySelector('[data-relationship-error]'); const kpis = root.querySelector('[data-relationship-kpis]');
    loading.hidden = false; error.hidden = true; kpis.hidden = true;
    try {
      const [dashboard, segments] = await Promise.all([api('/relationship/dashboard'), api('/relationship/segments')]);
      kpis.innerHTML = Object.entries(labels).map(([key, label]) => `<article><span>${label}</span><strong>${key.includes('revenue') || key === 'averageTicket' || key === 'cashbackBalance' ? money.format(dashboard[key] || 0) : escape(dashboard[key] || 0)}</strong></article>`).join('');
      root.querySelector('[data-relationship-segments]').innerHTML = segments.map(item => `<a href="/Relationship/Clients?segment=${encodeURIComponent(item.key)}"><strong>${escape(item.name)}</strong><span>Ver clientes →</span></a>`).join('') || '<p>Nenhum segmento disponível.</p>';
      root.querySelector('[data-relationship-campaigns]').innerHTML = (dashboard.recentCampaigns || []).map(item => `<div><strong>${escape(item.name || item.title)}</strong><span>${escape(item.status)}</span></div>`).join('') || '<p>Nenhuma campanha interna criada.</p>';
      kpis.hidden = false;
    } catch (failure) { error.querySelector('[data-error-message]').textContent = failure.message; error.querySelector('[data-error-trace]').textContent = failure.traceId || 'não informado'; error.hidden = false; }
    finally { loading.hidden = true; }
  }
  root.querySelector('[data-relationship-refresh]').addEventListener('click', load); load();
})();
