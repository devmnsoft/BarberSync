(() => {
  'use strict';
  const root = document.querySelector('[data-clients-page]'); if (!root) return;
  const $ = selector => root.querySelector(selector);
  const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
  const normalize = payload => Array.isArray(payload) ? payload : payload?.items || payload?.data || [];
  let clients = [];
  function render() {
    const term = $('#ClientsSearch').value.trim().toLocaleLowerCase('pt-BR');
    const rows = clients.filter(client => JSON.stringify(client).toLocaleLowerCase('pt-BR').includes(term));
    $('[data-clients-rows]').innerHTML = rows.length ? rows.map(client => `<tr><td><strong>${escapeHtml(client.name || client.fullName || 'Sem nome')}</strong></td><td>${escapeHtml(client.phone || client.email || 'Não informado')}</td><td><span class="badge badge-success">${escapeHtml(client.status || 'Ativo')}</span></td><td><a class="btn btn-secondary bs-touch-target" href="/Admin/Clients/${encodeURIComponent(client.id)}">Ver Cliente 360</a></td></tr>`).join('') : '<tr><td colspan="4"><div class="bs-premium-empty">Nenhum cliente encontrado.</div></td></tr>';
  }
  async function load() {
    $('[data-clients-error]').hidden = true;
    try {
      const response = await fetch('/AdminApi/clients', { headers: { Accept: 'application/json' } });
      const payload = await response.json().catch(() => ({}));
      if (!response.ok) throw Object.assign(new Error(payload.detail || payload.title || 'A API recusou a consulta.'), { traceId: payload.traceId });
      clients = normalize(payload); render();
    } catch (error) {
      $('[data-clients-rows]').innerHTML = '';
      $('[data-clients-error-detail]').textContent = error.message || 'Verifique a API e o banco de dados.';
      $('[data-clients-trace]').textContent = error.traceId || 'não informado'; $('[data-clients-error]').hidden = false;
    }
  }
  function modal(open) { $('[data-clients-modal]').hidden = !open; }
  $('[data-clients-new]').addEventListener('click', () => modal(true));
  root.querySelectorAll('[data-clients-close]').forEach(button => button.addEventListener('click', () => modal(false)));
  $('[data-clients-form]').addEventListener('submit', async event => {
    event.preventDefault(); const button = event.currentTarget.querySelector('[type=submit]'); button.disabled = true; button.textContent = 'Salvando…';
    try {
      const body = Object.fromEntries(new FormData(event.currentTarget));
      const response = await fetch('/AdminApi/clients', { method: 'POST', headers: { Accept: 'application/json', 'Content-Type': 'application/json' }, body: JSON.stringify(body) });
      const payload = await response.json().catch(() => ({}));
      if (!response.ok) throw new Error(payload.detail || payload.title || payload.errors?.[0]?.message || 'Não foi possível salvar o cliente.');
      event.currentTarget.reset(); modal(false); window.AdminToast?.show?.('Cliente cadastrado com sucesso.', 'success'); await load();
    } catch (error) { $('[data-clients-form-error]').hidden = false; $('[data-clients-form-error]').textContent = error.message; }
    finally { button.disabled = false; button.textContent = 'Salvar cliente'; }
  });
  $('#ClientsSearch').addEventListener('input', render); $('[data-clients-refresh]').addEventListener('click', load); $('[data-clients-retry]').addEventListener('click', load); load();
})();
