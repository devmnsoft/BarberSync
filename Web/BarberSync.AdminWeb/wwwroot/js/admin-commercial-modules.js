(() => {
  const root = document.querySelector('.commercial-page');
  if (!root) return;
  const endpoint = root.dataset.endpoint;
  const state = document.querySelector('#commercial-state');
  const table = document.querySelector('#commercial-table');
  const tbody = table.querySelector('tbody');
  const drawer = document.querySelector('#commercial-drawer');
  let items = [];

  const text = value => value == null ? '—' : Array.isArray(value) ? value.join(', ') : String(value);
  const safe = value => { const node = document.createElement('span'); node.textContent = text(value); return node.innerHTML; };
  const status = item => item.status || (item.isActive === false ? 'Inactive' : 'Active');
  function render(filter = '') {
    const visible = items.filter(x => JSON.stringify(x).toLowerCase().includes(filter.toLowerCase()));
    tbody.innerHTML = visible.map(item => {
      const title = item.name || item.description || item.invoiceNumber || item.professionalId || item.clientId || item.chair || item.id;
      const detail = Object.entries(item).filter(([k]) => !['id','tenantId','branchId','name','description','status','isActive','createdAt','updatedAt'].includes(k)).slice(0, 3).map(([k,v]) => `${k}: ${text(v)}`).join(' • ');
      return `<tr><td><strong>${safe(title)}</strong><small>${safe(item.createdAt ? new Date(item.createdAt).toLocaleDateString('pt-BR') : '')}</small></td><td>${safe(detail)}</td><td><span class="commercial-badge ${safe(status(item).toLowerCase())}">${safe(status(item))}</span></td><td><button class="btn btn-light btn-sm" data-delete="${safe(item.id)}">Inativar</button></td></tr>`;
    }).join('');
    state.hidden = visible.length > 0;
    state.textContent = visible.length ? '' : 'Nenhum registro encontrado. Cadastre o primeiro para iniciar a operação.';
    table.hidden = visible.length === 0;
    document.querySelector('#kpi-total').textContent = items.length;
    document.querySelector('#kpi-active').textContent = items.filter(x => !/inactive|cancel|expired/i.test(status(x))).length;
    document.querySelector('#kpi-attention').textContent = items.filter(x => /pending|overdue|expired/i.test(status(x))).length;
  }
  async function load() {
    state.hidden = false; state.textContent = 'Carregando dados operacionais…'; table.hidden = true;
    const response = await window.adminGet(`/api/${endpoint}`);
    if (!response.success) { state.textContent = response.message || 'Não foi possível carregar os dados.'; return; }
    items = Array.isArray(response.data) ? response.data : response.data?.items || [];
    render(document.querySelector('#commercial-search').value);
  }
  const close = () => { drawer.hidden = true; document.body.classList.remove('drawer-open'); };
  document.querySelector('#commercial-new').onclick = () => { drawer.hidden = false; document.body.classList.add('drawer-open'); };
  drawer.querySelectorAll('.drawer-close,.drawer-scrim').forEach(x => x.onclick = close);
  document.querySelector('#commercial-refresh').onclick = load;
  document.querySelector('#commercial-search').oninput = e => render(e.target.value);
  drawer.querySelector('form').onsubmit = async e => {
    e.preventDefault();
    const payload = Object.fromEntries(new FormData(e.currentTarget));
    Object.keys(payload).forEach(k => { if (/services|items/i.test(k)) payload[k] = payload[k].split(',').map(x => x.trim()).filter(Boolean); });
    if (endpoint === 'commissions') payload.saleStatus = 'Paid';
    const response = await window.adminPost(`/api/${endpoint}`, payload);
    if (!response.success) return;
    window.AdminToast?.showSuccess?.('Registro salvo com sucesso.'); e.currentTarget.reset(); close(); await load();
  };
  tbody.onclick = async e => {
    const button = e.target.closest('[data-delete]'); if (!button || !confirm('Confirma a inativação deste registro?')) return;
    const response = await window.adminDelete(`/api/${endpoint}/${button.dataset.delete}`);
    if (response.success) { window.AdminToast?.showSuccess?.('Registro inativado.'); await load(); }
  };
  load();
})();
