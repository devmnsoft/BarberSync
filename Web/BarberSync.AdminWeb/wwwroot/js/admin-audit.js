(() => {
  'use strict';
  const root = document.querySelector('[data-audit-page]');
  if (!root) return;

  const state = { events: [], filtered: [] };
  const one = selector => document.querySelector(selector);
  const all = selector => [...document.querySelectorAll(selector)];
  const value = name => one(`[data-audit-filter="${name}"]`)?.value.trim().toLowerCase() || '';
  const field = (event, ...names) => names.map(name => event?.[name]).find(item => item !== undefined && item !== null) ?? '';
  const text = value => String(value ?? '');
  const escapeHtml = value => text(value).replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]);
  const eventDate = event => new Date(field(event, 'createdAt', 'occurredAt', 'timestamp'));
  const validDate = date => !Number.isNaN(date.getTime());
  const formatDate = event => validDate(eventDate(event)) ? eventDate(event).toLocaleString('pt-BR') : 'Data não informada';
  const traceId = event => text(field(event, 'traceId', 'trace_id', 'correlationId'));
  const aiActions = new Set(['RecognitionSuggestionCreated', 'RecognitionSuggestionConfirmed', 'RecognitionSuggestionRejected', 'RecognitionSuggestionConvertedToOrder', 'AiProviderTested', 'AiProviderUnavailable']);

  function toggle(selector, visible) { const element = one(selector); if (element) element.hidden = !visible; }
  function setOptions(name, values) {
    const select = one(`[data-audit-filter="${name}"]`);
    const current = select.value;
    select.innerHTML = `<option value="">${name === 'module' ? 'Todos' : 'Todas'}</option>` +
      [...new Set(values.filter(Boolean))].sort().map(item => `<option>${escapeHtml(item)}</option>`).join('');
    select.value = current;
  }

  function applyFilters() {
    const from = value('from') ? new Date(`${value('from')}T00:00:00`) : null;
    const to = value('to') ? new Date(`${value('to')}T23:59:59.999`) : null;
    state.filtered = state.events.filter(event => {
      const date = eventDate(event);
      return (!value('module') || text(field(event, 'module')).toLowerCase() === value('module'))
        && (!value('action') || text(field(event, 'action', 'operation')).toLowerCase() === value('action'))
        && (!value('severity') || text(field(event, 'severity') || field(event, 'metadata')?.severity || 'Normal').toLowerCase() === value('severity'))
        && (!value('user') || text(field(event, 'userName', 'userId', 'user')).toLowerCase().includes(value('user')))
        && (!value('entity') || `${field(event, 'entityName', 'entity')} ${field(event, 'entityId')}`.toLowerCase().includes(value('entity')))
        && (!value('trace') || traceId(event).toLowerCase().includes(value('trace')))
        && (!from || (validDate(date) && date >= from)) && (!to || (validDate(date) && date <= to));
    });
    render();
  }

  function render() {
    one('[data-audit-count]').textContent = state.filtered.length.toLocaleString('pt-BR');
    one('[data-audit-last]').textContent = state.filtered.length ? formatDate(state.filtered[0]) : '—';
    one('[data-export-audit]').disabled = state.filtered.length === 0;
    toggle('[data-audit-empty]', state.filtered.length === 0);
    toggle('[data-audit-table]', state.filtered.length > 0);
    one('[data-audit-rows]').innerHTML = state.filtered.map((event, index) => {
      const module = text(field(event, 'module')) || 'Sistema';
      const action = text(field(event, 'action', 'operation')) || 'Evento';
      const user = text(field(event, 'userName', 'userId', 'user')) || 'Sistema';
      const entity = text(field(event, 'entityName', 'entity')) || '—';
      const trace = traceId(event);
      return `<tr><td>${escapeHtml(formatDate(event))}</td><td><span class="audit-badge">${escapeHtml(aiActions.has(action) ? 'IA' : module)}</span></td><td><strong>${escapeHtml(action)}</strong><small>${escapeHtml(field(event, 'description'))}</small></td><td>${escapeHtml(user)}</td><td>${escapeHtml(entity)}</td><td><code>${escapeHtml(trace || '—')}</code></td><td><button class="btn btn-light btn-small" type="button" data-audit-open="${index}">Detalhes</button></td></tr>`;
    }).join('');
  }

  async function load() {
    toggle('[data-audit-loading]', true); toggle('[data-audit-error]', false); toggle('[data-audit-empty]', false); toggle('[data-audit-table]', false);
    const result = await window.AdminApiClient.adminGet('/AdminApi/audit/events');
    toggle('[data-audit-loading]', false);
    if (!result?.success || !Array.isArray(result.data)) {
      const trace = result?.traceId ? ` TraceId: ${result.traceId}.` : '';
      one('[data-audit-error-message]').textContent = `${result?.message || 'A API de auditoria não respondeu.'}${trace}`;
      toggle('[data-audit-error]', true); return;
    }
    state.events = result.data.sort((a, b) => eventDate(b) - eventDate(a));
    setOptions('module', state.events.map(event => text(field(event, 'module'))));
    setOptions('action', state.events.map(event => text(field(event, 'action', 'operation'))));
    applyFilters();
  }

  function showDetail(index) {
    const event = state.filtered[index]; if (!event) return;
    const trace = traceId(event);
    one('[data-audit-detail]').innerHTML = `<div class="detail-grid"><div><span>Data</span><strong>${escapeHtml(formatDate(event))}</strong></div><div><span>Módulo</span><strong>${escapeHtml(field(event, 'module') || 'Sistema')}</strong></div><div><span>Ação</span><strong>${escapeHtml(field(event, 'action', 'operation') || 'Evento')}</strong></div><div><span>Usuário</span><strong>${escapeHtml(field(event, 'userName', 'userId', 'user') || 'Sistema')}</strong></div><div><span>Entidade</span><strong>${escapeHtml(field(event, 'entityName', 'entity') || '—')}</strong></div><div><span>Identificador</span><strong>${escapeHtml(field(event, 'entityId') || '—')}</strong></div></div><h3>Descrição</h3><p>${escapeHtml(field(event, 'description') || 'Sem descrição adicional.')}</p><h3>TraceId</h3><div class="audit-trace"><code>${escapeHtml(trace || 'Não informado')}</code>${trace ? '<button class="btn btn-light" type="button" data-copy-trace>Copiar</button>' : ''}</div><h3>Antes</h3><pre>${escapeHtml(JSON.stringify(field(event, 'beforeData', 'before_data') || {}, null, 2))}</pre><h3>Depois</h3><pre>${escapeHtml(JSON.stringify(field(event, 'afterData', 'after_data') || {}, null, 2))}</pre><h3>Metadados</h3><pre>${escapeHtml(JSON.stringify(field(event, 'metadata', 'details', 'payload') || {}, null, 2))}</pre>${field(event, 'entityId') ? `<a class="btn btn-primary" href="/Admin/${encodeURIComponent(field(event, 'entityName', 'entity'))}?id=${encodeURIComponent(field(event, 'entityId'))}">Abrir entidade auditada</a>` : ''}`;
    const drawer = one('[data-audit-drawer]'); drawer.hidden = false; document.body.classList.add('drawer-open');
    one('[data-copy-trace]')?.addEventListener('click', async () => { await navigator.clipboard.writeText(trace); window.AdminToast?.showSuccess?.('TraceId copiado.'); });
  }

  function exportCsv() {
    const columns = ['data', 'modulo', 'acao', 'usuario', 'entidade', 'entidadeId', 'traceId', 'descricao'];
    const quote = item => `"${text(item).replace(/"/g, '""')}"`;
    const rows = state.filtered.map(event => [formatDate(event), field(event, 'module'), field(event, 'action', 'operation'), field(event, 'userName', 'userId', 'user'), field(event, 'entityName', 'entity'), field(event, 'entityId'), traceId(event), field(event, 'description')].map(quote).join(','));
    const blob = new Blob([`\uFEFF${columns.join(',')}\n${rows.join('\n')}`], { type: 'text/csv;charset=utf-8' });
    const link = Object.assign(document.createElement('a'), { href: URL.createObjectURL(blob), download: `auditoria-${new Date().toISOString().slice(0, 10)}.csv` });
    link.click(); URL.revokeObjectURL(link.href);
  }

  all('[data-audit-filter]').forEach(control => control.addEventListener(control.tagName === 'INPUT' ? 'input' : 'change', applyFilters));
  all('[data-audit-refresh]').forEach(button => button.addEventListener('click', load));
  one('[data-audit-clear]').addEventListener('click', () => { all('[data-audit-filter]').forEach(control => { control.value = ''; }); applyFilters(); });
  one('[data-export-audit]').addEventListener('click', exportCsv);
  one('[data-audit-rows]').addEventListener('click', event => { const button = event.target.closest('[data-audit-open]'); if (button) showDetail(Number(button.dataset.auditOpen)); });
  all('[data-audit-close]').forEach(button => button.addEventListener('click', () => { one('[data-audit-drawer]').hidden = true; document.body.classList.remove('drawer-open'); }));
  load();
})();
