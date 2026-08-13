(() => {
  'use strict';
  const q = (selector, root = document) => root.querySelector(selector);
  const qa = (selector, root = document) => [...root.querySelectorAll(selector)];
  if (!q('#posWorkspace')) return;

  const state = { orders: [], order: null, services: [], products: [], clients: [], professionals: [], appointments: [], kind: 'services', category: 'Todos', busy: false };
  const money = value => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(value || 0));
  const number = value => Number(String(value ?? '').replace(/\./g, '').replace(',', '.'));
  const esc = value => String(value ?? '').replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[char]));
  const unwrap = payload => payload?.data ?? payload;
  const list = payload => { const value = unwrap(payload); return Array.isArray(value) ? value : value?.items ?? []; };
  const toast = (method, message) => window.AdminToast?.[method]?.(message) ?? window.AdminToast?.show?.(message, method === 'showError' ? 'error' : 'success');

  async function api(path, options = {}) {
    const response = await fetch(`/AdminApi/${path}`, { ...options, headers: { Accept: 'application/json', ...(options.body ? { 'Content-Type': 'application/json' } : {}), ...options.headers } });
    const payload = await response.json().catch(() => null);
    if (!response.ok) {
      const error = new Error(payload?.detail || payload?.message || payload?.title || 'Não foi possível concluir a operação.');
      error.status = response.status; error.errors = payload?.errors; throw error;
    }
    return unwrap(payload);
  }
  const post = (path, body) => api(path, { method: 'POST', body: JSON.stringify(body) });
  const setError = (field, message = '') => { const host = q(`[data-error-for="${field}"]`); if (host) host.textContent = message; };
  function handle(error, field) { if (field) setError(field, error.message); if (error.status === 403) toast('forbidden', 'Você não tem permissão para esta operação.'); else if (error.status === 400 || error.status === 422) toast('validation', error.message); else toast('showError', error.message); }
  const nameOf = (items, id, fallback = '—') => items.find(item => String(item.id) === String(id))?.name ?? fallback;
  const editable = order => order && order.status === 'Open';
  const payable = order => order && ['Open', 'PartiallyPaid'].includes(order.status) && Number(order.balance) > 0;
  function showPane(name) { qa('[data-pos-pane]').forEach(button => button.classList.toggle('active', button.dataset.posPane === name)); qa('[data-pane]').forEach(pane => pane.classList.toggle('active', pane.dataset.pane === name)); }

  async function load() {
    q('#posWorkspace').setAttribute('aria-busy', 'true'); q('#posAlert').hidden = true;
    try {
      const results = await Promise.allSettled(['service-orders', 'services', 'products', 'clients', 'professionals', 'appointments'].map(path => api(path)));
      if (results[0].status === 'rejected') throw results[0].reason;
      [state.orders, state.services, state.products, state.clients, state.professionals, state.appointments] = results.map(result => result.status === 'fulfilled' ? list(result.value) : []);
      renderOrders(); renderCatalog();
      const selected = state.order && state.orders.some(order => order.id === state.order.id) ? state.order.id : state.orders.find(order => ['Open', 'PartiallyPaid'].includes(order.status))?.id ?? state.orders[0]?.id;
      if (selected) await selectOrder(selected); else { state.order = null; renderOrder(); }
    } catch (error) { q('#posAlert').hidden = false; q('#posAlert').textContent = error.message; handle(error); }
    finally { q('#posWorkspace').setAttribute('aria-busy', 'false'); }
  }
  async function selectOrder(id) { try { state.order = await api(`service-orders/${encodeURIComponent(id)}`); renderOrders(); renderOrder(); } catch (error) { handle(error); } }

  function renderOrders() {
    q('#posOrders').innerHTML = state.orders.length ? state.orders.map(order => `<button type="button" class="pos-order-pill ${state.order?.id === order.id ? 'active' : ''}" data-order-id="${esc(order.id)}"><span>#${esc(order.number)}</span><strong>${money(order.balance)}</strong><small>${esc(order.status)}</small></button>`).join('') : '<div class="pos-empty"><strong>Nenhuma comanda encontrada</strong><span>Abra uma comanda para iniciar a venda.</span></div>';
  }
  function renderCatalog() {
    const source = state.kind === 'services' ? state.services : state.products;
    const categories = ['Todos', ...new Set(source.map(item => item.category || item.categoryName).filter(Boolean))];
    q('#posCategories').innerHTML = categories.map(category => `<button type="button" class="${category === state.category ? 'active' : ''}" data-category="${esc(category)}">${esc(category)}</button>`).join('');
    const term = q('#posCatalogSearch').value.trim().toLocaleLowerCase('pt-BR');
    const filtered = source.filter(item => (!term || String(item.name || item.description).toLocaleLowerCase('pt-BR').includes(term)) && (state.category === 'Todos' || (item.category || item.categoryName) === state.category));
    q('#posCatalog').innerHTML = filtered.map(item => { const stock = Number(item.stockQuantity ?? item.stock ?? item.availableStock ?? 0); const unavailable = state.kind === 'products' && stock <= 0; return `<button type="button" class="pos-product-card" data-catalog-id="${esc(item.id)}" ${unavailable || !editable(state.order) ? 'disabled' : ''}><span class="pos-product-icon">${state.kind === 'services' ? '✂' : '▣'}</span><span><strong>${esc(item.name || item.description)}</strong><small>${state.kind === 'services' ? `${esc(item.durationMinutes || item.duration || '—')} min` : `${stock} em estoque`}</small></span><b>${money(item.price || item.salePrice || item.amount)}</b>${unavailable ? '<em>Sem estoque</em>' : ''}</button>`; }).join('') || '<div class="pos-empty"><strong>Nenhum item encontrado</strong><span>Ajuste a busca ou os filtros.</span></div>';
  }
  function renderOrder() {
    const order = state.order; const canEdit = editable(order); const canPay = payable(order);
    q('#posClient').textContent = order ? nameOf(state.clients, order.clientId, `Cliente ${String(order.clientId).slice(0, 8)}`) : 'Selecione uma comanda';
    q('#posAppointment').textContent = order?.appointmentId ? `#${String(order.appointmentId).slice(0, 8)}` : 'Não vinculado';
    q('#posStatus').textContent = order?.status || '—'; q('#posStatus').dataset.status = order?.status || '';
    q('#posItemCount').textContent = `${order?.items?.length || 0} itens`;
    const fields = { sumSubtotal: order?.subtotal, sumDiscount: order?.discount, sumSurcharge: order?.surcharge, sumTotal: order?.total, sumPaid: order?.paid, sumBalance: order?.balance };
    Object.entries(fields).forEach(([id, value]) => { q(`#${id}`).textContent = money(value); });
    q('#posItems').innerHTML = order?.items?.length ? order.items.map(item => `<article class="pos-cart-item"><span class="pos-item-type">${item.type === 'Product' ? '▣' : '✂'}</span><div><strong>${esc(item.description)}</strong><small>${esc(nameOf(state.professionals, item.professionalId, item.professionalId ? 'Profissional' : 'Sem profissional'))}</small><span>${money(item.unitPrice)}${Number(item.discount) ? ` · desconto ${money(item.discount)}` : ''}</span><div class="pos-quantity" aria-label="Quantidade de ${esc(item.description)}"><button type="button" data-quantity-action="decrease" data-item-id="${esc(item.id)}" ${canEdit && Number(item.quantity) > 1 ? '' : 'disabled'} aria-label="Diminuir quantidade">−</button><output>${esc(item.quantity)}</output><button type="button" data-quantity-action="increase" data-item-id="${esc(item.id)}" ${canEdit ? '' : 'disabled'} aria-label="Aumentar quantidade">+</button></div></div><b>${money(item.total)}</b><button type="button" data-remove-item="${esc(item.id)}" ${canEdit ? '' : 'disabled'} aria-label="Remover ${esc(item.description)}">×</button></article>`).join('') : '<div class="pos-empty"><strong>Comanda sem itens</strong><span>Escolha um serviço ou produto no catálogo.</span></div>';
    q('#posPay').disabled = !canPay; q('#posPay').textContent = order?.status === 'PartiallyPaid' ? 'Complementar pagamento' : order?.status === 'Paid' ? 'Pagamento concluído' : 'Receber pagamento';
    q('#posReadonly').hidden = !order || canEdit; qa('#discountForm input, #discountForm button, #couponForm input, #couponForm button, #cashbackForm input, #cashbackForm button').forEach(control => { control.disabled = !canEdit; });
    q('#posAdjustments').innerHTML = Number(order?.discount) > 0 ? `<span>Descontos aplicados: <strong>${money(order.discount)}</strong></span>` : '<span>Nenhum desconto aplicado.</span>';
    q('#posPayments').innerHTML = order?.payments?.length ? order.payments.map(payment => `<span><strong>${esc(payment.method)}</strong> · ${money(payment.amount)} · ${esc(payment.status)}${payment.status === 'Paid' ? ` <button class="pos-refund" type="button" data-refund-payment="${esc(payment.id)}">Estornar</button>` : ''}</span>`).join('') : '<span>Nenhum pagamento registrado.</span>';
    q('#posReceipt').disabled = !order || !order.items?.length;
    renderCatalog();
  }

  async function mutate(path, body, success, field) {
    if (state.busy) return; state.busy = true;
    try { await post(path, body); await selectOrder(state.order.id); toast('updated', success); }
    catch (error) { handle(error, field); } finally { state.busy = false; }
  }
  async function addCatalogItem(id) {
    if (!editable(state.order)) return;
    const isService = state.kind === 'services'; const item = (isService ? state.services : state.products).find(entry => String(entry.id) === String(id));
    let professionalId = null;
    if (isService) { professionalId = state.professionals[0]?.id; if (!professionalId) return toast('validation', 'Cadastre um profissional antes de adicionar serviços.'); }
    await mutate(`service-orders/${state.order.id}/items/${isService ? 'services' : 'products'}`, isService ? { serviceId: item.id, professionalId, quantity: 1 } : { productId: item.id, quantity: 1, professionalId }, `${item.name} adicionado à comanda.`);
  }
  function splitRow(amount = '') { return `<div class="pos-split"><label>Forma<select data-split-method><option value="Pix">PIX</option><option value="DebitCard">Débito</option><option value="CreditCard">Crédito</option><option value="Cash">Dinheiro</option></select></label><label>Valor<input data-split-amount inputmode="decimal" value="${esc(amount)}" /></label><label class="received" hidden>Valor recebido<input data-split-received inputmode="decimal" /></label><button type="button" data-remove-split aria-label="Remover forma">×</button><small class="field-error"></small></div>`; }
  function openPayment() { q('#paymentSplits').innerHTML = splitRow(Number(state.order.balance).toFixed(2).replace('.', ',')); q('#paymentBalance').textContent = money(state.order.balance); q('#idempotencyKey').value = crypto.randomUUID(); q('#paymentDrawer').hidden = false; q('[data-split-method]').focus(); }
  function closeDrawers() { qa('.pos-drawer-backdrop').forEach(drawer => { drawer.hidden = true; }); }

  q('#posRefresh').addEventListener('click', load); q('#posCatalogSearch').addEventListener('input', renderCatalog);
  q('#posOrders').addEventListener('click', event => { const target = event.target.closest('[data-order-id]'); if (target) selectOrder(target.dataset.orderId); });
  q('#posCategories').addEventListener('click', event => { const target = event.target.closest('[data-category]'); if (target) { state.category = target.dataset.category; renderCatalog(); } });
  qa('[data-catalog-kind]').forEach(button => button.addEventListener('click', () => { state.kind = button.dataset.catalogKind; state.category = 'Todos'; qa('[data-catalog-kind]').forEach(item => item.classList.toggle('active', item === button)); renderCatalog(); }));
  q('#posCatalog').addEventListener('click', event => { const target = event.target.closest('[data-catalog-id]'); if (target) addCatalogItem(target.dataset.catalogId); });
  q('#posItems').addEventListener('click', async event => { const target = event.target.closest('[data-remove-item]'); if (!target) return; const accepted = await window.BarberSyncConfirm?.ask?.({ title: 'Remover item?', message: 'O total da comanda será recalculado.', confirmText: 'Remover' }); if (accepted === false) return; try { await api(`service-orders/${state.order.id}/items/${target.dataset.removeItem}`, { method: 'DELETE' }); await selectOrder(state.order.id); toast('updated', 'Item removido.'); } catch (error) { handle(error); } });
  q('#posItems').addEventListener('click', async event => { const target = event.target.closest('[data-quantity-action]'); if (!target || state.busy) return; const item = state.order?.items?.find(entry => String(entry.id) === target.dataset.itemId); if (!item) return; const quantity = Number(item.quantity) + (target.dataset.quantityAction === 'increase' ? 1 : -1); if (quantity <= 0) return; state.busy = true; qa('[data-quantity-action]').forEach(button => { button.disabled = true; }); try { state.order = await api(`service-orders/${state.order.id}/items/${item.id}`, { method: 'PUT', body: JSON.stringify({ quantity, discount: Number(item.discount || 0), professionalId: item.professionalId || null }) }); renderOrder(); toast('updated', 'Quantidade atualizada.'); } catch (error) { handle(error); renderOrder(); } finally { state.busy = false; } });
  q('#discountForm').addEventListener('submit', event => { event.preventDefault(); const amount = number(q('#discountAmount').value); const reason = q('#discountReason').value.trim(); setError('discountAmount'); if (!Number.isFinite(amount) || amount <= 0) return setError('discountAmount', 'Informe um desconto maior que zero.'); if (amount > Number(state.order.subtotal)) return setError('discountAmount', 'O desconto não pode superar o subtotal.'); if (!reason) return setError('discountAmount', 'Informe o motivo do desconto.'); mutate(`service-orders/${state.order.id}/discount`, { amount, reason }, 'Desconto aplicado.', 'discountAmount'); });
  q('#couponForm').addEventListener('submit', event => { event.preventDefault(); const code = q('#couponCode').value.trim(); setError('couponCode'); if (!code) return setError('couponCode', 'Informe o código do cupom.'); mutate(`service-orders/${state.order.id}/coupon`, { code }, 'Cupom aplicado.', 'couponCode'); });
  q('#cashbackForm').addEventListener('submit', event => { event.preventDefault(); const amount = number(q('#cashbackAmount').value); setError('cashbackAmount'); if (!Number.isFinite(amount) || amount <= 0) return setError('cashbackAmount', 'Informe um valor maior que zero.'); if (amount > Number(state.order.balance)) return setError('cashbackAmount', 'O cashback não pode superar o saldo da comanda.'); mutate(`service-orders/${state.order.id}/cashback`, { amount }, 'Cashback aplicado.', 'cashbackAmount'); });
  q('#posPay').addEventListener('click', openPayment); q('#addSplit').addEventListener('click', () => q('#paymentSplits').insertAdjacentHTML('beforeend', splitRow()));
  q('#posReceipt').addEventListener('click', () => window.print());
  q('#posPayments').addEventListener('click', async event => { const target = event.target.closest('[data-refund-payment]'); if (!target) return; const accepted = await window.BarberSyncConfirm?.ask?.({ title: 'Estornar pagamento?', message: 'Estoque, comissão, fidelidade e caixa serão revertidos. Esta ação fica registrada na auditoria.', confirmText: 'Estornar' }); if (accepted === false) return; const reason = window.prompt('Informe o motivo do estorno:')?.trim(); if (!reason) return toast('validation', 'O motivo do estorno é obrigatório.'); try { await post(`service-orders/payments/${target.dataset.refundPayment}/refund`, { reason }); await selectOrder(state.order.id); await load(); toast('updated', 'Pagamento estornado e efeitos operacionais revertidos.'); } catch (error) { handle(error); } });
  q('#paymentSplits').addEventListener('change', event => { if (event.target.matches('[data-split-method]')) q('.received', event.target.closest('.pos-split')).hidden = event.target.value !== 'Cash'; });
  q('#paymentSplits').addEventListener('click', event => { const target = event.target.closest('[data-remove-split]'); if (target) target.closest('.pos-split').remove(); });
  q('#paymentForm').addEventListener('submit', async event => { event.preventDefault(); if (state.busy) return; setError('splits'); const rows = qa('.pos-split'); if (!rows.length) return setError('splits', 'Adicione ao menos uma forma de pagamento.'); const splits = rows.map(row => { const method = q('[data-split-method]', row).value; const amount = number(q('[data-split-amount]', row).value); const received = number(q('[data-split-received]', row).value); return { method, amount, ...(method === 'Cash' && Number.isFinite(received) ? { receivedAmount: received } : {}) }; }); if (splits.some(split => !Number.isFinite(split.amount) || split.amount <= 0)) return setError('splits', 'Todos os valores devem ser maiores que zero.'); if (splits.reduce((sum, split) => sum + split.amount, 0) > Number(state.order.balance) + .001) return setError('splits', 'A soma não pode superar o saldo da comanda.'); if (splits.some(split => split.method === 'Cash' && split.receivedAmount != null && split.receivedAmount < split.amount)) return setError('splits', 'O valor recebido não pode ser menor que o valor aplicado.'); state.busy = true; const button = q('#confirmPayment'); button.disabled = true; button.textContent = 'Processando…'; try { const result = await post(`service-orders/${state.order.id}/payments`, { idempotencyKey: q('#idempotencyKey').value, splits, note: q('#paymentNote').value.trim() || null }); await selectOrder(state.order.id); closeDrawers(); toast('created', result.replayed ? 'Pagamento já processado; comanda atualizada.' : 'Pagamento registrado.'); } catch (error) { handle(error, 'splits'); } finally { state.busy = false; button.disabled = false; button.textContent = 'Confirmar recebimento'; } });
  q('#posOpenOrder').addEventListener('click', () => { q('#openClient').innerHTML = '<option value="">Selecione</option>' + state.clients.map(client => `<option value="${esc(client.id)}">${esc(client.name)}</option>`).join(''); q('#openAppointment').innerHTML = '<option value="">Sem vínculo</option>' + state.appointments.map(item => `<option value="${esc(item.id)}">${esc(item.clientName || item.id)} · ${esc(item.scheduledStart || '')}</option>`).join(''); q('#openOrderDrawer').hidden = false; });
  q('#openOrderForm').addEventListener('submit', async event => { event.preventDefault(); const clientId = q('#openClient').value; if (!clientId) return setError('openClient', 'Selecione o cliente.'); const button = event.submitter; button.disabled = true; try { const order = await post('service-orders/open', { clientId, appointmentId: q('#openAppointment').value || null, notes: q('#openNotes').value.trim() || null }); closeDrawers(); await load(); await selectOrder(order.id); toast('created', 'Comanda aberta.'); } catch (error) { handle(error, 'openClient'); } finally { button.disabled = false; } });
  qa('[data-pos-pane]').forEach(button => button.addEventListener('click', () => showPane(button.dataset.posPane)));
  document.addEventListener('click', event => { if (event.target.matches('[data-close-drawer]') || event.target.classList.contains('pos-drawer-backdrop')) closeDrawers(); });
  document.addEventListener('keydown', event => { if (event.key === 'Escape') closeDrawers(); if (event.key === 'F2') { event.preventDefault(); q('#posCatalogSearch').focus(); } });
  load();
})();
