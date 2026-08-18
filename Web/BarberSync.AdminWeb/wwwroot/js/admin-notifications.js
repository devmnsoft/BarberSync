(() => {
  'use strict';

  const page = document.querySelector('[data-notifications-page]');
  const list = document.querySelector('[data-notifications-list]');
  const count = document.querySelector('[data-notification-count]');
  const readAll = document.querySelector('[data-read-all]');
  if (!page || !list || !count) return;

  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, char => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;'
  })[char]);
  const unwrap = payload => payload?.data?.items || payload?.data || payload?.items || payload || [];
  const isRead = item => item.isRead === true || item.read === true || String(item.status).toLowerCase() === 'read';
  const idOf = item => item.id || item.notificationId;
  const aiTypeLabels = {
    RecognitionSuggestionPending: 'Sugestão aguardando confirmação',
    RecognitionSuggestionOverdue: 'Sugestão pendente há muito tempo',
    AiProviderNotConfigured: 'Provider de IA não configurado',
    AiProviderUnavailable: 'Erro no provider de IA',
    RecognitionSuggestionConfirmed: 'Sugestão confirmada',
    RecognitionSuggestionRejected: 'Sugestão rejeitada'
  };
  let notifications = [];
  const filterValue = name => document.querySelector(`[data-notification-filter="${name}"]`)?.value.trim().toLowerCase() || '';

  async function api(path, options = {}) {
    const response = await fetch(`/AdminApi/${path}`, {
      ...options,
      headers: { Accept: 'application/json', 'Content-Type': 'application/json', ...(options.headers || {}) }
    });
    const payload = await response.json().catch(() => ({}));
    if (!response.ok) {
      const traceId = payload?.traceId || payload?.extensions?.traceId || response.headers.get('X-Trace-Id');
      throw new Error(`${payload?.detail || payload?.message || 'Não foi possível concluir a operação.'}${traceId ? ` Código de suporte: ${traceId}` : ''}`);
    }
    return payload;
  }

  function render() {
    const unread = notifications.filter(item => !isRead(item)).length;
    count.textContent = String(unread);
    document.querySelectorAll('[data-topbar-notification-count]').forEach(element => { element.textContent = String(unread); });
    readAll.disabled = unread === 0;
    const visible = notifications.filter(item => (!filterValue('priority') || String(item.priority || item.payload?.priority || '').toLowerCase() === filterValue('priority')) && (!filterValue('type') || String(item.type || item.payload?.type || '').toLowerCase().includes(filterValue('type'))) && (!filterValue('status') || (filterValue('status') === 'read') === isRead(item)) && (!filterValue('branch') || String(item.branchId || item.branch_id || '').toLowerCase().includes(filterValue('branch'))));
    if (!visible.length) {
      list.innerHTML = '<div class="empty-state"><strong>Nenhuma notificação</strong><p>Os alertas reais da sua unidade aparecerão aqui.</p></div>';
      return;
    }
    list.innerHTML = visible.map(item => {
      const id = escapeHtml(idOf(item));
      const read = isRead(item);
      return `<article class="saas7-log ${read ? '' : 'saas7-info'}">
        <strong>${escapeHtml(item.title || item.message || 'Notificação')}</strong>
        <p><span class="badge">${escapeHtml(item.priority || item.payload?.priority || 'Normal')}</span> ${escapeHtml(aiTypeLabels[item.type || item.payload?.type] || item.type || item.payload?.type || item.category || 'Operação')} • ${read ? 'Lida' : 'Não lida'}</p>
        ${(item.link || item.payload?.link) ? `<a class="btn btn-primary" href="${escapeHtml(item.link || item.payload.link)}">Abrir destino</a>` : ''}
        ${read || !id ? '' : `<button class="btn btn-light" type="button" data-read-id="${id}">Marcar como lida</button>`}
      </article>`;
    }).join('');
  }

  async function load() {
    list.setAttribute('aria-busy', 'true');
    list.innerHTML = '<div class="loading-state">Carregando notificações...</div>';
    try {
      const payload = await api('notifications');
      const data = unwrap(payload);
      notifications = Array.isArray(data) ? data : [];
      render();
    } catch (error) {
      list.innerHTML = `<div class="error-state"><strong>Não foi possível carregar as notificações.</strong><p>${escapeHtml(error.message)}</p><button class="btn btn-primary" type="button" data-retry> tentar novamente</button></div>`;
    } finally {
      list.removeAttribute('aria-busy');
    }
  }

  list.addEventListener('click', async event => {
    if (event.target.closest('[data-retry]')) return load();
    const button = event.target.closest('[data-read-id]');
    if (!button) return;
    button.disabled = true;
    try {
      await api(`notifications/${encodeURIComponent(button.dataset.readId)}`, { method: 'PUT', body: JSON.stringify({ isRead: true, readAt: new Date().toISOString() }) });
      await load();
    } catch (error) {
      button.disabled = false;
      window.BarberSyncToast?.show?.(error.message, 'error');
    }
  });

  readAll?.addEventListener('click', async () => {
    readAll.disabled = true;
    try {
      const unread = notifications.filter(item => !isRead(item) && idOf(item));
      await Promise.all(unread.map(item => api(`notifications/${encodeURIComponent(idOf(item))}`, { method: 'PUT', body: JSON.stringify({ isRead: true, readAt: new Date().toISOString() }) })));
      await load();
    } catch (error) {
      readAll.disabled = false;
      window.BarberSyncToast?.show?.(error.message, 'error');
    }
  });

  document.querySelectorAll('[data-notification-filter]').forEach(control => control.addEventListener(control.tagName === 'INPUT' ? 'input' : 'change', render));

  load();
})();
