(() => {
  const $ = (s, root = document) => root.querySelector(s);
  const $$ = (s, root = document) => [...root.querySelectorAll(s)];
  const state = { people: [], services: [], selected: null, operations: null };
  const drawer = $('[data-team-drawer]'), form = $('[data-team-form]');
  const unwrap = value => value?.data ?? value ?? [];
  const initials = name => (name || '?').split(/\s+/).slice(0, 2).map(x => x[0]).join('').toUpperCase();
  const escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;' }[c]));

  async function load() {
    $('[data-team-loading]').hidden = false; $('[data-team-error]').hidden = true;
    const [people, services] = await Promise.all([adminGet('/AdminApi/professionals'), adminGet('/AdminApi/services')]);
    $('[data-team-loading]').hidden = true;
    if (people?.success === false || services?.success === false) { $('[data-team-error]').hidden = false; return; }
    state.people = unwrap(people); state.services = unwrap(services).filter(x => x.isActive !== false && x.status === 'Active'); render();
    const profile = new URLSearchParams(location.search).get('profile'); if (profile) { const selected = state.people.find(x => x.id === profile); if (selected) await open(selected); }
  }
  function render() {
    const query = $('[data-team-search]').value.trim().toLowerCase(), status = $('[data-team-status]').value;
    const rows = state.people.filter(x => (!status || x.status === status) && (!query || `${x.name} ${x.role || ''} ${x.specialty || ''}`.toLowerCase().includes(query)));
    $('[data-team-count]').textContent = `${rows.length} profissional${rows.length === 1 ? '' : 'is'}`;
    $('[data-team-empty]').hidden = rows.length !== 0; $('[data-team-grid]').innerHTML = rows.map(x => `<article class="team-card">
      <div class="team-card-head"><span class="team-avatar">${initials(x.name)}</span><div><h3>${escape(x.name)}</h3><p>${escape(x.role || x.specialty || 'Profissional')}</p></div></div>
      <div><span class="team-badge ${x.status === 'Active' ? '' : 'inactive'}">${x.status === 'Active' ? 'Ativo' : 'Inativo'}</span></div>
      <div class="team-meta"><span>${escape(x.employmentType || 'Vínculo não informado')}</span><span>${escape(x.specialty || '')}</span></div>
      <button class="btn btn-light" data-open-profile="${x.id}">Abrir perfil 360</button></article>`).join('');
    $$('[data-open-profile]').forEach(b => b.onclick = () => open(state.people.find(x => x.id === b.dataset.openProfile)));
  }
  function scheduleMarkup(values = []) {
    const names = ['Segunda','Terça','Quarta','Quinta','Sexta','Sábado','Domingo'];
    return names.map((name, i) => { const row = values.find(x => Number(x.day_of_week) === i + 1); return `<label class="schedule-row"><strong><input type="checkbox" data-day="${i + 1}" ${row ? 'checked' : ''}> ${name}</strong><input type="time" data-start value="${String(row?.start_time || '09:00').slice(0,5)}"><input type="time" data-end value="${String(row?.end_time || '18:00').slice(0,5)}"><input type="time" data-break-start value="${String(row?.break_start || '').slice(0,5)}" aria-label="Início da pausa"><input type="time" data-break-end value="${String(row?.break_end || '').slice(0,5)}" aria-label="Fim da pausa"></label>`; }).join('');
  }
  async function open(person = null) {
    state.selected = person; state.operations = null; form.reset(); $('[data-team-title]').textContent = person ? person.name : 'Novo profissional';
    if (person) Object.entries(person).forEach(([key, value]) => { const input = form.elements[key]; if (input && value != null) input.value = key === 'admissionDate' ? String(value).slice(0,10) : value; });
    form.elements.specialties.value = person?.specialty || '';
    $('[data-service-list]').innerHTML = state.services.map(s => `<label><input type="checkbox" value="${s.id}"> ${escape(s.name)} <small>${s.durationMinutes || s.duration_minutes || 30} min</small></label>`).join('');
    const standardSchedule = [1,2,3,4,5].map(day_of_week => ({ day_of_week, start_time:'09:00', end_time:'18:00', break_start:'12:00', break_end:'13:00' }));
    $('[data-schedule-list]').innerHTML = scheduleMarkup(person ? [] : standardSchedule); $('[data-block-list]').innerHTML = '';
    if (person) {
      const response = await adminGet(`/AdminApi/professionals/${person.id}/operations`); state.operations = unwrap(response);
      const linked = new Set((state.operations.services || []).map(x => String(x.id))); $$('[data-service-list] input').forEach(x => x.checked = linked.has(x.value));
      $('[data-schedule-list]').innerHTML = scheduleMarkup(state.operations.schedules || []);
      $('[data-block-list]').innerHTML = (state.operations.blocks || []).map(x => `<div class="block-item"><strong>${escape(x.reason)}</strong><br><small>${new Date(x.start_at).toLocaleString('pt-BR')} — ${new Date(x.end_at).toLocaleString('pt-BR')}</small></div>`).join('');
    }
    drawer.hidden = false; document.body.style.overflow = 'hidden';
  }
  function close() { drawer.hidden = true; document.body.style.overflow = ''; }
  form.onsubmit = async event => {
    event.preventDefault(); const data = Object.fromEntries(new FormData(form));
    const payload = { ...data, specialty: data.specialties, isActive: data.status === 'Active' };
    const response = state.selected ? await adminPut(`/AdminApi/professionals/${state.selected.id}`, payload) : await adminPost('/AdminApi/professionals', payload);
    if (response?.success === false) return;
    const professional = unwrap(response), id = professional.id;
    const serviceIds = $$('[data-service-list] input:checked').map(x => x.value);
    const schedule = $$('[data-schedule-list] [data-day]:checked').map(day => { const row = day.closest('.schedule-row'); return { dayOfWeek:Number(day.dataset.day), start:$('[data-start]',row).value, end:$('[data-end]',row).value, breakStart:$('[data-break-start]',row).value || null, breakEnd:$('[data-break-end]',row).value || null }; });
    const [servicesResult, scheduleResult] = await Promise.all([adminPut(`/AdminApi/professionals/${id}/services`, { serviceIds }), adminPut(`/AdminApi/professionals/${id}/schedule`, schedule)]);
    if (servicesResult?.success === false || scheduleResult?.success === false) return;
    window.AdminToast?.showSuccess?.('Profissional, serviços e escala salvos.'); close(); await load();
  };
  $('[data-create-block]').onclick = async () => {
    if (!state.selected) { window.AdminToast?.showError?.('Salve o profissional antes de criar um bloqueio.'); return; }
    const body = { start:form.elements.blockStart.value, end:form.elements.blockEnd.value, reason:form.elements.blockReason.value, description:form.elements.blockDescription.value };
    const result = await adminPost(`/AdminApi/professionals/${state.selected.id}/blocks`, body); if (result?.success === false) return;
    window.AdminToast?.showSuccess?.('Bloqueio criado e aplicado à agenda.'); await open(state.selected);
  };
  $$('[data-tab]').forEach(tab => tab.onclick = () => { $$('[data-tab]').forEach(x => x.classList.toggle('active', x === tab)); $$('[data-pane]').forEach(x => x.hidden = x.dataset.pane !== tab.dataset.tab); });
  $$('[data-team-new]').forEach(x => x.onclick = () => open()); $$('[data-team-close]').forEach(x => x.onclick = close); $$('[data-team-refresh]').forEach(x => x.onclick = load);
  $('[data-team-search]').oninput = render; $('[data-team-status]').onchange = render; drawer.onclick = e => { if (e.target === drawer) close(); }; load();
})();
