(() => {
  if (!location.pathname.toLowerCase().includes('/admin/cash')) return;

  const money = value => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(value || 0));
  const buttons = [...document.querySelectorAll('[data-cash-action]')];
  const feedback = document.querySelector('#cashFeedback');
  const status = document.querySelector('#cashStatus');
  const rows = document.querySelector('#cashRows');
  const kpis = document.querySelector('#cashKpis');

  const showUnavailable = message => {
    status.textContent = 'Indisponível';
    feedback.classList.add('bs-empty');
    feedback.innerHTML = `<strong>Não foi possível consultar o caixa</strong><span>${message || 'Tente novamente em instantes.'}</span><button class="btn btn-secondary" type="button" data-cash-retry>Tentar novamente</button>`;
    rows.innerHTML = '<tr><td colspan="4">Nenhuma movimentação disponível.</td></tr>';
  };

  const render = data => {
    const current = data?.current || data;
    if (!current) return showUnavailable('A API não retornou um caixa para a unidade ativa.');
    const isOpen = String(current.status || '').toLowerCase() === 'open' || String(current.status || '').toLowerCase() === 'aberto';
    status.textContent = isOpen ? 'Aberto' : 'Fechado';
    status.className = `badge ${isOpen ? 'badge-success' : 'badge-info'}`;
    buttons.forEach(button => { button.disabled = button.dataset.cashAction === 'open' ? isOpen : !isOpen; });
    feedback.hidden = true;
    const metrics = [
      ['Saldo inicial', current.openingBalance], ['Entradas', current.inflows],
      ['Saídas', current.outflows], ['Saldo esperado', current.expectedBalance]
    ];
    kpis.innerHTML = metrics.map(([label, value]) => `<article class="kpi-card"><span>${label}</span><strong class="kpi-value">${money(value)}</strong></article>`).join('');
    rows.innerHTML = (current.movements || []).map(item => `<tr><td>${item.time || '—'}</td><td>${item.type || '—'}</td><td>${money(item.amount)}</td><td>${item.method || '—'}</td></tr>`).join('') || '<tr><td colspan="4">Nenhuma movimentação registrada.</td></tr>';
  };

  async function load() {
    buttons.forEach(button => { button.disabled = true; });
    status.textContent = 'Consultando';
    try {
      const response = await fetch('/AdminApi/cash/current', { headers: { Accept: 'application/json' } });
      const payload = await response.json().catch(() => null);
      if (!response.ok) throw new Error(payload?.message || 'Serviço de caixa indisponível.');
      render(payload?.data ?? payload);
    } catch (error) { showUnavailable(error.message); }
  }

  document.addEventListener('click', event => {
    if (event.target.closest('[data-cash-retry]')) load();
  });
  load();
})();
