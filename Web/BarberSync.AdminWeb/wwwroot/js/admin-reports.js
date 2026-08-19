(() => {
  'use strict';
  const $ = selector => document.querySelector(selector);
  const labels = { revenueToday:'Faturamento hoje',revenueMonth:'Faturamento do mês',averageTicket:'Ticket médio',newClients:'Clientes novos',recurringClients:'Clientes recorrentes',pendingCommissions:'Comissões pendentes',purchasesAwaitingReceipt:'Compras a receber',overduePayables:'Contas vencidas',packagesSold:'Pacotes vendidos',activeSubscriptions:'Assinaturas ativas',recurringRevenue:'Receita recorrente',criticalStock:'Estoque crítico',noShow:'No-show',cashDifference:'Divergência de caixa',todayAppointments:'Agenda de hoje',waiting:'Aguardando',late:'Atrasados',kioskCheckins:'Check-ins Totem',openOrders:'Pré-comandas abertas',expiringPackages:'Pacotes vencendo',expiredSubscriptions:'Assinaturas vencidas',inactiveClients:'Clientes inativos' };
  const currency = new Set(['revenueToday','revenueMonth','averageTicket','pendingCommissions','recurringRevenue','cashDifference']);
  const format = (key,value) => currency.has(key) ? Number(value).toLocaleString('pt-BR',{style:'currency',currency:'BRL'}) : Number(value).toLocaleString('pt-BR');
  const escape = value => String(value ?? '').replace(/[&<>'"]/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[char]));
  async function load(event) {
    event?.preventDefault(); const view = $('[name="view"]')?.value || 'owner';
    const query = new URLSearchParams(new FormData($('[data-report-filters]'))); query.delete('view');
    $('[data-report-loading]').hidden=false; $('[data-report-error]').hidden=true; $('[data-report-kpis]').hidden=true; $('[data-report-content]').hidden=true;
    try {
      const response=await fetch(`/AdminApi/executive/${view}?${query}`,{headers:{Accept:'application/json'}}); const payload=await response.json().catch(()=>null);
      if(!response.ok || !payload?.success) throw Object.assign(new Error(payload?.detail||payload?.title||'A API não retornou dados.'),{traceId:payload?.traceId});
      const rows=Object.entries(payload.data.metrics||{}); const max=Math.max(...rows.map(([,v])=>Number(v)),1);
      $('[data-report-kpis]').innerHTML=rows.map(([key,value])=>`<article class="premium-kpi ${['overduePayables','criticalStock','late','cashDifference'].includes(key)&&Number(value)>0?'critical':''}"><span class="kpi-name">${escape(labels[key]||key)}</span><strong>${escape(format(key,value))}</strong><small>Dado confirmado na unidade atual</small></article>`).join('');
      $('[data-report-chart]').innerHTML=rows.filter(([,v])=>Number(v)>0).slice(0,12).map(([key,value])=>`<div class="chart-row"><span>${escape(labels[key]||key)}</span><i><b style="width:${Math.max(3,Number(value)/max*100)}%"></b></i><strong>${escape(format(key,value))}</strong></div>`).join('')||'<div class="dashboard-empty"><strong>Sem dados no período.</strong><p>Altere os filtros ou aguarde novos lançamentos.</p></div>';
      $('[data-report-table]').innerHTML=rows.map(([key,value])=>`<tr><td>${escape(labels[key]||key)}</td><td>${escape(format(key,value))}</td><td>Tenant e unidade autenticados</td></tr>`).join('');
      $('[data-report-kpis]').hidden=false; $('[data-report-content]').hidden=false;
    } catch(error) { $('[data-report-error]').hidden=false; $('[data-report-error-message]').textContent=error.message; $('[data-report-trace]').textContent=`traceId: ${error.traceId||'não informado pela API'}`; }
    finally { $('[data-report-loading]').hidden=true; }
  }
  $('[data-report-filters]')?.addEventListener('submit',load); $('[data-report-retry]')?.addEventListener('click',load); $('[data-report-print]')?.addEventListener('click',()=>window.print()); document.addEventListener('DOMContentLoaded',load);
  $('[data-report-export]')?.addEventListener('click', () => {
    const query = new URLSearchParams(new FormData($('[data-report-filters]'))); query.delete('view');
    window.location.assign(`/AdminApi/executive/export.csv?${query}`);
  });
})();
