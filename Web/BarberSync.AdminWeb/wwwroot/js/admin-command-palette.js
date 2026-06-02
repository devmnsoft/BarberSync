(() => {
  const actions = [
    { title: 'Novo cliente', hint: 'Abre o CRUD de Clientes', icon: '👥', href: '/Admin/Clients', run: () => window.openCreateModal?.('Clients') },
    { title: 'Novo agendamento', hint: 'Cria um horário na agenda', icon: '📅', href: '/Admin/Appointments', run: () => window.openCreateModal?.('Appointments') },
    { title: 'Abrir comanda', hint: 'Inicia uma comanda demo', icon: '🧾', href: '/Admin/ServiceOrders', run: () => window.openCreateModal?.('ServiceOrders') },
    { title: 'Cadastrar serviço', hint: 'Configura preço e canais', icon: '✂️', href: '/Admin/Services', run: () => window.openCreateModal?.('Services') },
    { title: 'Entrada de estoque', hint: 'Simula reposição de produto', icon: '📦', href: '/Admin/Stock', run: () => window.openCreateModal?.('Stock') },
    { title: 'Criar campanha', hint: 'Ativa jornada de retorno', icon: '🎯', href: '/Admin/Campaigns', run: () => window.openCreateModal?.('Campaigns') },
    { title: 'Abrir Copilot', hint: 'Perguntas e sugestões acionáveis', icon: '🤖', href: '/Admin/Copilot' },
    { title: 'Tour Demo', hint: 'Inicia o roteiro guiado BarberSync Demo Experience 2.0', icon: '🧭', href: '/Admin/Dashboard', run: () => window.BarberSyncDemoTour?.start?.() },
    { title: 'Abrir Totem', hint: 'Fluxo de autoatendimento', icon: '🖥️', href: 'http://localhost:8083/Kiosk/Services', external: true },
    { title: 'Ver roteiro de demo', hint: 'Checklist e narrativa comercial', icon: '📋', href: '/Admin/Help' }
  ];
  let active = 0;
  const els = () => ({ modal: document.getElementById('AdminCommandPalette'), scrim: document.querySelector('.admin-command-scrim'), input: document.getElementById('AdminCommandSearch'), list: document.querySelector('[data-command-list]') });
  const filtered = () => { const q = (els().input?.value || '').toLowerCase(); return actions.filter(a => `${a.title} ${a.hint}`.toLowerCase().includes(q)); };
  function render() { const { list } = els(); if (!list) return; const items = filtered(); active = Math.min(active, Math.max(items.length - 1, 0)); list.innerHTML = items.map((a, i) => `<button type="button" class="command-item ${i === active ? 'is-active' : ''}" data-command-index="${i}"><span>${a.icon}</span><strong>${a.title}</strong><small>${a.hint}</small></button>`).join('') || '<div class="empty-state-mini">Nenhuma ação encontrada.</div>'; }
  function open() { const { modal, scrim, input } = els(); if (!modal) return; modal.hidden = false; scrim.hidden = false; modal.setAttribute('aria-hidden','false'); active = 0; render(); setTimeout(() => input?.focus(), 30); }
  function close() { const { modal, scrim } = els(); if (!modal) return; modal.hidden = true; scrim.hidden = true; modal.setAttribute('aria-hidden','true'); }
  function execute(index = active) { const item = filtered()[index]; if (!item) return; close(); if (item.run && location.pathname.toLowerCase() === new URL(item.href, location.origin).pathname.toLowerCase()) { item.run(); window.AdminToast?.showInfo?.(`Ação aberta: ${item.title}.`); return; } if (item.external) window.open(item.href, '_blank', 'noopener'); else location.href = item.href; }
  document.addEventListener('DOMContentLoaded', () => { render(); document.querySelectorAll('[data-command-open], [data-open-command-palette]').forEach(b => b.addEventListener('click', open)); els().input?.addEventListener('input', () => { active = 0; render(); }); });
  document.addEventListener('click', e => { if (e.target.closest('[data-command-close]')) close(); const item = e.target.closest('[data-command-index]'); if (item) execute(Number(item.dataset.commandIndex)); });
  document.addEventListener('keydown', e => { if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') { e.preventDefault(); open(); } if (els().modal?.hidden === false) { if (e.key === 'Escape') close(); if (e.key === 'ArrowDown') { e.preventDefault(); active = Math.min(active + 1, filtered().length - 1); render(); } if (e.key === 'ArrowUp') { e.preventDefault(); active = Math.max(active - 1, 0); render(); } if (e.key === 'Enter') { e.preventDefault(); execute(); } } });
  window.AdminCommandPalette = { open, close, execute };
})();
