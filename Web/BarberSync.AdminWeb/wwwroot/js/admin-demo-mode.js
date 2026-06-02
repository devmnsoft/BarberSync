(() => {
  const MODE_KEY = 'BarberSync:AdminDemoMode';
  const getStore = () => window.BarberSyncDemoStore;
  const toast = (message, type = 'success') => window.AdminToast?.show ? window.AdminToast.show(message, type) : console.log(message);
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[ch]));
  const moduleFromPath = () => (location.pathname.split('/').filter(Boolean)[1] || 'Dashboard');
  const formatMoney = value => Number(value || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

  function setMode(mode) {
    const normalized = ['commercial', 'real', 'hybrid'].includes(mode) ? mode : 'hybrid';
    localStorage.setItem(MODE_KEY, normalized);
    document.body.dataset.demoMode = normalized;
    document.querySelectorAll('[data-demo-current-mode]').forEach(el => { el.textContent = normalized === 'commercial' ? 'Demo Comercial' : normalized === 'real' ? 'API Real' : 'Híbrido'; });
    document.querySelectorAll('[data-demo-mode-select]').forEach(el => { el.value = normalized; });
  }

  function addDemoBadges() {
    document.querySelectorAll('.panel-header, .page-title, .module-panel h3').forEach((el, index) => {
      if (index < 10 && !el.querySelector?.('.demo-badge')) el.insertAdjacentHTML('beforeend', '<span class="demo-badge">Demo</span>');
    });
  }

  function renderDashboardExperience() {
    if (document.querySelector('[data-demo-dashboard-20]')) return;
    const dashboard = getStore()?.get('dashboard') || {};
    const k = dashboard.kpis || {};
    const flow = dashboard.flow || {};
    const content = document.querySelector('.admin-content');
    if (!content) return;
    content.insertAdjacentHTML('afterbegin', `
      <section class="demo-experience-panel" data-demo-dashboard-20 data-tour-target="dashboard">
        <div class="panel-header"><div><h2>BarberSync Demo Experience 2.0</h2><p>Fluxo vivo da agenda ao pagamento, com estoque, campanhas, totem, PublicWeb e Copilot conectados.</p></div><span class="badge badge-success">Operação guiada</span></div>
        <div class="demo-experience-grid" data-tour-target="kpis">
          <article class="demo-insight-card" title="Receita acumulada no cenário demo"><span>💰 Receita</span><strong>${formatMoney(k.revenue)}</strong><small>Ticket e PDV sincronizados</small></article>
          <article class="demo-insight-card" title="Percentual de horários ocupados"><span>📅 Ocupação</span><strong>${k.occupancy || 0}%</strong><small>Agenda por profissional</small></article>
          <article class="demo-insight-card" title="Clientes que retornam"><span>🔁 Recorrência</span><strong>${k.recurrence || 0}%</strong><small>Campanhas + cashback</small></article>
          <article class="demo-insight-card" title="Avaliações e NPS"><span>⭐ Satisfação</span><strong>${k.satisfaction || 0}%</strong><small>NPS e reputação</small></article>
          <article class="demo-insight-card" title="Saúde do estoque"><span>📦 Estoque</span><strong>${k.stockHealth || 0}%</strong><small>Reposição inteligente</small></article>
          <article class="demo-insight-card" title="Conversão do site público"><span>🌐 PublicWeb</span><strong>${k.siteConversion || 0}%</strong><small>Agendamento online</small></article>
        </div>
      </section>
      <section class="demo-experience-panel" data-tour-target="agenda">
        <div class="panel-header"><h3>Fluxo de Hoje</h3><span class="badge badge-info">Agenda → Caixa</span></div>
        <div class="demo-flow"><span>Agendado ${flow.scheduled || 0}</span>→<span>Check-in ${flow.checkin || 0}</span>→<span>Atendimento ${flow.service || 0}</span>→<span>Comanda ${flow.orders || 0}</span>→<span>Pagamento ${flow.paid || 0}</span>→<span>Avaliação ${flow.reviews || 0}</span></div>
      </section>
      <section class="demo-experience-panel"><div class="demo-experience-grid">
        <article><h3>Alertas Prioritários</h3><ul class="metric-list">${(dashboard.alerts || []).map(x => `<li>⚠️ ${escapeHtml(x)}</li>`).join('')}</ul></article>
        <article><h3>Oportunidades</h3><ul class="metric-list">${(dashboard.opportunities || []).map(x => `<li>💡 ${escapeHtml(x)}</li>`).join('')}</ul></article>
        <article><h3>Resumo da Operação</h3><ul class="metric-list"><li>Agenda, atendimento e comandas conectados</li><li>Caixa com PIX, cartão, dinheiro e misto</li><li>Totem reduz fila e PublicWeb converte agendamentos</li><li>Copilot recomenda próximas ações</li></ul></article>
      </div></section>`);
  }

  function renderModuleExperience() {
    const module = moduleFromPath();
    if (module === 'Dashboard' || module === 'Index') return renderDashboardExperience();
    if (document.querySelector('[data-demo-module-20]')) return;
    const content = document.querySelector('.admin-content');
    if (!content) return;
    const data = Array.isArray(getStore()?.get(module)) ? getStore().get(module) : [];
    const configs = {
      Clients: ['Cliente 360', 'Segmentação VIP, recorrente, inativo, novo, aniversariante e cashback.', ['Agendar', 'Abrir comanda', 'Enviar campanha', 'Adicionar observação', 'Ver histórico']],
      Professionals: ['Performance de profissionais', 'Ranking, comissão estimada, meta, agenda do dia e serviços mais executados.', ['Ver agenda', 'Ver comissão', 'Vincular serviço', 'Criar bloqueio']],
      Services: ['Catálogo omnichannel', 'Cards/tabela, filtros por categoria e canais PublicWeb, Totem e Mobile.', ['Alternar cards', 'Prévia PublicWeb', 'Prévia Totem', 'Toggle canal']],
      Appointments: ['Operação do dia', 'Timeline por profissional, lista de espera, bloqueios e mudança visual de status.', ['Confirmar', 'Check-in', 'Iniciar', 'Finalizar', 'No-show']],
      ServiceOrders: ['PDV visual', 'Kanban, descontos, cupom, cashback, recibo, impressão e fechamento.', ['Adicionar serviço', 'Adicionar produto', 'Aplicar cupom', 'Registrar pagamento', 'Imprimir recibo']],
      Stock: ['Reposição inteligente', 'Valor em estoque, produtos críticos, giro, sugestão de compra e histórico.', ['Repor produto', 'Registrar baixa', 'Ver histórico', 'Sugestão de compra']],
      Campaigns: ['Campanhas comerciais', 'Funil, público-alvo, período, resultado, impacto e receita estimada.', ['Criar campanha', 'Duplicar', 'Pausar', 'Ver clientes']],
      Coupons: ['Cupons e conversão', 'Código copiável, validade, limite, uso e status.', ['Copiar código', 'Pausar cupom', 'Ver uso']],
      Loyalty: ['Fidelidade e cashback', 'Saldo total, clientes com saldo, expiração, extrato e regras.', ['Ver extrato', 'Criar regra', 'Notificar expiração']],
      Reviews: ['Avaliações e NPS', 'Nota média, distribuição, promotores, detratores e recuperação.', ['Responder cliente', 'Criar ação de recuperação']],
      Copilot: ['Copilot 2.0', 'Chat visual com sugestões por vendas, estoque, agenda, clientes e financeiro.', ['Criar campanha', 'Ver clientes', 'Abrir estoque', 'Abrir agenda']],
      Cash: ['Caixa operacional', 'Abertura, entradas, saídas, sangria, suprimento, fechamento e divergência.', ['Abrir caixa', 'Sangria', 'Suprimento', 'Fechar caixa']],
      Payments: ['Pagamentos', 'Lista, formas, status e recibo visual.', ['Ver recibo', 'Reprocessar demo', 'Exportar']],
      Financial: ['Financeiro', 'Receita, ticket médio, descontos, cashback usado, comissões e fluxo simples.', ['Ver fluxo', 'Exportar', 'Projetar semana']]
    };
    const cfg = configs[module];
    if (!cfg) return;
    content.insertAdjacentHTML('afterbegin', `<section class="demo-experience-panel" data-demo-module-20 data-tour-target="${module.toLowerCase()}"><div class="panel-header"><div><h2>${cfg[0]} <span class="demo-badge">Demo</span></h2><p>${cfg[1]}</p></div><span class="badge badge-success">${data.length || 'Dados'} registros</span></div><div class="demo-action-row">${cfg[2].map(a => `<button type="button" class="btn btn-light" data-demo-smart-action="${escapeHtml(a)}">${escapeHtml(a)}</button>`).join('')}</div><div class="demo-experience-grid" style="margin-top:1rem">${data.slice(0,3).map(item => `<article class="demo-insight-card"><span>${escapeHtml(item.segment || item.category || item.status || item.priority || 'Demo')}</span><strong>${escapeHtml(item.name || item.client || item.code || item.summary || item.product || item.id)}</strong><small>${escapeHtml(item.nextBestAction || item.suggestion || item.action || item.service || item.specialty || item.result || 'Ação demonstrável')}</small></article>`).join('')}</div></section>`);
  }

  function openClientDrawer(item) {
    const client = item || getStore()?.get('clients')?.[0];
    if (!client) return;
    let drawer = document.querySelector('[data-client-360-drawer]');
    if (!drawer) { document.body.insertAdjacentHTML('beforeend', '<aside class="demo-drawer" data-client-360-drawer hidden></aside>'); drawer = document.querySelector('[data-client-360-drawer]'); }
    drawer.innerHTML = `<button class="modal-close" type="button" data-close-drawer>×</button><h2>Cliente 360</h2><h3>${escapeHtml(client.name)}</h3><p><span class="badge badge-info">${escapeHtml(client.segment || client.status)}</span> <span class="badge badge-success">Score ${client.score || 86}</span></p><p><strong>Próxima melhor ação:</strong> ${escapeHtml(client.nextBestAction || 'Agendar retorno')}</p><p><strong>Cashback:</strong> ${formatMoney(client.cashback || 0)}</p><div class="demo-timeline"><div>Cadastro validado</div><div>Agendamento pelo PublicWeb</div><div>Check-in no Totem</div><div>Comanda paga</div><div>Avaliação/NPS registrada</div><div>Campanha de retorno enviada</div></div><div class="demo-action-row"><a class="btn btn-light" href="/Admin/Appointments">Agendar</a><a class="btn btn-light" href="/Admin/ServiceOrders">Abrir comanda</a><a class="btn btn-primary" href="/Admin/Campaigns">Enviar campanha</a></div>`;
    drawer.hidden = false;
  }

  function handleAction(text) {
    const module = moduleFromPath();
    if (module === 'Clients' && /histórico|observação|Cliente 360/i.test(text)) openClientDrawer();
    if (/pagamento/i.test(text)) {
      const order = getStore()?.get('serviceOrders')?.[0];
      if (order) getStore().patch('serviceOrders', order.id, { status: 'Paga', paidAt: new Date().toISOString() });
    }
    if (/recibo|imprimir/i.test(text)) {
      const receipt = 'BarberSync Demo\nComanda paga\nPIX/Cartão/Dinheiro/Misto\nObrigado pela preferência!';
      toast(receipt, 'success');
    } else toast(`${text} executado em modo demo.`, 'success');
  }

  document.addEventListener('click', event => {
    const target = event.target.closest('[data-demo-reset],[data-demo-scenario],[data-demo-smart-action],[data-close-drawer]');
    if (!target) return;
    if (target.matches('[data-demo-reset]')) { getStore()?.resetAll(); toast('Dados demo resetados.'); location.reload(); }
    if (target.dataset.demoScenario) { getStore()?.loadScenario(target.dataset.demoScenario); toast('Cenário demo carregado.'); location.reload(); }
    if (target.dataset.demoSmartAction) handleAction(target.dataset.demoSmartAction);
    if (target.matches('[data-close-drawer]')) target.closest('[data-client-360-drawer]').hidden = true;
  });
  document.addEventListener('change', event => { if (event.target.matches('[data-demo-mode-select]')) setMode(event.target.value); });
  document.addEventListener('DOMContentLoaded', () => { setMode(localStorage.getItem(MODE_KEY) || 'hybrid'); addDemoBadges(); renderDashboardExperience(); renderModuleExperience(); });
  window.BarberSyncDemoMode = { setMode, getMode: () => localStorage.getItem(MODE_KEY) || 'hybrid', openClientDrawer, handleAction };
})();
