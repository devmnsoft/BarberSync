(() => {
  const endpointMap = {
    Clients: '/AdminApi/clients', Professionals: '/AdminApi/professionals', Services: '/AdminApi/services',
    Appointments: '/AdminApi/appointments', ServiceOrders: '/AdminApi/service-orders', Stock: '/AdminApi/products',
    Campaigns: '/AdminApi/campaigns', Coupons: '/AdminApi/coupons', Loyalty: '/AdminApi/loyalty',
    Reviews: '/AdminApi/reviews', Copilot: '/AdminApi/copilot-suggestions'
  };

  const moduleCopy = {
    Clients: { icon: '👥', singular: 'Cliente', detail: 'Base 360º com preferências, consentimentos e fidelidade.' },
    Professionals: { icon: '💈', singular: 'Profissional', detail: 'Escala, comissão, serviços habilitados e performance.' },
    Services: { icon: '✂️', singular: 'Serviço', detail: 'Catálogo multicanal para site, totem e mobile.' },
    Appointments: { icon: '📅', singular: 'Agendamento', detail: 'Agenda do dia com status operacional.' },
    ServiceOrders: { icon: '🧾', singular: 'Comanda', detail: 'Kanban de abertura, pagamento e fechamento.' },
    Stock: { icon: '📦', singular: 'Movimento de estoque', detail: 'Produtos, mínimos e reposição crítica.' },
    Campaigns: { icon: '🎯', singular: 'Campanha', detail: 'CRM com período, público e resultado.' },
    Coupons: { icon: '🏷️', singular: 'Cupom', detail: 'Cupons promocionais para recorrência.' },
    Loyalty: { icon: '💎', singular: 'Fidelidade', detail: 'Cashback, pontos e clube de benefícios.' },
    Reviews: { icon: '⭐', singular: 'Avaliação', detail: 'Reputação, NPS e comentários.' },
    Copilot: { icon: '🤖', singular: 'Sugestão', detail: 'Copilot comercial com ações recomendadas.' }
  };

  const formTemplates = {
    Clients: [
      ['name', 'Nome', 'text', true], ['personType', 'Tipo PF/PJ', 'select:PF|PJ', true], ['document', 'CPF/CNPJ', 'text', true],
      ['phone', 'Telefone', 'tel', true], ['whatsapp', 'WhatsApp', 'tel', false], ['email', 'E-mail', 'email', true],
      ['birthDate', 'Data nascimento', 'date', false], ['address', 'Endereço', 'text', false],
      ['preferredProfessional', 'Profissional preferido', 'text', false], ['preferredService', 'Serviço preferido', 'text', false],
      ['acceptsPromotions', 'Aceita promoções', 'select:Sim|Não', false], ['vip', 'VIP', 'select:Não|Sim', false], ['status', 'Status', 'select:Ativo|VIP|Inativo', true]
    ],
    Professionals: [
      ['name', 'Nome', 'text', true], ['phone', 'Telefone', 'tel', true], ['email', 'E-mail', 'email', true],
      ['specialty', 'Especialidade', 'text', true], ['commission', 'Comissão (%)', 'number', true], ['status', 'Status', 'select:Disponível|Em atendimento|Folga|Inativo', true],
      ['workDays', 'Dias de trabalho', 'text', true], ['hours', 'Horários', 'text', true], ['services', 'Serviços', 'text', true]
    ],
    Services: [
      ['name', 'Nome', 'text', true], ['category', 'Categoria', 'text', true], ['description', 'Descrição', 'textarea', true],
      ['price', 'Preço', 'number', true], ['durationMinutes', 'Duração (min)', 'number', true], ['commission', 'Comissão (%)', 'number', false],
      ['site', 'Exibir no site', 'select:Sim|Não', false], ['kiosk', 'Exibir no totem', 'select:Sim|Não', false], ['mobile', 'Exibir no mobile', 'select:Sim|Não', false], ['status', 'Status', 'select:Ativo|Inativo', true]
    ],
    Appointments: [
      ['client', 'Cliente', 'text', true], ['service', 'Serviço', 'text', true], ['professional', 'Profissional', 'text', true],
      ['date', 'Data', 'date', true], ['time', 'Hora', 'time', true], ['notes', 'Observação', 'textarea', false]
    ],
    ServiceOrders: [
      ['client', 'Cliente', 'text', true], ['service', 'Serviço', 'text', true], ['professional', 'Profissional', 'text', true],
      ['items', 'Itens/Produtos', 'textarea', false], ['discount', 'Desconto', 'number', false], ['paymentMethod', 'Pagamento', 'select:PIX|Cartão|Dinheiro|Carteira', true]
    ],
    Stock: [
      ['product', 'Produto', 'text', true], ['movement', 'Tipo', 'select:Entrada|Saída|Ajuste', true], ['quantity', 'Quantidade', 'number', true],
      ['cost', 'Custo unitário', 'number', false], ['supplier', 'Fornecedor', 'text', false], ['notes', 'Observação', 'textarea', false]
    ],
    Campaigns: [['name', 'Nome', 'text', true], ['period', 'Período', 'text', true], ['audience', 'Público', 'text', true], ['channel', 'Canal', 'select:WhatsApp|SMS|Instagram|E-mail', true], ['goal', 'Meta', 'text', false]],
    Coupons: [['code', 'Código', 'text', true], ['discount', 'Valor/desconto', 'text', true], ['validUntil', 'Validade', 'date', true], ['status', 'Status', 'select:Ativo|Pausado|Expirado', true]],
    Copilot: [['question', 'Pergunta para o Copilot', 'textarea', true], ['context', 'Contexto', 'textarea', false]]
  };

  const writeToast = (msg, type = 'success') => window.AdminToast?.show ? window.AdminToast.show(msg, type) : alert(msg);
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[ch]));
  const normalizeList = data => Array.isArray(data) ? data : (Array.isArray(data?.items) ? data.items : (Array.isArray(data?.data) ? data.data : (data ? [data] : [])));
  const detail = i => i.service || i.specialty || i.category || i.channel || i.comment || i.code || i.phone || i.description || i.client || i.priority || i.status || 'Operação BarberSync';
  const status = i => i.status || (i.isDemo ? 'Demo' : 'Ativo');
  const name = i => i.name || i.fullName || i.title || i.client || i.code || i.label || i.product || 'Registro demo';
  const money = value => Number(value ?? 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

  function fieldHtml([key, label, type, required]) {
    const req = required ? 'required' : '';
    if (type === 'textarea') return `<label>${label}<textarea name="${key}" ${req} rows="3" placeholder="${label}"></textarea></label>`;
    if (type.startsWith('select:')) {
      return `<label>${label}<select name="${key}" ${req}>${type.substring(7).split('|').map(o => `<option>${o}</option>`).join('')}</select></label>`;
    }
    const step = type === 'number' ? ' step="0.01" min="0"' : '';
    return `<label>${label}<input name="${key}" type="${type}" ${req}${step} placeholder="${label}" /></label>`;
  }

  function hydrateModal(module) {
    const modal = document.getElementById(`${module}Modal`);
    const form = modal?.querySelector(`[data-admin-form='${module}']`);
    if (!modal || !form || form.dataset.rich === 'true') return;
    const copy = moduleCopy[module] || { singular: module, icon: '💈' };
    const fields = formTemplates[module] || [['name', 'Nome', 'text', true], ['status', 'Status', 'select:Ativo|Pendente|Inativo', true], ['notes', 'Observação', 'textarea', false]];
    form.dataset.rich = 'true';
    form.innerHTML = `
      <div class="modal-header"><div><span class="modal-kicker">${copy.icon} ${copy.singular}</span><h3 id="${module}ModalTitle">Novo ${copy.singular}</h3></div><button type="button" class="modal-close" data-admin-close="${module}" aria-label="Fechar">×</button></div>
      <div class="form-grid form-grid-2">${fields.map(fieldHtml).join('')}</div>
      <div class="form-validation" data-form-error="${module}" hidden>Preencha os campos obrigatórios para continuar.</div>
      <div class="modal-actions"><button type="button" class="btn btn-light" data-admin-close="${module}">Cancelar</button><button class="btn btn-primary" type="submit">Salvar ${copy.singular}</button></div>`;
  }

  function ensureWorkspace(module, panel) {
    const copy = moduleCopy[module] || { detail: 'Operação BarberSync' };
    if (!document.getElementById(`${module}Toolbar`)) {
      panel.insertAdjacentHTML('afterbegin', `<div class="module-toolbar" id="${module}Toolbar"><input class="form-control" id="${module}Search" placeholder="Buscar em ${module}..." /><select class="form-control" id="${module}Status"><option value="">Todos os status</option><option>Ativo</option><option>Disponível</option><option>Confirmado</option><option>Crítico</option><option>Demo</option></select><button class="btn btn-light" data-admin-export="${module}">Exportar visão</button></div>`);
    }
    if (!document.getElementById(`${module}Insights`)) {
      panel.insertAdjacentHTML('beforebegin', `<section class="panel module-insights" id="${module}Insights"><div><strong>Fluxo visual completo</strong><p>${copy.detail}</p></div><div class="status-flow"><span>Listar</span><span>Criar</span><span>Editar</span><span>Detalhar</span><span>Excluir</span></div></section>`);
    }
    if (!document.getElementById(`${module}DetailModal`)) {
      document.body.insertAdjacentHTML('beforeend', `<div class="admin-modal-backdrop" id="${module}DetailModal" hidden><div class="admin-modal"><div class="modal-header"><h3>Detalhes</h3><button type="button" class="modal-close" data-detail-close="${module}">×</button></div><div id="${module}DetailBody" class="detail-grid"></div><div class="modal-actions"><button type="button" class="btn btn-light" data-detail-close="${module}">Fechar</button></div></div></div>`);
      document.querySelectorAll(`[data-detail-close='${module}']`).forEach(b => b.addEventListener('click', () => document.getElementById(`${module}DetailModal`).hidden = true));
    }
  }

  function decorateRows(module, list, rowsEl) {
    if (module === 'ServiceOrders') {
      const by = label => list.filter(i => status(i).toLowerCase().includes(label));
      rowsEl.closest('.table-responsive').insertAdjacentHTML('beforebegin', `<div class="kanban-board" id="${module}Kanban"><div><h4>Abertas</h4>${by('abert').map(card).join('') || card(list[0])}</div><div><h4>Em pagamento</h4>${by('pag').map(card).join('') || card(list[1])}</div><div><h4>Fechadas</h4>${by('fech').map(card).join('') || card({ name: 'Recibo visual demo', total: 95, status: 'Fechada' })}</div></div>`);
    }
    function card(i = {}) { return `<article class="kanban-card"><strong>${escapeHtml(name(i))}</strong><span>${escapeHtml(detail(i))}</span><b>${money(i.total || i.price || 0)}</b></article>`; }
  }

  function actionButtons(module, i, idx) {
    const id = escapeHtml(i.id || idx);
    const base = `<button class='btn btn-secondary' data-admin-detail='${module}' data-index='${idx}'>Detalhe</button> <button class='btn btn-secondary' data-admin-edit='${module}' data-id='${id}'>Editar</button> <button class='btn btn-danger' data-admin-remove='${module}' data-id='${id}'>Excluir</button>`;
    if (module === 'Appointments') return `${base}<div class='row-actions'><button data-appointment-action='Confirmar'>Confirmar</button><button data-appointment-action='Check-in'>Check-in</button><button data-appointment-action='Iniciar'>Iniciar</button><button data-appointment-action='Finalizar'>Finalizar</button><button data-appointment-action='Cancelar'>Cancelar</button></div>`;
    if (module === 'ServiceOrders') return `${base}<div class='row-actions'><button data-order-action='pay'>Pagar</button><button data-order-action='close'>Fechar</button></div>`;
    return base;
  }

  window.renderAdminCrudPage = async function (module) {
    const endpoint = endpointMap[module] || '/AdminApi/dashboard';
    const copy = moduleCopy[module] || { icon: '💈', singular: module };
    const demo = [
      { id: 'demo-1', name: `${copy.singular} demo 1`, status: 'Ativo', category: 'Demonstração', total: 70 },
      { id: 'demo-2', name: `${copy.singular} demo 2`, status: 'Pendente', category: 'Operação', total: 95 },
      { id: 'demo-3', name: `${copy.singular} demo 3`, status: 'Ativo', category: 'SaaS', total: 130 }
    ];
    hydrateModal(module);
    const panel = document.querySelector('.module-panel');
    if (panel) ensureWorkspace(module, panel);
    let currentList = demo;

    const load = async () => {
      const { data, fallback } = await adminApiClient.get(endpoint, demo);
      if (fallback) document.getElementById(`${module}Fallback`)?.classList.remove('d-none');
      const list = normalizeList(data).length ? normalizeList(data) : demo;
      currentList = list;
      const activeCount = list.filter(x => /ativo|dispon|confirm|ok/i.test(status(x))).length || list.length;
      const revenue = list.reduce((acc, x) => acc + Number(x.total || x.price || x.revenueMonth || 0), 0);
      document.getElementById(`${module}Cards`).innerHTML = `<article class='kpi-card'><div class='kpi-icon'>${copy.icon}</div><div><p class='kpi-label'>Total</p><strong class='kpi-value'>${list.length}</strong><span class='kpi-variation'>registros</span></div></article><article class='kpi-card'><div class='kpi-icon'>✅</div><div><p class='kpi-label'>Ativos</p><strong class='kpi-value'>${activeCount}</strong><span class='kpi-variation'>em operação</span></div></article><article class='kpi-card'><div class='kpi-icon'>💰</div><div><p class='kpi-label'>Potencial</p><strong class='kpi-value'>${money(revenue)}</strong><span class='kpi-variation'>amostra</span></div></article><article class='kpi-card'><div class='kpi-icon'>🛡️</div><div><p class='kpi-label'>Origem</p><strong class='kpi-value'>${fallback ? 'Demo' : 'API'}</strong><span class='kpi-variation'>proxy MVC</span></div></article>`;
      const term = (document.getElementById(`${module}Search`)?.value || '').toLowerCase();
      const filtered = list.filter(i => JSON.stringify(i).toLowerCase().includes(term));
      document.getElementById(`${module}Rows`).innerHTML = filtered.slice(0, 16).map((i, idx) => `<tr><td><strong>${escapeHtml(name(i))}</strong><small>${escapeHtml(i.email || i.phone || i.time || i.code || '')}</small></td><td>${escapeHtml(detail(i))}</td><td><span class='badge badge-success'>${escapeHtml(status(i))}</span></td><td>${actionButtons(module, i, idx)}</td></tr>`).join('') || `<tr><td colspan="4"><div class="empty-state-mini">Nenhum registro encontrado. Use Novo ${copy.singular} para demonstrar o fluxo.</div></td></tr>`;
      document.getElementById(`${module}Kanban`)?.remove();
      decorateRows(module, filtered, document.getElementById(`${module}Rows`));
    };

    await load();
    document.getElementById(`${module}Search`)?.addEventListener('input', load);
    document.querySelector(`[data-admin-export='${module}']`)?.addEventListener('click', () => writeToast('Visão exportada em modo demonstração.', 'info'));
    document.querySelector(`[data-admin-new='${module}']`)?.addEventListener('click', () => { document.getElementById(`${module}ModalTitle`).textContent = `Novo ${copy.singular}`; document.getElementById(`${module}Modal`).hidden = false; });
    document.querySelector(`[data-admin-refresh='${module}']`)?.addEventListener('click', load);
    document.querySelectorAll(`[data-admin-close='${module}']`).forEach(b => b.addEventListener('click', () => document.getElementById(`${module}Modal`).hidden = true));
    document.querySelector(`[data-admin-form='${module}']`)?.addEventListener('submit', async (e) => {
      e.preventDefault();
      if (!e.target.checkValidity()) { document.querySelector(`[data-form-error='${module}']`).hidden = false; return; }
      const body = Object.fromEntries(new FormData(e.target).entries());
      await adminApiClient.post(module === 'ServiceOrders' ? '/AdminApi/service-orders/open' : endpoint, body, { success: true, message: 'Salvo em modo demonstração.' });
      document.getElementById(`${module}Modal`).hidden = true;
      e.target.reset();
      writeToast(`${copy.singular} salvo com sucesso.`);
      await load();
    });
    document.getElementById(`${module}Rows`)?.addEventListener('click', async (e) => {
      const edit = e.target.closest('[data-admin-edit]');
      const remove = e.target.closest('[data-admin-remove]');
      const detailBtn = e.target.closest('[data-admin-detail]');
      const apptAction = e.target.closest('[data-appointment-action]');
      const orderAction = e.target.closest('[data-order-action]');
      if (detailBtn) {
        const item = currentList[Number(detailBtn.dataset.index)] || {};
        document.getElementById(`${module}DetailBody`).innerHTML = Object.entries(item).slice(0, 14).map(([k, v]) => `<div><span>${escapeHtml(k)}</span><strong>${escapeHtml(typeof v === 'object' ? JSON.stringify(v) : v)}</strong></div>`).join('');
        document.getElementById(`${module}DetailModal`).hidden = false;
      }
      if (edit) { document.getElementById(`${module}ModalTitle`).textContent = `Editar ${copy.singular}`; document.getElementById(`${module}Modal`).hidden = false; }
      if (remove && confirm(`Excluir ${copy.singular}?`)) { await adminApiClient.delete(`${endpoint}/${remove.dataset.id}`, { success: true }); writeToast(`${copy.singular} excluído em modo demonstração.`); await load(); }
      if (apptAction) writeToast(`Agendamento: ${apptAction.dataset.appointmentAction}.`, 'info');
      if (orderAction) { await adminApiClient.post(`/AdminApi/service-orders/${orderAction.closest('[data-admin-remove],[data-admin-edit]')?.dataset.id || 'demo'}/${orderAction.dataset.orderAction}`, {}, { success: true }); writeToast(orderAction.dataset.orderAction === 'pay' ? 'Pagamento mock aprovado.' : 'Comanda fechada com recibo visual.', 'success'); }
    });
  };
})();
