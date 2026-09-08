(() => {
  'use strict';
  const root = document.querySelector('[data-platform-page]');
  if (!root) return;
  const page = root.dataset.platformPage;
  const money = value => value == null ? 'Preço não configurado' : new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
  const number = value => new Intl.NumberFormat('pt-BR').format(value || 0);
  const date = value => value ? new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(value)) : 'Sem acesso registrado';
  const escape = value => String(value ?? '').replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character]));
  const error = root.querySelector('[data-error]');
  const loading = root.querySelector('[data-loading]');
  const request = async (url, options = {}) => {
    const token = root.querySelector('input[name="__RequestVerificationToken"]')?.value;
    const headers = { Accept: 'application/json', ...(options.body ? { 'Content-Type': 'application/json' } : {}), ...(token ? { RequestVerificationToken: token } : {}), ...(options.headers || {}) };
    const response = await fetch(url, { credentials: 'same-origin', ...options, headers });
    const payload = await response.json().catch(() => null);
    if (!response.ok) throw new Error(payload?.message || 'Não foi possível consultar os dados da plataforma.');
    return payload?.data ?? payload;
  };
  const showError = reason => { if (loading) loading.hidden = true; if (error) { error.hidden = false; error.querySelector('p').textContent = reason.message; } };
  root.querySelector('[data-retry]')?.addEventListener('click', () => location.reload());

  async function dashboard() {
    const data = await request('/Platform/Data/Dashboard');
    const kpis = root.querySelector('[data-kpis]');
    const values = [['Clientes ativos', data.activeTenants], ['Trials', data.trialTenants], ['Suspensos', data.suspendedTenants], ['MRR', money(data.mrr)], ['Usuários', data.users], ['Unidades', data.branches], ['Profissionais', data.professionals], ['Clientes finais', data.clients], ['Totens', data.kiosks], ['ARR', money(data.arr)]];
    kpis.innerHTML = values.map(item => `<article><span>${escape(item[0])}</span><strong>${typeof item[1] === 'number' ? number(item[1]) : escape(item[1])}</strong></article>`).join('');
    root.querySelector('[data-plans]').innerHTML = (data.plans || []).map(plan => `<div class="module-meta"><strong>${escape(plan.planName)}</strong><span>${number(plan.tenants)} clientes</span><span>${money(plan.revenue)}</span></div>`).join('') || '<p class="platform-empty">Nenhum plano contratado.</p>';
    root.querySelector('[data-modules]').innerHTML = (data.modules || []).map(module => `<tr><td><strong>${escape(module.name)}</strong><small>${escape(module.moduleKey)}</small></td><td>${number(module.activeTenants)}</td><td>${number(module.trialTenants)}</td><td>${number(module.adoptionPercent)}%</td><td>${money(module.revenue)}</td></tr>`).join('');
    loading.hidden = true; kpis.hidden = false; root.querySelector('[data-content]').hidden = false;
  }

  let tenantPage = 1;
  async function tenants() {
    const form = root.querySelector('[data-tenant-filters]');
    const params = new URLSearchParams(new FormData(form)); params.set('page', tenantPage); params.set('pageSize', '25');
    [...params].forEach(([key, value]) => { if (!value) params.delete(key); });
    const data = await request(`/Platform/Data/Tenants?${params}`); const body = root.querySelector('[data-tenants]');
    body.innerHTML = data.items.map(item => `<tr><td><strong>${escape(item.name)}</strong><small>${escape(item.institutionalEmail || item.slug)}</small></td><td>${escape(item.maskedDocument || 'Não informado')}</td><td>${escape(item.planName || 'Sem plano')}</td><td>${number(item.modules)}</td><td>${number(item.users)}</td><td>${number(item.branches)}</td><td><span class="status-pill">${escape(item.status)}</span></td><td>${escape(date(item.lastAccess))}</td><td><details class="row-menu"><summary aria-label="Ações">⋮</summary><a href="/Platform/Tenants/${encodeURIComponent(item.id)}">Ver detalhes</a></details></td></tr>`).join('');
    root.querySelector('[data-empty]').hidden = data.items.length > 0; root.querySelector('[data-page-info]').textContent = `Página ${data.page} de ${Math.max(data.totalPages, 1)} · ${number(data.total)} clientes`; root.querySelector('[data-prev]').disabled = data.page <= 1; root.querySelector('[data-next]').disabled = data.page >= data.totalPages;
  }

  async function tenant() {
    const id = root.dataset.tenantId; const [detail, users, modules, roles, branches, usage] = await Promise.all([request(`/Platform/Data/Tenants/${id}`), request(`/Platform/Data/Tenants/${id}/Users?page=1&pageSize=50`), request(`/Platform/Data/Tenants/${id}/Modules`), request(`/Platform/Data/Tenants/${id}/Roles`), request(`/Platform/Data/Tenants/${id}/Branches`), request(`/Platform/Data/Tenants/${id}/Usage`)]);
    root.querySelector('[data-tenant-name]').textContent = detail.name; root.querySelector('[data-tenant-document]').textContent = detail.maskedDocument || 'Documento não informado'; root.querySelector('[data-tenant-status]').textContent = detail.status;
    const overview = [['Plano', detail.planName || 'Sem plano'], ['Assinatura', detail.subscriptionStatus || 'Não configurada'], ['Ciclo', detail.billingCycle || 'Não configurado'], ['Unidades', detail.branches], ['Usuários', detail.users], ['Profissionais', detail.professionals], ['Clientes finais', detail.clients], ['Último acesso', date(detail.lastAccess)]];
    root.querySelector('[data-overview]').innerHTML = overview.map(item => `<article><span>${escape(item[0])}</span><strong>${escape(item[1])}</strong></article>`).join('');
    root.querySelector('[data-tenant-users]').innerHTML = users.items.map(user => `<tr><td><strong>${escape(user.name)}</strong><small>${escape(user.email)}</small></td><td>${escape(user.maskedCpf || 'Não informado')}</td><td>${escape(user.roles.join(', ') || 'Sem perfil')}</td><td>${escape(user.branchName || 'Sem unidade')}</td><td><span class="status-pill">${escape(user.status)}</span></td><td>${escape(date(user.lastAccess))}</td></tr>`).join('');
    root.querySelector('[data-tenant-modules]').innerHTML = modules.map(module => `<article class="module-card"><header><h2>${escape(module.moduleName)}</h2><span class="status-pill">${escape(module.status)}</span></header><p>${escape(module.moduleKey)}</p><div class="module-meta"><span>${escape(module.billingCycle)}</span><span>${money(module.contractedPrice)}</span><span>Início ${escape(date(module.startsAt))}</span></div></article>`).join('') || '<p class="platform-empty">Nenhum add-on contratado. Módulos do plano base permanecem na assinatura.</p>';
    root.querySelector('[data-tenant-roles]').innerHTML = roles.map(role => `<tr><td><strong>${escape(role.name)}</strong>${role.isSystem ? '<small>Sistema</small>' : '<small>Personalizado</small>'}</td><td>${escape(role.code)}</td><td>${number(role.users)}</td><td>${escape(role.permissions.join(', ') || 'Sem permissões')}</td></tr>`).join('') || '<tr><td colspan="4">Nenhum perfil disponível.</td></tr>';
    root.querySelector('[data-tenant-branches]').innerHTML = branches.map(branch => `<tr><td><strong>${escape(branch.name)}</strong></td><td>${escape(branch.code || 'Não informado')}</td><td>${number(branch.users)}</td><td>${number(branch.professionals)}</td><td><span class="status-pill">${escape(branch.status)}</span></td></tr>`).join('') || '<tr><td colspan="5">Nenhuma unidade cadastrada.</td></tr>';
    root.querySelector('[data-tenant-contract]').innerHTML = [['Plano base', detail.planName || 'Sem plano'], ['Situação', detail.subscriptionStatus || 'Não configurada'], ['Ciclo', detail.billingCycle || 'Não configurado'], ['Início', date(detail.startsAt)], ['Término / renovação', date(detail.endsAt)]].map(item => `<article><span>${escape(item[0])}</span><strong>${escape(item[1])}</strong></article>`).join('');
    root.querySelector('[data-contract-modules]').innerHTML = modules.map(module => `<article class="module-card"><header><h2>${escape(module.moduleName)}</h2><span class="status-pill">${escape(module.status)}</span></header><div class="module-meta"><span>${escape(module.billingCycle)}</span><span>${money(module.contractedPrice)}</span></div></article>`).join('');
    root.querySelector('[data-tenant-usage]').innerHTML = usage.map(item => `<tr><td>${escape(new Intl.DateTimeFormat('pt-BR').format(new Date(`${item.date}T12:00:00`)))}</td><td><strong>${escape(item.moduleName)}</strong><small>${escape(item.moduleKey)}</small></td><td>${number(item.activeUsers)}</td><td>${number(item.requests)}</td><td>${number(item.relevantOperations)}</td><td>${item.usageUnits == null ? '—' : number(item.usageUnits)}</td></tr>`).join('');
    root.querySelector('[data-usage-empty]').hidden = usage.length > 0;
  }

  let moduleMap = new Map();
  async function modules() {
    const data = await request('/Platform/Data/Modules'); moduleMap = new Map(data.map(module => [module.id, module]));
    root.querySelector('[data-module-cards]').innerHTML = data.map(module => `<article class="module-card"><header><div><small>${escape(module.category)}</small><h2>${escape(module.name)}</h2></div><span class="status-pill">${escape(module.status)}</span></header><p>${escape(module.description || 'Descrição não configurada.')}</p><div class="module-meta"><span>${escape(module.moduleKey)}</span><span>${module.currentMonthlyPrice == null ? 'Preço não configurado' : money(module.currentMonthlyPrice) + '/mês'}</span><span>${number(module.activeTenants)} tenants ativos</span>${module.isCore ? '<span>Core</span>' : ''}</div><div class="module-actions"><button type="button" data-edit-module="${escape(module.id)}">Editar</button><button type="button" data-new-price="${escape(module.id)}" data-module-name="${escape(module.name)}">Novo preço</button></div></article>`).join('');
    root.querySelectorAll('[data-edit-module]').forEach(button => button.addEventListener('click', () => openModule(moduleMap.get(button.dataset.editModule))));
    root.querySelectorAll('[data-new-price]').forEach(button => button.addEventListener('click', () => openPrice(button.dataset.newPrice, button.dataset.moduleName)));
  }

  async function prices() {
    const data = await request('/Platform/Data/Prices');
    root.querySelector('[data-prices]').innerHTML = data.map(item => `<tr><td><strong>${escape(item.moduleName)}</strong><small>${escape(item.moduleKey)}</small></td><td>${escape(item.billingCycle)}</td><td>${money(item.price)} ${escape(item.currency)}</td><td>${escape(date(item.validFrom))} — ${item.validUntil ? escape(date(item.validUntil)) : 'sem término'}</td><td><span class="status-pill">${escape(item.status)}</span></td></tr>`).join('');
    root.querySelector('[data-empty]').hidden = data.length > 0;
  }

  async function contracts() {
    const data = await request('/Platform/Data/Contracts' + location.search);
    root.querySelector('[data-contracts]').innerHTML = data.map(item => `<tr><td><strong>${escape(item.tenantName)}</strong></td><td>${escape(item.moduleName)}<small>${escape(item.moduleKey)}</small></td><td><span class="status-pill">${escape(item.status)}</span></td><td>${escape(item.billingCycle)}</td><td>${money(item.contractedPrice)} ${escape(item.currency)}</td><td>${escape(date(item.startsAt))}</td><td>${item.endsAt ? escape(date(item.endsAt)) : 'Sem término'}</td></tr>`).join('');
    root.querySelector('[data-empty]').hidden = data.length > 0;
  }

  async function audit() {
    const params = new URLSearchParams(location.search); params.set('pageSize', '250');
    const data = await request(`/Platform/Data/Audit?${params}`);
    root.querySelector('[data-audit]').innerHTML = data.map(item => `<tr><td>${escape(date(item.createdAt))}</td><td><strong>${escape(item.action)}</strong></td><td>${escape(item.module)}</td><td>${escape(item.entity)}${item.entityId ? `<small>${escape(item.entityId)}</small>` : ''}</td><td>${escape(item.actorUserId || 'Sistema')}</td><td>${escape(item.reason || 'Não informado')}</td><td>${escape(item.correlationId || 'Não informado')}</td></tr>`).join('');
    root.querySelector('[data-empty]').hidden = data.length > 0;
  }

  const moduleDialog = root.querySelector('[data-module-dialog]'); const moduleForm = root.querySelector('[data-module-form]');
  const priceDialog = root.querySelector('[data-price-dialog]'); const priceForm = root.querySelector('[data-price-form]');
  function openModule(module) {
    moduleForm.reset(); moduleForm.elements.id.value = module?.id || ''; moduleForm.elements.moduleKey.value = module?.moduleKey || ''; moduleForm.elements.moduleKey.disabled = Boolean(module);
    moduleForm.elements.name.value = module?.name || ''; moduleForm.elements.category.value = module?.category || ''; moduleForm.elements.description.value = module?.description || '';
    moduleForm.elements.status.value = module?.status || 'Active'; moduleForm.elements.displayOrder.value = module?.displayOrder ?? 100; moduleForm.elements.iconKey.value = module?.iconKey || '';
    moduleForm.elements.isCore.checked = Boolean(module?.isCore); moduleForm.elements.isSellable.checked = module ? Boolean(module.isSellable) : true;
    root.querySelector('[data-form-title]').textContent = module ? `Editar ${module.name}` : 'Novo módulo'; moduleForm.querySelector('[data-form-error]').hidden = true; moduleDialog.showModal();
  }
  function openPrice(moduleId, moduleName) { priceForm.reset(); priceForm.elements.moduleId.value = moduleId; priceForm.elements.currency.value = 'BRL'; priceForm.elements.status.value = 'Active'; priceForm.elements.validFrom.value = new Date(Date.now() - new Date().getTimezoneOffset() * 60000).toISOString().slice(0,16); root.querySelector('[data-price-title]').textContent = `Novo preço · ${moduleName}`; priceForm.querySelector('[data-form-error]').hidden = true; priceDialog.showModal(); }
  root.querySelector('[data-new-module]')?.addEventListener('click', () => openModule());
  root.querySelectorAll('[data-close]').forEach(button => button.addEventListener('click', () => button.closest('dialog').close()));
  moduleForm?.addEventListener('submit', async event => { event.preventDefault(); const submit=moduleForm.querySelector('[type="submit"]');submit.disabled=true;try{const values=new FormData(moduleForm);const id=values.get('id');const body={name:values.get('name'),description:values.get('description')||null,category:values.get('category'),status:values.get('status'),displayOrder:Number(values.get('displayOrder')),iconKey:values.get('iconKey')||null,isCore:values.get('isCore')==='on',isSellable:values.get('isSellable')==='on'};if(!id)body.moduleKey=values.get('moduleKey');await request(id?`/Platform/Data/Modules/${id}`:'/Platform/Data/Modules',{method:id?'PUT':'POST',body:JSON.stringify(body)});moduleDialog.close();await modules();}catch(exception){const message=moduleForm.querySelector('[data-form-error]');message.hidden=false;message.textContent=exception.message;}finally{submit.disabled=false;}});
  priceForm?.addEventListener('submit', async event => { event.preventDefault();const submit=priceForm.querySelector('[type="submit"]');submit.disabled=true;try{const values=new FormData(priceForm);const body={moduleId:values.get('moduleId'),billingCycle:values.get('billingCycle'),currency:values.get('currency'),price:Number(values.get('price')),validFrom:new Date(values.get('validFrom')).toISOString(),validUntil:values.get('validUntil')?new Date(values.get('validUntil')).toISOString():null,status:values.get('status')};await request('/Platform/Data/Prices',{method:'POST',body:JSON.stringify(body)});priceDialog.close();await modules();}catch(exception){const message=priceForm.querySelector('[data-form-error]');message.hidden=false;message.textContent=exception.message;}finally{submit.disabled=false;}});

  root.querySelectorAll('[data-tab]').forEach(button => button.addEventListener('click', () => { root.querySelectorAll('[data-tab]').forEach(item => item.classList.toggle('active', item === button)); root.querySelectorAll('[data-tab-panel]').forEach(panel => { panel.hidden = panel.dataset.tabPanel !== button.dataset.tab; }); }));
  root.querySelector('[data-tenant-filters]')?.addEventListener('submit', event => { event.preventDefault(); tenantPage = 1; tenants().catch(showError); });
  root.querySelector('[data-prev]')?.addEventListener('click', () => { tenantPage--; tenants().catch(showError); }); root.querySelector('[data-next]')?.addEventListener('click', () => { tenantPage++; tenants().catch(showError); });
  ({ dashboard, tenants, tenant, modules, prices, contracts, audit }[page] || (() => Promise.resolve()))().catch(showError);
})();
