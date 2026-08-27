(() => {
  'use strict';
  const api = '/AdminApi/clients360';
  const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
  const unwrap = payload => payload?.data ?? payload;
  async function request(url, options) {
    const response = await fetch(url, { headers: { Accept: 'application/json', 'Content-Type': 'application/json' }, ...options });
    const payload = await response.json().catch(() => ({}));
    if (!response.ok) throw Object.assign(new Error(payload.detail || payload.title || 'A API recusou a operação.'), { traceId: payload.traceId });
    return unwrap(payload);
  }
  const showError = (root, error) => { root.querySelector('[data-error]').hidden = false; root.querySelector('[data-error-message]').textContent = error.message; root.querySelector('[data-trace-id]').textContent = error.traceId || 'não informado'; };

  const searchRoot = document.querySelector('[data-c360-search]');
  if (searchRoot) {
    const input = searchRoot.querySelector('[data-client-search]');
    async function search() {
      searchRoot.querySelector('[data-error]').hidden = true;
      try {
        const clients = await request(`${api}/search?q=${encodeURIComponent(input.value.trim())}`);
        searchRoot.querySelector('[data-results]').innerHTML = clients.length ? clients.map(client => `<a class="c360-result" href="/Clients360/Profile/${encodeURIComponent(client.id)}"><span class="c360-avatar">${escapeHtml((client.name || '?').split(/\s+/).slice(0, 2).map(x => x[0]).join(''))}</span><span><strong>${escapeHtml(client.name)}</strong><small>${escapeHtml(client.phone || client.email || 'Contato não informado')}</small></span><b>Abrir perfil →</b></a>`).join('') : '<div class="c360-empty"><strong>Nenhum cliente encontrado</strong><span>Revise a busca ou cadastre o cliente na área de relacionamento.</span></div>';
      } catch (error) { showError(searchRoot, error); }
    }
    searchRoot.querySelector('[data-search-button]').addEventListener('click', search);
    input.addEventListener('keydown', event => { if (event.key === 'Enter') search(); });
    request(`${api}/dashboard`).then(data => { searchRoot.querySelector('[data-dashboard]').innerHTML = [['Clientes', data.clients], ['Com ficha técnica', data.technicalSheets], ['Planos ativos', data.activePlans], ['Follow-ups pendentes', data.pendingFollowUps]].map(x => `<article><small>${x[0]}</small><strong>${Number(x[1] || 0)}</strong></article>`).join(''); }).catch(error => showError(searchRoot, error));
  }

  const root = document.querySelector('[data-c360-workspace]');
  if (!root) return;
  const clientId = root.dataset.clientId;
  const section = root.dataset.section;
  const config = {
    TechnicalSheet: ['technical-sheets', 'Ficha técnica', 'Fórmulas, medidas e observações por atendimento'],
    Anamnesis: ['anamnesis', 'Anamnese', 'Risco e respostas sensíveis'], VisualHistory: ['visual-records', 'Histórico visual', 'Antes, depois e evolução com consentimento'],
    Consents: ['consents', 'Consentimentos', 'Termos versionados e revogações'], Budgets: ['budgets', 'Orçamentos', 'Propostas e conversão confirmada no PDV'],
    TreatmentPlans: ['treatment-plans', 'Planos de tratamento', 'Jornada planejada em checklist'], FollowUps: ['follow-ups', 'Follow-ups', 'Retornos e comunicação responsável']
  };
  root.querySelector(`[data-tab="${section}"]`)?.classList.add('active');
  const date = value => value ? new Date(value).toLocaleString('pt-BR', { dateStyle: 'medium', timeStyle: 'short' }) : 'Não informado';
  function renderProfile(profile, timeline) {
    const client = profile.client; root.querySelector('[data-client-name]').textContent = client.name; root.querySelector('[data-client-contact]').textContent = [client.phone, client.email].filter(Boolean).join(' • ') || 'Contato não informado';
    root.querySelector('[data-avatar]').textContent = client.name.split(/\s+/).slice(0, 2).map(x => x[0]).join('').toUpperCase();
    const restrictions = profile.restrictions || []; const critical = restrictions.filter(x => x.severity === 'Critical');
    if (critical.length) { const alerts = root.querySelector('[data-risk-alerts]'); alerts.hidden = false; alerts.innerHTML = `<strong>Restrição crítica ativa</strong><span>${escapeHtml(critical.map(x => x.title).join(', '))}. Confirme a permissão antes de serviços de risco.</span>`; }
    root.querySelector('[data-preferences]').innerHTML = (profile.preferences || []).map(x => `<div class="c360-line"><strong>${escapeHtml(x.preference_key)}</strong><span>${escapeHtml(x.preference_value)}</span></div>`).join('') || 'Nenhuma preferência registrada.';
    root.querySelector('[data-restrictions]').innerHTML = restrictions.map(x => `<div class="c360-line"><span class="c360-pill ${escapeHtml(x.severity.toLowerCase())}">${escapeHtml(x.severity)}</span><span><strong>${escapeHtml(x.title)}</strong><small>${escapeHtml(x.description)}</small></span></div>`).join('') || 'Nenhuma restrição ativa.';
    root.querySelector('[data-timeline]').innerHTML = timeline.length ? timeline.map(x => `<article><i></i><div><strong>${escapeHtml(x.event_title)}</strong><p>${escapeHtml(x.event_description || x.event_type)}</p><time>${escapeHtml(date(x.occurred_at))}</time></div></article>`).join('') : '<div class="c360-empty"><strong>Linha do tempo vazia</strong><span>Novas fichas, consentimentos e planos aparecerão aqui.</span></div>';
  }
  function renderRecords(records) {
    root.querySelector('[data-records]').innerHTML = records.length ? records.map(item => `<article class="c360-record"><header><span class="c360-pill">${escapeHtml(item.status || item.privacy_status)}</span><time>${escapeHtml(date(item.created_at))}</time></header><h3>${escapeHtml(item.title || item.form_type || item.event_title || 'Registro')}</h3><p>${escapeHtml(item.summary || item.description || item.objective || item.review_notes || 'Sem observações adicionais.')}</p></article>`).join('') : '<div class="c360-empty c360-wide"><strong>Nenhum registro nesta área</strong><span>Use “Novo registro” para iniciar com trilha de auditoria.</span></div>';
  }
  const common = `<label>Título<input name="title" required maxlength="160"></label><label>Profissional<select name="professionalId" data-professional><option value="">Sem profissional vinculado</option></select></label><label>Observações<textarea name="description" rows="4" maxlength="2000"></textarea></label>`;
  const fields = {
    TechnicalSheet: `${common}<label>Tipo<select name="type" required><option value="Hair">Cabelo</option><option value="Beard">Barba</option><option value="Nails">Unhas</option><option value="Brows">Sobrancelhas</option><option value="Aesthetic">Estética</option><option value="Makeup">Maquiagem</option><option value="Other">Outro</option></select></label><label>Notas técnicas<textarea name="notes" rows="4"></textarea></label>`,
    Anamnesis: `<label>Tipo<select name="type" required><option value="General">Geral</option><option value="Aesthetic">Estética</option><option value="Chemical">Química</option><option value="Hair">Cabelo</option></select></label><fieldset><legend>Risco</legend><label><input type="radio" name="risk" value="Low" checked> Baixo</label><label><input type="radio" name="risk" value="Medium"> Médio</label><label><input type="radio" name="risk" value="High"> Alto</label></fieldset><label>Revisão<textarea name="notes" required></textarea></label><input type="hidden" name="status" value="Completed">`,
    Consents: `<label>Termo ativo<select name="termId" data-term required><option value="">Selecione um termo versionado</option></select></label><input type="hidden" name="channel" value="Admin"><label class="c360-check"><input type="checkbox" required> Confirmo que o cliente leu e aceitou este termo.</label>`,
    Budgets: `${common}<label>Validade<input type="date" name="date" required></label><div class="c360-money"><label>Subtotal<input type="number" min="0" step="0.01" name="subtotal" required></label><label>Desconto<input type="number" min="0" step="0.01" name="discount" value="0"></label><label>Total<input type="number" min="0" step="0.01" name="total" required></label></div>`,
    TreatmentPlans: `${common}<label>Início<input type="date" name="date" required></label><label>Fim previsto<input type="date" name="endDate"></label><label>Estimativa<input type="number" min="0" step="0.01" name="total"></label><input type="hidden" name="status" value="Active">`,
    FollowUps: `${common}<label>Origem<select name="type"><option value="Manual">Manual</option><option value="Appointment">Agendamento</option><option value="TreatmentPlan">Plano</option><option value="Budget">Orçamento</option></select></label><label>Prazo<input type="datetime-local" name="date" required></label>`,
    VisualHistory: `<div class="c360-consent-notice"><strong>Consentimento obrigatório</strong><p>O envio só é habilitado pelo armazenamento protegido quando há aceite ativo. Imagens não são expostas ou baixadas nesta tela sem permissão.</p></div><label>Foto protegida<input type="file" accept="image/jpeg,image/png,image/webp" required></label>`
  };
  let options;
  async function load() {
    root.querySelector('[data-error]').hidden = true; root.querySelector('[data-loading]').hidden = false; root.querySelector('[data-content]').hidden = true;
    try {
      const [profile, timeline] = await Promise.all([request(`${api}/${clientId}/profile`), request(`${api}/${clientId}/timeline`)]); renderProfile(profile, timeline);
      if (section === 'Profile') root.querySelector('[data-profile-section]').hidden = false;
      else { root.querySelector('[data-profile-section]').hidden = true; const meta = config[section]; root.querySelector('[data-module-section]').hidden = false; root.querySelector('[data-module-title]').textContent = meta[1]; root.querySelector('[data-module-eyebrow]').textContent = meta[2]; renderRecords(await request(`${api}/${clientId}/${meta[0]}`)); }
      root.querySelector('[data-content]').hidden = false;
    } catch (error) { showError(root, error); } finally { root.querySelector('[data-loading]').hidden = true; }
  }
  const dialog = root.querySelector('[data-record-dialog]');
  root.querySelector('[data-open-form]')?.addEventListener('click', async () => { root.querySelector('[data-dynamic-fields]').innerHTML = fields[section]; root.querySelector('[data-form-title]').textContent = `Novo: ${config[section][1]}`; options ||= await request(`${api}/filter-options`); (options.professionals || []).forEach(x => root.querySelector('[data-professional]')?.insertAdjacentHTML('beforeend', `<option value="${escapeHtml(x.value)}">${escapeHtml(x.label)}</option>`)); (options.terms || []).forEach(x => root.querySelector('[data-term]')?.insertAdjacentHTML('beforeend', `<option value="${escapeHtml(x.value)}">${escapeHtml(x.label)}</option>`)); dialog.showModal(); });
  root.querySelectorAll('[data-close-form]').forEach(button => button.addEventListener('click', () => dialog.close()));
  root.querySelector('[data-record-form]').addEventListener('submit', async event => { event.preventDefault(); const form = event.currentTarget; if (!form.checkValidity()) { form.reportValidity(); return; } if (section === 'VisualHistory') { root.querySelector('[data-form-error]').hidden = false; root.querySelector('[data-form-error]').textContent = 'O provedor de armazenamento protegido não está configurado. Nenhuma imagem foi enviada.'; return; } const body = Object.fromEntries(new FormData(form)); try { await request(`${api}/${clientId}/${config[section][0]}`, { method: 'POST', body: JSON.stringify(body) }); dialog.close(); form.reset(); await load(); } catch (error) { root.querySelector('[data-form-error]').hidden = false; root.querySelector('[data-form-error]').textContent = `${error.message} (traceId: ${error.traceId || 'não informado'})`; } });
  root.querySelector('[data-retry]').addEventListener('click', load); load();
})();
