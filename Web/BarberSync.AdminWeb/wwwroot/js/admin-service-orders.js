(() => {
  'use strict';

  const root = document.querySelector('[data-pos-root]');
  if (!root) return;

  const state = { orders: [], order: null, services: [], products: [], clients: [], professionals: [], filter: 'all', query: '', sending: false };
  const $ = (selector, host = document) => host.querySelector(selector);
  const $$ = (selector, host = document) => [...host.querySelectorAll(selector)];
  const money = value => Number(value || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]);
  const valueOf = (object, ...keys) => keys.map(key => object?.[key]).find(value => value !== undefined);
  const idOf = object => valueOf(object, 'id', 'Id');

  async function api(path, options = {}) {
    const response = await fetch(`/AdminApi/${path}`, {
      credentials: 'same-origin',
      headers: { Accept: 'application/json', ...(options.body ? { 'Content-Type': 'application/json' } : {}) },
      ...options
    });
    const body = await response.json().catch(() => null);
    if (!response.ok) throw new Error(body?.detail || body?.title || body?.message || `Não foi possível concluir a operação (${response.status}).`);
    return body;
  }

  function notify(message, type = 'success') {
    if (window.AdminToast?.show) window.AdminToast.show(message, type);
    else $('[data-pos-state]').textContent = message;
  }

  function normalizeList(payload) {
    if (Array.isArray(payload)) return payload;
    return payload?.items || payload?.data || payload?.results || [];
  }

  async function load() {
    root.setAttribute('aria-busy', 'true');
    $('[data-pos-state]').className = 'bs-pos-state';
    $('[data-pos-state]').textContent = 'Carregando dados da operação…';
    try {
      const [orders, services, products, clients, professionals] = await Promise.all([
        api('service-orders'), api('services'), api('products'), api('clients'), api('professionals')
      ]);
      state.orders = normalizeList(orders);
      state.services = normalizeList(services);
      state.products = normalizeList(products);
      state.clients = normalizeList(clients);
      state.professionals = normalizeList(professionals);
      const selectedId = idOf(state.order) || idOf(state.orders.find(order => !['Closed', 'Cancelled', 'Refunded'].includes(valueOf(order, 'status', 'Status')))) || idOf(state.orders[0]);
      state.order = selectedId ? await api(`service-orders/${selectedId}`) : null;
      $('[data-pos-state]').textContent = state.order ? '' : 'Nenhuma comanda encontrada. Abra uma comanda para começar.';
      render();
    } catch (error) {
      $('[data-pos-state]').className = 'bs-pos-state is-error';
      $('[data-pos-state]').textContent = error.message;
      notify(error.message, 'error');
    } finally { root.setAttribute('aria-busy', 'false'); }
  }

  function render() {
    renderPicker(); renderCatalog(); renderOrder(); renderSummary();
  }

  function renderPicker() {
    const picker = $('[data-order-picker]');
    picker.innerHTML = state.orders.length ? state.orders.map(order => `<option value="${idOf(order)}" ${idOf(order) === idOf(state.order) ? 'selected' : ''}>#${escapeHtml(valueOf(order, 'number', 'Number') || String(idOf(order)).slice(0, 8))} · ${escapeHtml(valueOf(order, 'status', 'Status'))}</option>`).join('') : '<option value="">Sem comandas</option>';
  }

  function renderCatalog() {
    const query = state.query.toLocaleLowerCase('pt-BR');
    const services = state.services.map(item => ({ ...item, catalogType: 'service' }));
    const products = state.products.map(item => ({ ...item, catalogType: 'product' }));
    const items = [...services, ...products].filter(item => {
      const name = valueOf(item, 'name', 'Name', 'description', 'Description') || '';
      return (state.filter === 'all' || item.catalogType === state.filter) && name.toLocaleLowerCase('pt-BR').includes(query);
    });
    $('[data-catalog]').innerHTML = items.length ? items.map(item => {
      const isProduct = item.catalogType === 'product';
      const stock = Number(valueOf(item, 'currentStock', 'CurrentStock', 'stock', 'Stock') || 0);
      const allowNegative = Boolean(valueOf(item, 'allowNegativeStock', 'AllowNegativeStock'));
      const unavailable = isProduct && stock <= 0 && !allowNegative;
      const price = valueOf(item, 'price', 'Price', 'salePrice', 'SalePrice');
      const duration = valueOf(item, 'durationMinutes', 'DurationMinutes');
      return `<button class="bs-catalog-card" type="button" data-add-catalog="${item.catalogType}" data-id="${idOf(item)}" ${unavailable ? 'disabled' : ''}>
        <span class="bs-catalog-type">${isProduct ? 'Produto' : 'Serviço'}</span>
        <strong>${escapeHtml(valueOf(item, 'name', 'Name', 'description', 'Description'))}</strong>
        <small>${isProduct ? `${stock} em estoque` : `${duration || '—'} min`}</small>
        <b>${money(price)}</b>${unavailable ? '<em>Sem estoque</em>' : ''}</button>`;
    }).join('') : '<div class="bs-empty">Nenhum item corresponde à busca.</div>';
  }

  function renderOrder() {
    const order = state.order;
    const items = valueOf(order, 'items', 'Items') || [];
    $('[data-item-count]').textContent = items.length;
    $('[data-order-status]').textContent = valueOf(order, 'status', 'Status') || '—';
    $('[data-order-meta]').textContent = order ? `Comanda #${valueOf(order, 'number', 'Number')} · Cliente ${String(valueOf(order, 'clientId', 'ClientId')).slice(0, 8)}` : 'Selecione ou abra uma comanda.';
    $('[data-order-items]').innerHTML = items.length ? items.map(item => `<div class="bs-order-item">
      <div><strong>${escapeHtml(valueOf(item, 'description', 'Description'))}</strong><small>${escapeHtml(valueOf(item, 'type', 'Type'))} · ${money(valueOf(item, 'unitPrice', 'UnitPrice'))}</small></div>
      <label>Qtd.<input type="number" min="0.01" step="0.01" value="${valueOf(item, 'quantity', 'Quantity')}" data-item-quantity="${idOf(item)}" /></label>
      <b>${money(valueOf(item, 'total', 'Total'))}</b>
      <button type="button" data-remove-item="${idOf(item)}" aria-label="Remover ${escapeHtml(valueOf(item, 'description', 'Description'))}">×</button>
    </div>`).join('') : '<div class="bs-empty">A comanda ainda não possui itens.</div>';
  }

  function renderSummary() {
    const order = state.order || {};
    $('[data-subtotal]').textContent = money(valueOf(order, 'subtotal', 'Subtotal'));
    $('[data-discount]').textContent = `− ${money(valueOf(order, 'discount', 'Discount'))}`;
    $('[data-total]').textContent = money(valueOf(order, 'total', 'Total'));
    $('[data-paid]').textContent = money(valueOf(order, 'paid', 'Paid'));
    $('[data-balance]').textContent = money(valueOf(order, 'balance', 'Balance'));
    const readonly = !state.order || ['Paid', 'Closed', 'Refunded', 'Cancelled'].includes(valueOf(order, 'status', 'Status'));
    $$('[data-coupon-form] input, [data-coupon-form] button, [data-cashback-form] input, [data-cashback-form] button').forEach(control => { control.disabled = readonly; });
    $('[data-open-payment]').disabled = readonly || Number(valueOf(order, 'balance', 'Balance') || 0) <= 0;
  }

  async function refreshOrder(orderId = idOf(state.order)) {
    if (orderId) state.order = await api(`service-orders/${orderId}`);
    const index = state.orders.findIndex(order => idOf(order) === orderId);
    if (index >= 0) state.orders[index] = state.order;
    else if (state.order) state.orders.unshift(state.order);
    render();
  }

  async function mutate(path, body, success, method = 'POST') {
    if (state.sending) return;
    state.sending = true;
    try {
      state.order = await api(path, { method, body: JSON.stringify(body) });
      const index = state.orders.findIndex(order => idOf(order) === idOf(state.order));
      if (index >= 0) state.orders[index] = state.order; else state.orders.unshift(state.order);
      render(); notify(success);
    } catch (error) { notify(error.message, 'error'); throw error; }
    finally { state.sending = false; }
  }

  function addSplit() {
    const row = document.createElement('div');
    row.className = 'bs-split';
    row.innerHTML = `<label>Forma<select data-split-method><option value="Pix">PIX</option><option value="CreditCard">Crédito</option><option value="DebitCard">Débito</option><option value="Cash">Dinheiro</option></select></label><label>Valor<input data-split-amount type="number" min="0.01" step="0.01" required /></label><label class="bs-received" hidden>Recebido<input data-split-received type="number" min="0.01" step="0.01" /></label><button type="button" data-remove-split aria-label="Remover forma">×</button>`;
    $('[data-splits]').append(row); updatePaymentPreview();
  }

  function updatePaymentPreview() {
    let total = 0; let change = 0;
    $$('.bs-split').forEach(row => {
      const method = $('[data-split-method]', row).value;
      const amount = Number($('[data-split-amount]', row).value || 0);
      const received = Number($('[data-split-received]', row).value || 0);
      $('.bs-received', row).hidden = method !== 'Cash';
      if (method !== 'Cash') $('[data-split-received]', row).value = '';
      total += amount; if (method === 'Cash') change += Math.max(0, received - amount);
    });
    $('[data-split-total]').textContent = money(total); $('[data-change]').textContent = money(change);
  }

  function openPayment() {
    $('[data-splits]').innerHTML = ''; addSplit();
    $('[data-split-amount]').value = Number(valueOf(state.order, 'balance', 'Balance') || 0).toFixed(2);
    $('[data-payment-error]').textContent = '';
    $('[data-payment-drawer]').hidden = false; $('[data-drawer-backdrop]').hidden = false;
    updatePaymentPreview(); $('[data-split-method]').focus();
  }
  function closePayment() { $('[data-payment-drawer]').hidden = true; $('[data-drawer-backdrop]').hidden = true; }

  root.addEventListener('click', async event => {
    const tab = event.target.closest('[data-pos-tab]');
    if (tab) { $$('[data-pos-tab]').forEach(button => button.classList.toggle('is-active', button === tab)); root.dataset.mobileTab = tab.dataset.posTab; }
    const filter = event.target.closest('[data-catalog-filter]');
    if (filter) { state.filter = filter.dataset.catalogFilter; $$('[data-catalog-filter]').forEach(button => button.classList.toggle('is-active', button === filter)); renderCatalog(); }
    const add = event.target.closest('[data-add-catalog]');
    if (add && state.order) {
      const professionalId = idOf(state.professionals[0]);
      const service = add.dataset.addCatalog === 'service';
      if (service && !professionalId) return notify('Cadastre um profissional antes de adicionar um serviço.', 'error');
      await mutate(`service-orders/${idOf(state.order)}/items/${service ? 'services' : 'products'}`, service ? { serviceId: add.dataset.id, professionalId, quantity: 1 } : { productId: add.dataset.id, quantity: 1 }, 'Item adicionado à comanda.').catch(() => {});
    }
    const remove = event.target.closest('[data-remove-item]');
    if (remove && state.order && window.confirm('Remover este item da comanda?')) await mutate(`service-orders/${idOf(state.order)}/items/${remove.dataset.removeItem}`, null, 'Item removido.', 'DELETE').catch(() => {});
    if (event.target.closest('[data-refresh]')) await load();
    if (event.target.closest('[data-open-order]')) {
      const select = $('[data-open-form] select[name="clientId"]');
      select.innerHTML = state.clients.map(client => `<option value="${idOf(client)}">${escapeHtml(valueOf(client, 'name', 'Name'))}</option>`).join('');
      $('[data-open-dialog]').showModal();
    }
    if (event.target.closest('[data-open-payment]')) openPayment();
  });

  root.addEventListener('change', async event => {
    if (event.target.matches('[data-order-picker]') && event.target.value) { state.order = await api(`service-orders/${event.target.value}`); render(); }
    if (event.target.matches('[data-item-quantity]')) {
      const item = (valueOf(state.order, 'items', 'Items') || []).find(candidate => idOf(candidate) === event.target.dataset.itemQuantity);
      await mutate(`service-orders/${idOf(state.order)}/items/${idOf(item)}`, { quantity: Number(event.target.value), discount: valueOf(item, 'discount', 'Discount') || 0, professionalId: valueOf(item, 'professionalId', 'ProfessionalId') }, 'Quantidade atualizada.', 'PUT').catch(() => renderOrder());
    }
  });
  $('[data-catalog-search]').addEventListener('input', event => { state.query = event.target.value; renderCatalog(); });

  $('[data-coupon-form]').addEventListener('submit', async event => {
    event.preventDefault(); const feedback = $('[data-benefit-feedback]'); feedback.textContent = '';
    try { await mutate(`service-orders/${idOf(state.order)}/coupon`, { code: new FormData(event.currentTarget).get('code') }, 'Cupom aplicado.'); event.currentTarget.reset(); feedback.textContent = 'Cupom aplicado ao resumo real da comanda.'; }
    catch (error) { feedback.textContent = error.message; }
  });
  $('[data-cashback-form]').addEventListener('submit', async event => {
    event.preventDefault(); const amount = Number(new FormData(event.currentTarget).get('amount')); const balance = Number(valueOf(state.order, 'balance', 'Balance'));
    if (amount <= 0 || amount > balance) return $('[data-benefit-feedback]').textContent = 'Informe um cashback positivo e não superior ao saldo.';
    try { await mutate(`service-orders/${idOf(state.order)}/cashback`, { amount }, 'Cashback aplicado.'); event.currentTarget.reset(); $('[data-benefit-feedback]').textContent = 'Cashback debitado e resumo atualizado.'; }
    catch (error) { $('[data-benefit-feedback]').textContent = error.message; }
  });

  $('[data-open-form]').addEventListener('submit', async event => {
    event.preventDefault(); if (event.submitter?.value === 'cancel') return;
    const data = Object.fromEntries(new FormData(event.currentTarget));
    if (!data.appointmentId) data.appointmentId = null;
    try { await mutate('service-orders/open', data, 'Comanda aberta.'); $('[data-open-dialog]').close(); event.currentTarget.reset(); }
    catch (error) { $('[data-open-error]').textContent = error.message; }
  });

  $('[data-payment-drawer]').addEventListener('click', event => {
    if (event.target.closest('[data-close-payment]')) closePayment();
    if (event.target.closest('[data-add-split]')) addSplit();
    const remove = event.target.closest('[data-remove-split]'); if (remove) { remove.closest('.bs-split').remove(); updatePaymentPreview(); }
  });
  $('[data-drawer-backdrop]').addEventListener('click', closePayment);
  $('[data-payment-drawer]').addEventListener('input', updatePaymentPreview);
  $('[data-payment-drawer]').addEventListener('change', updatePaymentPreview);
  $('[data-payment-form]').addEventListener('submit', async event => {
    event.preventDefault(); const error = $('[data-payment-error]'); error.textContent = '';
    const splits = $$('.bs-split').map(row => ({ method: $('[data-split-method]', row).value, amount: Number($('[data-split-amount]', row).value), receivedAmount: $('[data-split-method]', row).value === 'Cash' && $('[data-split-received]', row).value ? Number($('[data-split-received]', row).value) : null }));
    if (!splits.length) return error.textContent = 'Adicione ao menos uma forma de pagamento.';
    if (splits.some(split => !split.amount || split.amount <= 0)) return error.textContent = 'Todos os valores aplicados devem ser maiores que zero.';
    if (splits.some(split => split.method === 'Cash' && split.receivedAmount !== null && split.receivedAmount < split.amount)) return error.textContent = 'O valor recebido em dinheiro não pode ser menor que o aplicado.';
    const button = $('[data-submit-payment]'); button.disabled = true;
    try {
      await api(`service-orders/${idOf(state.order)}/payments`, { method: 'POST', body: JSON.stringify({ idempotencyKey: crypto.randomUUID(), splits }) });
      await refreshOrder(); closePayment(); notify('Pagamento confirmado e comanda atualizada.');
    } catch (exception) { error.textContent = exception.message; notify(exception.message, 'error'); }
    finally { button.disabled = false; }
  });

  load();
})();
