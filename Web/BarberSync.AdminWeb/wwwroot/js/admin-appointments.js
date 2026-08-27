(() => {
  const $ = (selector, root = document) => root.querySelector(selector);
  const $$ = (selector, root = document) => [...root.querySelectorAll(selector)];
  const state = { appointments: [], dirty: false };
  const unwrap = value => value?.data?.items ?? value?.data ?? value?.items ?? value ?? [];
  const escape = value => String(value ?? '').replace(/[&<>'"]/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[char]));
  const toast = (kind, message) => window.AdminToast?.[kind]?.(message) ?? window.AdminToast?.show?.(message);

  async function api(path, options = {}) {
    const response = await fetch(`/AdminApi/${path}`, { headers: {'Content-Type':'application/json'}, ...options });
    const body = await response.json().catch(() => ({}));
    if (!response.ok) { const error = new Error(body.detail || body.message || 'Não foi possível concluir a operação.'); error.status = response.status; throw error; }
    return body;
  }

  async function loadReference(path, selectors, label) {
    const values = unwrap(await api(path));
    selectors.forEach(selector => {
      const select = $(selector); if (!select) return;
      select.insertAdjacentHTML('beforeend', values.map(x => `<option value="${escape(x.id)}">${escape(x.name || x.fullName || x.title || label)}</option>`).join(''));
    });
  }

  function query() {
    const date = $('[data-filter-date]').value;
    const params = new URLSearchParams();
    if (date) { params.set('from', `${date}T00:00:00-03:00`); params.set('to', `${date}T23:59:59-03:00`); }
    [['professionalId','[data-filter-professional]'],['serviceId','[data-filter-service]'],['status','[data-filter-status]'],['origin','[data-filter-origin]']].forEach(([key, selector]) => { if ($(selector).value) params.set(key, $(selector).value); });
    return params.toString();
  }

  function actions(item) {
    const permitted = { Scheduled:['confirm','check-in','cancel','no-show'], Confirmed:['check-in','cancel','no-show'], CheckedIn:['start','cancel'], InService:['finish'] }[item.status] || [];
    const names = {'confirm':'Confirmar','check-in':'Cliente chegou','start':'Iniciar','finish':'Finalizar','cancel':'Cancelar','no-show':'Marcar no-show'};
    return permitted.map(action => `<button class="btn btn-light btn-sm" data-action="${action}" data-id="${escape(item.id)}">${names[action]}</button>`).join('') +
      `<a class="btn btn-light btn-sm" href="/Admin/ServiceOrders?appointmentId=${escape(item.id)}">Abrir comanda</a><a class="btn btn-light btn-sm" href="/Clients360/Profile/${escape(item.clientId)}">Cliente 360</a>`;
  }

  function render() {
    $('[data-agenda-loading]').hidden = true; $('[data-agenda-error]').hidden = true;
    $('[data-agenda-empty]').hidden = state.appointments.length > 0;
    $('[data-agenda-list]').innerHTML = state.appointments.map(item => {
      const start = new Date(item.scheduledStart); const end = new Date(item.scheduledEnd);
      return `<article class="bs-calendar-card panel"><div class="calendar-time"><strong>${start.toLocaleTimeString('pt-BR',{hour:'2-digit',minute:'2-digit'})}</strong><small>até ${end.toLocaleTimeString('pt-BR',{hour:'2-digit',minute:'2-digit'})}</small></div><div><span class="bs-status-badge status-${escape(item.status).toLowerCase()}">${escape(item.status)}</span><h3>${escape(item.clientName)}</h3><p>${escape(item.serviceName)} com ${escape(item.professionalName)}</p><small>Origem: ${escape(item.origin)}</small></div><div class="page-actions">${actions(item)}</div></article>`;
    }).join('');
  }

  async function load() {
    $('[data-agenda-loading]').hidden = false; $('[data-agenda-error]').hidden = true;
    try { state.appointments = unwrap(await api(`appointments?${query()}`)); render(); }
    catch (error) { $('[data-agenda-loading]').hidden = true; $('[data-agenda-error]').hidden = false; $('[data-error-detail]').textContent = error.message; }
  }

  async function changeStatus(button) {
    const action = button.dataset.action; let body;
    if (['cancel','no-show'].includes(action)) {
      const accepted = await window.BarberSyncConfirm?.ask?.({ title: action === 'cancel' ? 'Cancelar agendamento?' : 'Registrar ausência?', message:'Esta ação ficará registrada no histórico.', confirmText:'Confirmar' });
      if (accepted === false) return;
      const reason = prompt(action === 'cancel' ? 'Informe o motivo do cancelamento:' : 'Informe o motivo da ausência:')?.trim();
      if (!reason) return toast('validation', action === 'cancel' ? 'O motivo do cancelamento é obrigatório.' : 'O motivo da ausência é obrigatório.');
      body = JSON.stringify({reason});
    }
    button.disabled = true;
    try { await api(`appointments/${button.dataset.id}/${action}`, {method:'POST', body}); toast('updated', action === 'no-show' ? 'Ausência registrada.' : 'Agenda atualizada.'); await load(); }
    catch (error) { toast(error.status === 403 ? 'validation' : 'error', error.status === 403 ? 'Você não possui permissão para esta ação.' : error.message); }
    finally { button.disabled = false; }
  }

  function toggleDrawer(open) { $('[data-agenda-drawer]').hidden = !open; if (!open) state.dirty = false; }
  document.addEventListener('click', async event => {
    if (event.target.closest('[data-agenda-refresh]')) load();
    if (event.target.closest('[data-agenda-new]')) toggleDrawer(true);
    if (event.target.closest('[data-drawer-close]')) { if (!state.dirty || confirm('Descartar alterações não salvas?')) toggleDrawer(false); }
    const action = event.target.closest('[data-action]'); if (action) changeStatus(action);
  });
  $$('[data-filter-date],[data-filter-professional],[data-filter-service],[data-filter-status],[data-filter-origin]').forEach(input => input.addEventListener('change', load));
  $('[data-agenda-form]').addEventListener('input', () => state.dirty = true);
  $('[data-agenda-form]').addEventListener('submit', async event => {
    event.preventDefault(); const form = event.currentTarget; const submit = $('[type=submit]', form); const data = Object.fromEntries(new FormData(form));
    if (!data.clientId || !data.serviceId || !data.professionalId || !data.date || !data.time) return toast('validation','Preencha os campos obrigatórios.');
    submit.disabled = true; submit.textContent = 'Criando…';
    try { await api('appointments', { method:'POST', body:JSON.stringify({clientId:data.clientId, serviceId:data.serviceId, professionalId:data.professionalId, scheduledStart:`${data.date}T${data.time}:00-03:00`, origin:data.origin, notes:data.notes || null}) }); toast('created','Agendamento criado.'); form.reset(); toggleDrawer(false); await load(); }
    catch (error) { toast(error.message.includes('horário') ? 'validation' : 'error', error.message); }
    finally { submit.disabled = false; submit.textContent = 'Criar agendamento'; }
  });
  document.addEventListener('DOMContentLoaded', async () => {
    $('[data-filter-date]').value = new Date().toISOString().slice(0,10); $('[name=date]').value = $('[data-filter-date]').value;
    await Promise.allSettled([loadReference('clients',['[name=clientId]'],'Cliente'), loadReference('services',['[name=serviceId]','[data-filter-service]'],'Serviço'), loadReference('professionals',['[name=professionalId]','[data-filter-professional]'],'Profissional')]);
    load();
  });
})();
