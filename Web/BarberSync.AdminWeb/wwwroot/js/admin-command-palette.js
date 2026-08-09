(() => {
  'use strict';
  const quickActions = [
    { title: 'Novo cliente', hint: 'Cadastrar cliente', group: 'Ações', href: '/Admin/Clients?new=true' },
    { title: 'Novo agendamento', hint: 'Reservar um horário', group: 'Ações', href: '/Admin/Appointments?new=true' },
    { title: 'Nova comanda', hint: 'Iniciar uma venda', group: 'Ações', href: '/Admin/ServiceOrders?new=true' },
    { title: 'Abrir caixa', hint: 'Iniciar operação financeira', group: 'Ações', href: '/Admin/Cash?action=open' },
    { title: 'Movimentar estoque', hint: 'Entrada, saída ou ajuste', group: 'Ações', href: '/Admin/Stock?action=movement' }
  ];
  let results = quickActions, active = 0, timer;
  const elements = () => ({ modal: document.getElementById('AdminCommandPalette'), scrim: document.querySelector('.admin-command-scrim'), input: document.getElementById('AdminCommandSearch'), list: document.querySelector('[data-command-list]') });
  const escape = value => String(value ?? '').replace(/[&<>'"]/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[char]));
  function render() {
    const host = elements().list; if (!host) return;
    active = Math.min(active, Math.max(results.length - 1, 0));
    let group = '';
    host.innerHTML = results.map((item, index) => {
      const heading = item.group !== group ? `<p class="command-group">${escape(item.group)}</p>` : ''; group = item.group;
      return `${heading}<button type="button" class="command-item ${index === active ? 'is-active' : ''}" data-command-index="${index}"><span class="nav-icon">${escape(item.icon || item.group?.slice(0,2).toUpperCase())}</span><strong>${escape(item.title)}</strong><small>${escape(item.hint)}</small></button>`;
    }).join('') || '<div class="empty-state-mini">Nenhum resultado encontrado.</div>';
  }
  async function search(query) {
    if (query.length < 2) { results = quickActions; render(); return; }
    try {
      const response = await fetch(`/AdminApi/search?q=${encodeURIComponent(query)}`, { headers: { Accept: 'application/json' } });
      if (!response.ok) throw new Error();
      const payload = await response.json();
      const groups = payload.data || payload;
      results = Object.entries(groups).flatMap(([group, items]) => (Array.isArray(items) ? items : []).map(item => ({ group, title: item.title || item.name || item.code, hint: item.subtitle || item.phone || item.description, href: item.url || item.href })));
    } catch { results = []; }
    active = 0; render();
  }
  function open() { const { modal, scrim, input } = elements(); if (!modal) return; modal.hidden = false; scrim.hidden = false; modal.setAttribute('aria-hidden', 'false'); results = quickActions; active = 0; render(); setTimeout(() => input.focus(), 30); }
  function close() { const { modal, scrim } = elements(); if (!modal) return; modal.hidden = true; scrim.hidden = true; modal.setAttribute('aria-hidden', 'true'); }
  function execute(index = active) { const item = results[index]; if (item?.href) location.assign(item.href); }
  document.addEventListener('DOMContentLoaded', () => { render(); document.querySelectorAll('[data-command-open]').forEach(button => button.addEventListener('click', open)); elements().input?.addEventListener('input', event => { clearTimeout(timer); timer = setTimeout(() => search(event.target.value.trim()), 250); }); });
  document.addEventListener('click', event => { if (event.target.closest('[data-command-close]')) close(); const item = event.target.closest('[data-command-index]'); if (item) execute(Number(item.dataset.commandIndex)); });
  document.addEventListener('keydown', event => { if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') { event.preventDefault(); open(); } if (!elements().modal?.hidden) { if (event.key === 'Escape') close(); if (event.key === 'ArrowDown') { event.preventDefault(); active = Math.min(active + 1, results.length - 1); render(); } if (event.key === 'ArrowUp') { event.preventDefault(); active = Math.max(active - 1, 0); render(); } if (event.key === 'Enter') { event.preventDefault(); execute(); } } });
})();
