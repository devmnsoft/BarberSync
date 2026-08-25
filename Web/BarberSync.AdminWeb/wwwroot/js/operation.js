(() => {
  'use strict';
  const root = document.querySelector('#operationWorkspace');
  if (!root) return;
  const q = selector => document.querySelector(selector);
  const qa = selector => [...document.querySelectorAll(selector)];
  const state = { appointments: [], orders: [], professionals: [], cash: null, selected: null, busy: false };
  const esc = value => String(value ?? '').replace(/[&<>'"]/g, c => ({ '&':'&amp;', '<':'&lt;', '>':'&gt;', "'":'&#39;', '"':'&quot;' }[c]));
  const list = payload => { const value = payload?.data ?? payload; return Array.isArray(value) ? value : value?.items ?? []; };
  const money = value => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(Number(value || 0));
  const statusName = { Scheduled:'Agendado', Confirmed:'Confirmado', CheckedIn:'Check-in realizado', InService:'Em atendimento', Finished:'Aguardando pagamento', Cancelled:'Cancelado', NoShow:'Não compareceu', Open:'Aberta', PartiallyPaid:'Pagamento parcial', Paid:'Paga' };

  async function api(path, options = {}) {
    const response = await fetch(`/AdminApi/${path}`, { ...options, headers: { Accept:'application/json', ...(options.body ? {'Content-Type':'application/json'} : {}), ...options.headers } });
    const text = await response.text();
    let payload = null;
    try { payload = text ? JSON.parse(text) : null; } catch { payload = null; }
    if (!response.ok) {
      const traceId = payload?.traceId || payload?.extensions?.traceId || response.headers.get('X-Trace-Id');
      const messages = { 401:'Sua sessão expirou. Entre novamente para continuar.', 403:'Seu perfil não possui permissão para esta ação.', 404:'O registro não existe nesta unidade ou foi removido.' };
      const error = new Error(messages[response.status] || payload?.detail || payload?.message || payload?.title || 'A operação não pôde ser concluída.');
      error.status = response.status; error.traceId = traceId; throw error;
    }
    return payload?.data ?? payload;
  }
  const post = (path, body) => api(path, { method:'POST', ...(body === undefined ? {} : { body:JSON.stringify(body) }) });
  function showError(error) {
    q('#operationAlert').hidden = false;
    q('#operationAlertTitle').textContent = error.status === 403 ? 'Permissão necessária' : error.status === 401 ? 'Autenticação necessária' : 'Não foi possível concluir';
    q('#operationAlertMessage').textContent = error.message;
    q('#operationTrace').textContent = error.traceId ? `traceId: ${error.traceId}` : '';
  }
  const clearError = () => { q('#operationAlert').hidden = true; };

  function filters() {
    const professional = q('#operationProfessional').value, status = q('#operationStatus').value, period = q('#operationPeriod').value;
    return state.appointments.filter(item => (!professional || item.professionalId === professional) && (!status || item.status === status) && (period === 'day' || (period === 'morning' ? new Date(item.scheduledStart).getHours() < 12 : new Date(item.scheduledStart).getHours() >= 12)));
  }
  function renderMetrics() {
    const appointments = state.appointments;
    q('[data-metric="scheduled"]').textContent = appointments.filter(x => ['Scheduled','Confirmed','CheckedIn'].includes(x.status)).length;
    q('[data-metric="serving"]').textContent = appointments.filter(x => x.status === 'InService').length;
    q('[data-metric="waiting"]').textContent = state.orders.filter(x => ['Open','PartiallyPaid'].includes(x.status) && Number(x.balance) > 0).length;
    q('[data-metric="paid"]').textContent = state.orders.filter(x => x.status === 'Paid').length;
    q('[data-metric="cash"]').textContent = state.cash ? money(state.cash.expectedBalance) : 'Sem caixa';
    q('[data-cash-status]').textContent = state.cash?.status === 'Open' ? 'Caixa aberto' : 'Nenhum caixa aberto';
  }
  function renderTimeline() {
    const items = filters().sort((a,b) => new Date(a.scheduledStart) - new Date(b.scheduledStart));
    q('#agendaCount').textContent = `${items.length} atendimento${items.length === 1 ? '' : 's'}`;
    q('#operationTimeline').className = 'operation-timeline';
    q('#operationTimeline').innerHTML = items.length ? items.map(item => `<article class="operation-appointment ${state.selected?.id === item.id ? 'active' : ''}" data-appointment-id="${esc(item.id)}" data-status="${esc(item.status)}" tabindex="0"><time class="time">${new Date(item.scheduledStart).toLocaleTimeString('pt-BR',{hour:'2-digit',minute:'2-digit'})}</time><i class="rail"></i><div><h3>${esc(item.clientName)}</h3><p>${esc(item.serviceName)} · ${esc(item.professionalName)}</p></div><span class="operation-badge">${esc(statusName[item.status] || item.status)}</span></article>`).join('') : '<div class="operation-empty"><span>◷</span><h2>Nenhum atendimento neste período</h2><p>A agenda real não retornou registros para os filtros selecionados.</p></div>';
  }
  function orderFor(appointment) { return state.orders.find(order => order.appointmentId === appointment.id); }
  function renderDetail() {
    const item = state.selected;
    if (!item) return;
    const order = orderFor(item);
    const action = item.status === 'Scheduled' || item.status === 'Confirmed' ? '<button class="btn btn-primary" data-action="check-in">Fazer check-in</button>' : item.status === 'CheckedIn' ? '<button class="btn btn-primary" data-action="start">Iniciar atendimento</button>' : item.status === 'InService' ? '<button class="btn btn-primary" data-action="finish">Finalizar serviço</button>' : '';
    const exceptions = ['Scheduled','Confirmed','CheckedIn'].includes(item.status) ? '<button class="btn btn-light" data-action="no-show">Não compareceu</button><button class="btn btn-light" data-action="cancel">Cancelar</button>' : '';
    const orderAction = order ? `<a class="btn btn-light" href="/Operation/ServiceOrders/${esc(order.id)}">Comanda ${esc(order.number)}</a>` : `<button class="btn btn-light" data-action="open-order">Abrir comanda</button>`;
    q('#operationDetail').innerHTML = `<span class="operation-kicker">Detalhes do atendimento</span><h2 id="detailTitle">${esc(item.clientName)}</h2><span class="operation-badge">${esc(statusName[item.status] || item.status)}</span><dl><div><dt>Horário</dt><dd>${new Date(item.scheduledStart).toLocaleTimeString('pt-BR',{hour:'2-digit',minute:'2-digit'})}</dd></div><div><dt>Duração</dt><dd>${esc(item.durationMinutes)} min</dd></div><div><dt>Serviço</dt><dd>${esc(item.serviceName)}</dd></div><div><dt>Profissional</dt><dd>${esc(item.professionalName)}</dd></div><div><dt>Origem</dt><dd>${esc(item.origin)}</dd></div><div><dt>Comanda</dt><dd>${order ? esc(statusName[order.status] || order.status) : 'Ainda não aberta'}</dd></div></dl>${item.cancellationReason ? `<p><strong>Motivo:</strong> ${esc(item.cancellationReason)}</p>` : ''}<div class="operation-detail-actions">${action}${orderAction}${exceptions}</div>`;
  }
  function renderOrders() {
    const orders = state.orders.filter(order => ['Open','PartiallyPaid'].includes(order.status));
    q('#operationOrders').className = 'operation-orders-list';
    q('#operationOrders').innerHTML = orders.length ? orders.map(order => `<a class="operation-order" href="/Operation/ServiceOrders/${esc(order.id)}"><div><strong>${esc(order.number)}</strong><span class="operation-badge">${esc(statusName[order.status] || order.status)}</span></div><small>${order.items?.length || 0} itens · saldo pendente</small><b>${money(order.balance)}</b></a>`).join('') : '<div class="operation-empty"><h2>Nenhuma comanda aberta</h2><p>As comandas pagas permanecem disponíveis no histórico do PDV.</p></div>';
  }
  async function load() {
    root.setAttribute('aria-busy','true'); clearError();
    try {
      const date = new Date(), start = new Date(date); start.setHours(0,0,0,0); const end = new Date(date); end.setHours(23,59,59,999);
      const results = await Promise.all([api(`appointments?from=${encodeURIComponent(start.toISOString())}&to=${encodeURIComponent(end.toISOString())}`), api('service-orders'), api('professionals'), api('cash-registers/current')]);
      state.appointments = list(results[0]); state.orders = list(results[1]); state.professionals = list(results[2]); state.cash = results[3];
      q('#operationProfessional').innerHTML = '<option value="">Todos</option>' + state.professionals.map(item => `<option value="${esc(item.id)}">${esc(item.name)}</option>`).join('');
      if (state.selected) state.selected = state.appointments.find(x => x.id === state.selected.id) || null;
      renderMetrics(); renderTimeline(); renderOrders(); if (state.selected) renderDetail();
    } catch (error) { showError(error); q('#operationTimeline').innerHTML = '<div class="operation-empty"><h2>Agenda indisponível</h2><p>Use o traceId acima ao solicitar suporte.</p></div>'; }
    finally { root.setAttribute('aria-busy','false'); }
  }
  function reason(title) {
    return new Promise(resolve => { const dialog=q('#reasonDialog'), form=q('#reasonForm'); q('#reasonTitle').textContent=title; q('#reasonText').value='';q('#reasonError').textContent='';dialog.showModal();form.addEventListener('submit',event=>{if(event.submitter?.value==='cancel'){resolve(null);return;}event.preventDefault();const value=q('#reasonText').value.trim();if(!value){q('#reasonError').textContent='O motivo é obrigatório.';return;}dialog.close();resolve(value);},{once:true}); });
  }
  async function act(action) {
    if (state.busy || !state.selected) return;
    state.busy = true; clearError();
    try {
      if (action === 'open-order') await post('service-orders/open', { clientId:state.selected.clientId, appointmentId:state.selected.id, notes:'Aberta pela Operação do Dia' });
      else if (action === 'start') { let order=orderFor(state.selected); if (!order) order=await post('service-orders/open',{clientId:state.selected.clientId,appointmentId:state.selected.id,notes:'Atendimento iniciado pela Operação do Dia'}); await post(`appointments/${state.selected.id}/start`); }
      else if (action === 'cancel' || action === 'no-show') { const why=await reason(action === 'cancel' ? 'Motivo do cancelamento' : 'Motivo da ausência'); if (!why) return; await post(`appointments/${state.selected.id}/${action}`,{reason:why}); }
      else await post(`appointments/${state.selected.id}/${action}`);
      window.AdminToast?.updated?.('Operação registrada no histórico.'); await load();
    } catch(error) { showError(error); }
    finally { state.busy=false; }
  }
  q('#operationDate').textContent = new Intl.DateTimeFormat('pt-BR',{weekday:'long',day:'2-digit',month:'long',year:'numeric'}).format(new Date());
  q('#operationRefresh').addEventListener('click',load); q('#operationRetry').addEventListener('click',load);
  qa('#operationProfessional,#operationStatus,#operationPeriod').forEach(control=>control.addEventListener('change',renderTimeline));
  q('#operationTimeline').addEventListener('click',event=>{const card=event.target.closest('[data-appointment-id]');if(!card)return;state.selected=state.appointments.find(x=>x.id===card.dataset.appointmentId);renderTimeline();renderDetail();});
  q('#operationTimeline').addEventListener('keydown',event=>{if(event.key==='Enter')event.target.click();});
  q('#operationDetail').addEventListener('click',event=>{const button=event.target.closest('[data-action]');if(button)act(button.dataset.action);});
  load();
})();
