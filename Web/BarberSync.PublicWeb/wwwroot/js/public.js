(() => {
  'use strict';

  const form = document.getElementById('publicAppointment');
  const serviceSelect = form?.elements.namedItem('serviceId');
  const professionalSelect = form?.elements.namedItem('professionalId');
  const result = document.getElementById('publicAppointmentResult');
  const submit = form?.querySelector('[type="submit"]');
  const toastElement = document.getElementById('publicToast');

  const toast = (message, kind = 'info') => {
    if (!toastElement) return;
    toastElement.textContent = message;
    toastElement.dataset.kind = kind;
    toastElement.hidden = false;
    window.setTimeout(() => { toastElement.hidden = true; }, 4500);
  };

  const unwrap = payload => payload?.data ?? payload?.items ?? payload ?? [];
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, char => ({ '&':'&amp;', '<':'&lt;', '>':'&gt;', "'":'&#39;', '"':'&quot;' })[char]);

  async function request(url, options) {
    const response = await fetch(url, { headers: { Accept: 'application/json', ...(options?.headers || {}) }, ...options });
    const payload = await response.json().catch(() => ({ success: false, message: 'A API retornou uma resposta inválida.' }));
    if (!response.ok || payload?.success === false) {
      const error = new Error(payload?.message || 'Não foi possível concluir a operação.');
      error.errors = payload?.errors || [];
      throw error;
    }
    return payload;
  }

  function renderCards(id, entries, template, emptyMessage) {
    const host = document.getElementById(id);
    if (!host) return;
    host.classList.remove('skeleton-grid');
    host.innerHTML = entries.length ? entries.map(template).join('') : `<article class="panel"><h3>Nenhum registro disponível</h3><p>${escapeHtml(emptyMessage)}</p></article>`;
  }

  async function loadCatalog() {
    try {
      const [servicePayload, professionalPayload] = await Promise.all([
        request('/PublicApi/services'), request('/PublicApi/professionals')
      ]);
      const services = unwrap(servicePayload);
      const professionals = unwrap(professionalPayload);

      renderCards('services', services.slice(0, 6), service => `<article class="panel service-card"><span class="public-badge">${escapeHtml(service.category || 'Serviço')}</span><h3>${escapeHtml(service.name)}</h3><p>${escapeHtml(service.description || 'Disponível para agendamento.')}</p><div><strong>R$ ${Number(service.price || 0).toFixed(2).replace('.', ',')}</strong><small>${Number(service.durationMinutes || 0)} min</small></div></article>`, 'Novos serviços serão publicados em breve.');
      renderCards('pros', professionals.slice(0, 5), professional => `<article class="panel pro-card"><div class="avatar">${escapeHtml((professional.name || 'P').slice(0, 1))}</div><h3>${escapeHtml(professional.name)}</h3><p>${escapeHtml(professional.specialty || 'Profissional')}</p></article>`, 'Não há profissionais disponíveis no momento.');

      if (serviceSelect) serviceSelect.innerHTML = '<option value="">Selecione o serviço</option>' + services.map(service => `<option value="${escapeHtml(service.id)}">${escapeHtml(service.name)} — R$ ${Number(service.price || 0).toFixed(2).replace('.', ',')}</option>`).join('');
      if (professionalSelect) professionalSelect.innerHTML = '<option value="">Primeiro profissional disponível</option>' + professionals.map(professional => `<option value="${escapeHtml(professional.id)}">${escapeHtml(professional.name)}</option>`).join('');
    } catch (error) {
      renderCards('services', [], () => '', 'Não foi possível carregar o catálogo. Tente novamente mais tarde.');
      renderCards('pros', [], () => '', 'Não foi possível carregar a equipe. Tente novamente mais tarde.');
      if (form) form.querySelectorAll('select, input, textarea, button').forEach(control => { control.disabled = true; });
      toast(error.message, 'error');
    }
  }

  form?.addEventListener('submit', async event => {
    event.preventDefault();
    if (!form.checkValidity()) { form.reportValidity(); toast('Revise os campos obrigatórios.', 'warning'); return; }

    const values = new FormData(form);
    const payload = {
      clientName: values.get('clientName'), phone: values.get('phone'), email: values.get('email') || null,
      serviceId: values.get('serviceId'), professionalId: values.get('professionalId') || null,
      scheduledAt: new Date(values.get('scheduledAt')).toISOString(), notes: values.get('notes') || null
    };

    submit.disabled = true;
    submit.setAttribute('aria-busy', 'true');
    result.textContent = 'Confirmando disponibilidade e reservando o horário...';
    try {
      const response = await request('/PublicApi/appointments', { method: 'POST', headers: { 'Content-Type':'application/json' }, body: JSON.stringify(payload) });
      const protocol = response?.data?.protocol;
      result.innerHTML = `<strong>Agendamento confirmado${protocol ? ` — protocolo ${escapeHtml(protocol)}` : ''}.</strong><br>Você receberá os detalhes no contato informado.`;
      toast('Agendamento criado com sucesso.', 'success');
      form.reset();
    } catch (error) {
      const fieldMessage = error.errors?.[0]?.message;
      result.textContent = fieldMessage || error.message;
      toast(fieldMessage || error.message, 'error');
    } finally {
      submit.disabled = false;
      submit.removeAttribute('aria-busy');
    }
  });

  const minimum = new Date(Date.now() + 30 * 60 * 1000);
  const scheduledAt = form?.elements.namedItem('scheduledAt');
  if (scheduledAt) scheduledAt.min = new Date(minimum.getTime() - minimum.getTimezoneOffset() * 60000).toISOString().slice(0, 16);
  loadCatalog();
})();

(() => {
  const form = document.getElementById('roiCalculator');
  if (!form) return;
  const calculate = () => {
    const values = new FormData(form);
    const revenue = (+values.get('att') || 0) * (+values.get('ticket') || 0) * (+values.get('days') || 0);
    document.getElementById('roiResult').innerHTML = `Receita mensal estimada: <strong>${revenue.toLocaleString('pt-BR', { style:'currency', currency:'BRL' })}</strong>`;
  };
  form.addEventListener('submit', event => { event.preventDefault(); calculate(); });
  calculate();
})();
