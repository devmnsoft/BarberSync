(() => {
  const endpointMap = {
    Clients: '/AdminApi/clients', Professionals: '/AdminApi/professionals', Services: '/AdminApi/services',
    Appointments: '/AdminApi/appointments', ServiceOrders: '/AdminApi/service-orders', Stock: '/AdminApi/products', Products: '/AdminApi/products',
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
    Products: { icon: '🧴', singular: 'Produto', detail: 'Catálogo de produtos com venda e estoque.' },
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
      ['birthDate', 'Data nascimento', 'date', false], ['zipCode', 'CEP', 'text', false], ['street', 'Rua', 'text', false], ['number', 'Número', 'text', false], ['district', 'Bairro', 'text', false], ['city', 'Cidade', 'text', false], ['state', 'Estado', 'text', false],
      ['preferredProfessional', 'Profissional preferido', 'text', false], ['preferredService', 'Serviço preferido', 'text', false],
      ['acceptsPromotions', 'Aceita promoções', 'select:Sim|Não', false], ['vip', 'VIP', 'select:Não|Sim', false], ['status', 'Status', 'select:Ativo|VIP|Inativo', true]
    ],
    Professionals: [
      ['name', 'Nome', 'text', true], ['phone', 'Telefone', 'tel', true], ['email', 'E-mail', 'email', true],
      ['specialty', 'Especialidade', 'text', true], ['bio', 'Bio', 'textarea', false], ['commission', 'Comissão (%)', 'number', true], ['status', 'Status', 'select:Disponível|Em atendimento|Folga|Inativo', true],
      ['workDays', 'Dias de trabalho', 'text', true], ['startTime', 'Horário inicial', 'time', true], ['endTime', 'Horário final', 'time', true], ['services', 'Serviços', 'text', true], ['monthlyGoal', 'Meta mensal', 'number', false]
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
    Products: [
      ['name', 'Nome', 'text', true], ['category', 'Categoria', 'text', true], ['sku', 'SKU', 'text', false],
      ['salePrice', 'Preço de venda', 'number', true], ['costPrice', 'Custo', 'number', false], ['currentStock', 'Estoque atual', 'number', true], ['minStock', 'Estoque mínimo', 'number', true], ['status', 'Status', 'select:Ativo|Inativo', true]
    ],
    Campaigns: [['name', 'Nome', 'text', true], ['period', 'Período', 'text', true], ['audience', 'Público', 'text', true], ['channel', 'Canal', 'select:WhatsApp|SMS|Instagram|E-mail', true], ['goal', 'Meta', 'text', false]],
    Coupons: [['code', 'Código', 'text', true], ['discount', 'Valor/desconto', 'text', true], ['validUntil', 'Validade', 'date', true], ['status', 'Status', 'select:Ativo|Pausado|Expirado', true]],
    Copilot: [['question', 'Pergunta para o Copilot', 'textarea', true], ['context', 'Contexto', 'textarea', false]]
  };

  const writeToast = (msg, type = 'success') => window.AdminToast?.show ? window.AdminToast.show(msg, type) : alert(msg);
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[ch]));
  const normalizeList = data => Array.isArray(data) ? data : (Array.isArray(data?.items) ? data.items : (Array.isArray(data?.data) ? data.data : (data ? [data] : [])));
  const detail = i => i.service || i.specialty || i.category || i.channel || i.comment || i.code || i.phone || i.description || i.client || i.priority || i.status || 'Operação BarberSync';
  const status = i => i.status || 'Não informado';
  const name = i => i.name || i.fullName || i.title || i.client || i.code || i.label || i.product || 'Registro';
  const money = value => Number(value ?? 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });

  function fieldHtml([key, label, type, required]) {
    const req = required ? 'required' : '';
    if (type === 'textarea') return `<label>${label}${required ? ' *' : ''}<textarea name="${key}" ${req} rows="3" placeholder="${label}"></textarea></label>`;
    if (type.startsWith('select:')) {
      return `<label>${label}${required ? ' *' : ''}<select name="${key}" ${req}>${type.substring(7).split('|').map(o => `<option>${o}</option>`).join('')}</select></label>`;
    }
    const step = type === 'number' ? ' step="0.01" min="0"' : '';
    return `<label>${label}${required ? ' *' : ''}<input name="${key}" type="${type}" ${req}${step} placeholder="${label}" /></label>`;
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
      panel.insertAdjacentHTML('afterbegin', `<div class="module-toolbar" id="${module}Toolbar"><input class="form-control" id="${module}Search" placeholder="Buscar em ${module}..." /><select class="form-control" id="${module}Status"><option value="">Todos os status</option><option>Ativo</option><option>Disponível</option><option>Confirmado</option><option>Crítico</option></select><button class="btn btn-light" data-admin-export="${module}">Exportar visão</button></div>`);
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
      rowsEl.closest('.table-responsive').insertAdjacentHTML('beforebegin', `<div class="kanban-board" id="${module}Kanban"><div><h4>Abertas</h4>${by('abert').map(card).join('') || '<p class="bs-empty-state">Nenhuma comanda aberta.</p>'}</div><div><h4>Em pagamento</h4>${by('pag').map(card).join('') || '<p class="bs-empty-state">Nenhuma comanda em pagamento.</p>'}</div><div><h4>Fechadas</h4>${by('fech').map(card).join('') || '<p class="bs-empty-state">Nenhuma comanda fechada.</p>'}</div></div>`);
    }
    function card(i = {}) { return `<article class="kanban-card"><strong>${escapeHtml(name(i))}</strong><span>${escapeHtml(detail(i))}</span><b>${money(i.total || i.price || 0)}</b></article>`; }
  }

  function actionButtons(module, i, idx) {
    const id = escapeHtml(i.id || idx);
    const detailLabel = module === 'Clients' ? 'Ver Cliente 360' : module === 'Professionals' ? 'Performance' : module === 'ServiceOrders' ? 'Ver recibo' : 'Detalhe';
    const base = `<button class='btn btn-secondary' data-admin-detail='${module}' data-index='${idx}'>${detailLabel}</button> <button class='btn btn-secondary' data-admin-edit='${module}' data-id='${id}' data-index='${idx}'>Editar</button> <button class='btn btn-danger' data-admin-remove='${module}' data-id='${id}'>Excluir</button>`;
    if (module === 'Appointments') return `${base}<div class='status-flow'><span>Agendado</span><span>Confirmado</span><span>Check-in</span><span>Em atendimento</span><span>Finalizado</span></div><div class='row-actions'><button data-id='${id}' data-appointment-action='confirm'>Confirmar</button><button data-id='${id}' data-appointment-action='check-in'>Check-in</button><button data-id='${id}' data-appointment-action='start'>Iniciar</button><button data-id='${id}' data-appointment-action='finish'>Finalizar</button><button data-id='${id}' data-appointment-action='cancel'>Cancelar</button></div>`;
    if (module === 'ServiceOrders') return `${base}<div class='row-actions'><button data-id='${id}' data-order-action='pay'>Registrar pagamento</button><button data-id='${id}' data-order-action='close'>Fechar</button></div>`;
    if (module === 'Stock') return `${base}<div class='stock-bar'><span style='width:${Math.max(0, Math.min(100, Number(i.quantity || i.stock || 0)))}%'></span></div>`;
    if (module === 'Coupons') return `${base}<div class='row-actions'><button data-copy-coupon='${escapeHtml(i.code || '')}'>Copiar código</button></div>`;
    return base;
  }

  function clearApiErrors(form) {
    form?.querySelectorAll('.admin-field-error').forEach(el => el.textContent = '');
    form?.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
  }

  function applyApiErrors(form, errors = []) {
    clearApiErrors(form);
    const messages = [];
    errors.forEach(error => {
      const field = error?.field || error?.Field || error?.propertyName || error?.PropertyName;
      const message = error?.message || error?.Message || String(error || 'Campo inválido.');
      messages.push(message);
      if (!field) return;
      const input = form?.elements?.[field];
      if (!input) return;
      input.classList.add('is-invalid');
      let target = input.closest('.form-group, .field, label')?.querySelector('.admin-field-error');
      if (!target) {
        target = document.createElement('small');
        target.className = 'admin-field-error';
        input.insertAdjacentElement('afterend', target);
      }
      target.textContent = message;
    });
    return messages;
  }

  function richDetail(module, item) {
    return Object.entries(item).slice(0, 18).map(([key, value]) => `<div><span>${escapeHtml(key)}</span><strong>${escapeHtml(typeof value === 'object' ? JSON.stringify(value) : value)}</strong></div>`).join('');
  }

  function normalizePayload(module, body) {
    const normalized = { ...body };
    if (module === 'Professionals' && normalized.commission && !normalized.commissionPercent) normalized.commissionPercent = Number(normalized.commission);
    if (module === 'Services') {
      if (normalized.commission && !normalized.commissionPercent) normalized.commissionPercent = Number(normalized.commission);
      normalized.visibleOnPublicWeb = normalized.site !== 'Não';
      normalized.visibleOnKiosk = normalized.kiosk !== 'Não';
    }
    if (module === 'Appointments' && normalized.date && normalized.time && !normalized.scheduledAt) normalized.scheduledAt = `${normalized.date}T${normalized.time}:00`;
    if (module === 'ServiceOrders') {
      normalized.clientName ||= normalized.client;
      normalized.serviceName ||= normalized.service;
      normalized.professionalName ||= normalized.professional;
      normalized.total ||= 70;
      if (!normalized.items) normalized.items = [{ type: 'service', name: normalized.serviceName || 'Serviço', quantity: 1, price: Number(normalized.total || 70) }];
    }
    if (module === 'Stock') {
      normalized.name ||= normalized.product;
      normalized.productId ||= normalized.productId || normalized.id;
      normalized.salePrice ||= Number(normalized.cost || 1);
      normalized.currentStock ||= Number(normalized.quantity || 0);
      normalized.minStock ||= 0;
    }
    if (module === 'Products') {
      normalized.salePrice = Number(normalized.salePrice || 0);
      normalized.costPrice = Number(normalized.costPrice || 0);
      normalized.currentStock = Number(normalized.currentStock || 0);
      normalized.minStock = Number(normalized.minStock || 0);
    }
    return normalized;
  }

  window.renderAdminCrudPage = async function (module) {
    const endpoint = endpointMap[module] || '/AdminApi/dashboard';
    const copy = moduleCopy[module] || { icon: '💈', singular: module };
    hydrateModal(module);
    const panel = document.querySelector('.module-panel');
    if (panel) ensureWorkspace(module, panel);
    let currentList = [];
    let editingId = null;

    const load = async () => {
      const rows = document.getElementById(`${module}Rows`);
      rows.innerHTML = '<tr><td colspan="4"><div class="bs-skeleton" aria-label="Carregando registros"></div></td></tr>';
      const data = await adminApiClient.adminGet(endpoint);
      if (data?.success === false) {
        currentList = [];
        document.getElementById(`${module}Cards`).innerHTML = '';
        rows.innerHTML = `<tr><td colspan="4"><div class="bs-error-state">${escapeHtml(data.message || 'Não foi possível carregar os registros.')}${data.traceId ? `<span data-trace-id>Código de suporte: ${escapeHtml(data.traceId)}</span>` : ''}</div></td></tr>`;
        return;
      }
      const list = normalizeList(data?.data ?? data);
      currentList = list;
      const activeCount = list.filter(x => /ativo|active|dispon|confirm|ok/i.test(status(x))).length;
      const revenue = list.reduce((acc, x) => acc + Number(x.total || x.price || x.revenueMonth || 0), 0);
      document.getElementById(`${module}Cards`).innerHTML = `<article class='kpi-card'><div class='kpi-icon'>${copy.icon}</div><div><p class='kpi-label'>Total</p><strong class='kpi-value'>${list.length}</strong><span class='kpi-variation'>registros reais</span></div></article><article class='kpi-card'><div class='kpi-icon'>✅</div><div><p class='kpi-label'>Ativos</p><strong class='kpi-value'>${activeCount}</strong><span class='kpi-variation'>em operação</span></div></article><article class='kpi-card'><div class='kpi-icon'>💰</div><div><p class='kpi-label'>Valor</p><strong class='kpi-value'>${money(revenue)}</strong><span class='kpi-variation'>dados da API</span></div></article>`;
      const term = (document.getElementById(`${module}Search`)?.value || '').toLowerCase();
      const filtered = list.filter(i => JSON.stringify(i).toLowerCase().includes(term));
      rows.innerHTML = filtered.slice(0, 50).map((i, idx) => `<tr><td><strong>${escapeHtml(name(i))}</strong><small>${escapeHtml(i.email || i.phone || i.time || i.code || '')}</small></td><td>${escapeHtml(detail(i))}</td><td><span class='badge badge-success'>${escapeHtml(status(i))}</span></td><td>${actionButtons(module, i, idx)}</td></tr>`).join('') || `<tr><td colspan="4"><div class="bs-empty-state">Nenhum registro encontrado.</div></td></tr>`;
    };
    await load();
    document.getElementById(`${module}Search`)?.addEventListener('input', load);
    document.querySelector(`[data-admin-export='${module}']`)?.addEventListener('click', () => writeToast('Exportação disponível nos relatórios executivos.', 'info'));
    document.querySelector(`[data-admin-new='${module}']`)?.addEventListener('click', () => { editingId = null; const form = document.querySelector(`[data-admin-form='${module}']`); form?.reset(); clearApiErrors(form); document.getElementById(`${module}ModalTitle`).textContent = `Novo ${copy.singular}`; window.AdminModal?.openModal ? window.AdminModal.openModal(`${module}Modal`) : (document.getElementById(`${module}Modal`).hidden = false); });
    document.querySelector(`[data-admin-refresh='${module}']`)?.addEventListener('click', load);
    document.querySelectorAll(`[data-admin-close='${module}']`).forEach(b => b.addEventListener('click', () => window.AdminModal?.closeModal ? window.AdminModal.closeModal(`${module}Modal`) : (document.getElementById(`${module}Modal`).hidden = true)));
    document.querySelector(`[data-admin-form='${module}']`)?.addEventListener('submit', async (e) => {
      e.preventDefault();
      if (!e.target.checkValidity()) { document.querySelector(`[data-form-error='${module}']`).hidden = false; applyApiErrors(e.target, [...e.target.querySelectorAll(':invalid')].map(input => ({ field: input.name, message: input.validationMessage || 'Campo obrigatório.' }))); return; }
      const body = normalizePayload(module, Object.fromEntries(new FormData(e.target).entries()));
      clearApiErrors(e.target);
      const submitButton = e.target.querySelector('button[type="submit"]');
      const mutationEndpoint = module === 'ServiceOrders' ? '/AdminApi/service-orders/open' : module === 'Stock' ? (body.movement === 'Saída' ? '/AdminApi/stock/exit' : '/AdminApi/stock/entry') : endpoint;
      adminApiClient.setLoading(submitButton, true);
      try {
        const canUpdate = !['Stock'].includes(module);
        const result = editingId && canUpdate
          ? await adminApiClient.put(`${endpoint}/${encodeURIComponent(editingId)}`, body)
          : await adminApiClient.post(mutationEndpoint, body);
        if (result?.success === false) {
          document.querySelector(`[data-form-error='${module}']`).hidden = false;
          showFormErrors(applyApiErrors(e.target, result.errors || []));
          return;
        }
        window.AdminModal?.closeModal ? window.AdminModal.closeModal(`${module}Modal`) : (document.getElementById(`${module}Modal`).hidden = true);
        e.target.reset();
        writeToast(result?.message || `${copy.singular} salvo com sucesso.`);
        await load();
      } finally {
        adminApiClient.setLoading(submitButton, false);
      }
    });
    document.getElementById(`${module}Rows`)?.addEventListener('click', async (e) => {
      const edit = e.target.closest('[data-admin-edit]');
      const remove = e.target.closest('[data-admin-remove]');
      const detailBtn = e.target.closest('[data-admin-detail]');
      const apptAction = e.target.closest('[data-appointment-action]');
      const orderAction = e.target.closest('[data-order-action]');
      const copyCoupon = e.target.closest('[data-copy-coupon]');
      if (copyCoupon) { navigator.clipboard?.writeText(copyCoupon.dataset.copyCoupon); writeToast('Cupom copiado com sucesso.', 'success'); }
      if (detailBtn) {
        const item = currentList[Number(detailBtn.dataset.index)] || {};
        document.getElementById(`${module}DetailBody`).innerHTML = richDetail(module, item);
        document.getElementById(`${module}DetailModal`).hidden = false;
      }
      if (edit) {
        editingId = edit.dataset.id;
        const item = currentList.find(x => String(x.id) === String(editingId)) || currentList[Number(edit.dataset.index)] || {};
        const form = document.querySelector(`[data-admin-form='${module}']`);
        form?.reset();
        clearApiErrors(form);
        Object.entries(item).forEach(([key, value]) => { const field = form?.elements?.[key] || form?.elements?.[key === 'type' ? 'personType' : key]; if (field && typeof value !== 'object') field.value = value; });
        document.getElementById(`${module}ModalTitle`).textContent = `Editar ${copy.singular}`;
        window.AdminModal?.openModal ? window.AdminModal.openModal(`${module}Modal`) : (document.getElementById(`${module}Modal`).hidden = false);
      }
      if (remove) {
        const executeDelete = async () => {
          const result = await adminApiClient.delete(`${endpoint}/${remove.dataset.id}`);
          if (result?.success === false) return;
          writeToast(result?.message || `${copy.singular} excluído com sucesso.`);
          await load();
        };
        if (window.AdminModal?.confirmAction) window.AdminModal.confirmAction(`Excluir ${copy.singular}?`, executeDelete);
        else if (confirm(`Excluir ${copy.singular}?`)) await executeDelete();
      }
      if (apptAction) {
        const id = apptAction.dataset.id;
        if (!id) return;
        const result = await adminApiClient.post(`/AdminApi/appointments/${encodeURIComponent(id)}/${apptAction.dataset.appointmentAction}`, {});
        if (result?.success === false) return;
        writeToast(result?.message || 'Agendamento atualizado.', 'info');
        await load();
      }
      if (orderAction) {
        const id = orderAction.dataset.id;
        if (!id) return;
        const item = currentList.find(value => String(value.id) === String(id));
        const payload = orderAction.dataset.orderAction === 'pay' ? { amount: Number(item?.total), method: item?.paymentMethod } : {};
        const result = await adminApiClient.post(`/AdminApi/service-orders/${encodeURIComponent(id)}/${orderAction.dataset.orderAction}`, payload);
        if (result?.success === false) return;
        writeToast(result?.message || 'Comanda atualizada.', 'success');
        await load();
      }
    });
  };

  function openCreateModal(moduleName) {
    document.querySelector(`[data-admin-new='${moduleName}']`)?.click();
  }

  function openEditModal(moduleName, id) {
    document.querySelector(`[data-admin-edit][data-id='${id}']`)?.click();
  }

  function openDetailsModal(moduleName, id) {
    document.querySelector(`[data-admin-detail][data-id='${id}']`)?.click();
  }

  function confirmDelete(moduleName, id) {
    document.querySelector(`[data-admin-remove][data-id='${id}']`)?.click();
  }

  function submitModuleForm(moduleName) {
    document.querySelector(`[data-admin-form='${moduleName}']`)?.requestSubmit();
  }

  function refreshModule(moduleName) {
    document.querySelector(`[data-admin-refresh='${moduleName}']`)?.click();
  }

  function renderModuleTable(moduleName, data) {
    const rows = document.getElementById(`${moduleName}Rows`);
    if (!rows) return;
    const list = normalizeList(data);
    rows.innerHTML = list.map((item, idx) => `<tr><td><strong>${escapeHtml(name(item))}</strong></td><td>${escapeHtml(detail(item))}</td><td><span class='badge badge-success'>${escapeHtml(status(item))}</span></td><td>${actionButtons(moduleName, item, idx)}</td></tr>`).join('');
  }

  function renderModuleCards(moduleName, data) {
    const cards = document.getElementById(`${moduleName}Cards`);
    if (!cards) return;
    const copy = moduleCopy[moduleName] || { icon: '💈' };
    const list = normalizeList(data);
    cards.innerHTML = `<article class='kpi-card'><div class='kpi-icon'>${copy.icon}</div><div><p class='kpi-label'>Total</p><strong class='kpi-value'>${list.length}</strong><span class='kpi-variation'>renderizado</span></div></article>`;
  }


  function showFormErrors(errors) {
    const message = Array.isArray(errors) ? errors.join('\n') : (errors || 'Revise os campos obrigatórios.');
    window.AdminToast?.showError?.(message);
  }

  function clearForm(formId) {
    const form = document.getElementById(formId) || document.querySelector(`[data-admin-form='${formId}']`);
    form?.reset?.();
  }

  Object.assign(window, { openCreateModal, openEditModal, openDetailsModal, confirmDelete, submitModuleForm, refreshModule, renderModuleTable, renderModuleCards, showFormErrors, clearForm, applyApiErrors, clearApiErrors });

})();
