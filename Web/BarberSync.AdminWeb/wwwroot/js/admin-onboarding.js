(() => {
  const steps = [
    { title: 'Dados da empresa', description: 'Identificação fiscal e comercial.', fields: [['companyName','Nome da empresa','text'],['document','CNPJ ou CPF','text']] },
    { title: 'Unidade principal', description: 'Nome e fuso usados em toda a operação.', fields: [['branchName','Nome da unidade','text'],['timezone','Fuso horário','select',['America/Sao_Paulo','America/Manaus','America/Recife']]] },
    { title: 'Horários', description: 'Defina a jornada padrão da unidade.', fields: [['weekdays','Dias de funcionamento','text'],['opensAt','Abertura','time'],['closesAt','Fechamento','time']] },
    { title: 'Serviço inicial', description: 'Cadastre a primeira oferta comercial.', fields: [['serviceName','Nome do serviço','text'],['durationMinutes','Duração (minutos)','number'],['price','Preço','number']] },
    { title: 'Profissional', description: 'Inclua quem realizará os atendimentos.', fields: [['professionalName','Nome completo','text'],['email','E-mail profissional','email']] },
    { title: 'Pagamentos', description: 'Informe os meios aceitos nesta unidade.', fields: [['methods','Formas de pagamento','text']] },
    { title: 'Caixa inicial', description: 'Defina o saldo de abertura da primeira operação.', fields: [['openingBalance','Saldo inicial','number']] },
    { title: 'Permissões básicas', description: 'Escolha um perfil inicial e suas permissões.', fields: [['profile','Perfil','select',['Manager','Receptionist','Cashier','Professional']],['permissions','Permissões (separadas por vírgula)','text']] },
    { title: 'Canais digitais', description: 'Escolha quais canais estarão disponíveis.', fields: [['publicWebEnabled','Ativar PublicWeb','checkbox'],['kioskEnabled','Ativar Totem','checkbox']] },
    { title: 'Conclusão', description: 'Revise e confirme a configuração desta unidade.', fields: [] }
  ];
  let current = 1; let saved = {};
  const q = selector => document.querySelector(selector);
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, char => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[char]));
  function fieldHtml(field) {
    const [name,label,type,options] = field; const value = saved[current]?.[name];
    if (type === 'select') return `<label class="form-field"><span>${label}</span><select name="${name}" required><option value="">Selecione</option>${options.map(x => `<option ${value === x ? 'selected' : ''}>${x}</option>`).join('')}</select></label>`;
    if (type === 'checkbox') return `<label class="form-field checkbox-field"><input name="${name}" type="checkbox" ${value ? 'checked' : ''}><span>${label}</span></label>`;
    const attrs = type === 'number' ? 'min="0" step="0.01"' : '';
    return `<label class="form-field"><span>${label}</span><input name="${name}" type="${type}" value="${escapeHtml(value)}" ${attrs} required></label>`;
  }
  function render() {
    const progress = current * 10; const step = steps[current - 1];
    q('[data-step-title]').textContent = step.title; q('[data-step-description]').textContent = step.description; q('[data-step-eyebrow]').textContent = `Etapa ${current} de 10`;
    q('[data-progress-label]').textContent = `Etapa ${current} de 10`; q('[data-progress-percent]').textContent = `${progress}%`; q('[data-progress-bar]').style.width = `${progress}%`; q('.onboarding-progress').setAttribute('aria-valuenow', progress);
    q('[data-step-fields]').innerHTML = step.fields.map(fieldHtml).join('') || '<div class="review-box"><strong>Tudo certo para começar?</strong><p>Ao concluir, esta unidade será marcada como configurada. A ação ficará disponível na auditoria.</p></div>';
    q('[data-back]').disabled = current === 1; q('[data-button-label]').textContent = current === 10 ? 'Concluir configuração' : 'Salvar e continuar';
    q('[data-step-nav]').innerHTML = steps.map((item,index) => `<button type="button" data-go="${index + 1}" ${index + 1 > Math.max(current, ...Object.keys(saved).map(Number), 1) ? 'disabled' : ''} class="${index + 1 === current ? 'active' : ''} ${saved[index + 1] ? 'done' : ''}"><span>${saved[index + 1] ? '✓' : index + 1}</span>${item.title}</button>`).join('');
  }
  async function load() {
    q('[data-loading]').hidden = false; q('[data-error]').hidden = true; q('[data-onboarding-form]').hidden = true;
    const result = await window.AdminApiClient.adminGet('/api/branch-onboarding');
    q('[data-loading]').hidden = true;
    if (!result.success && result.connectionError) { q('[data-error]').hidden = false; return; }
    const data = result.data || result; saved = data.steps || {}; current = data.isCompleted ? 10 : (data.currentStep || 1);
    q('[data-onboarding-form]').hidden = false; render();
  }
  q('[data-onboarding-form]').addEventListener('submit', async event => {
    event.preventDefault(); const form = event.currentTarget; const error = q('[data-form-error]'); error.hidden = true;
    if (!form.reportValidity()) return;
    const button = q('[data-next]'); window.AdminApiClient.setLoading(button, true);
    if (current === 10) {
      const response = await window.AdminApiClient.adminPost('/api/branch-onboarding/complete', {});
      window.AdminApiClient.setLoading(button, false);
      if (!response.success && response.httpStatus) { error.textContent = response.message; error.hidden = false; return; }
      form.hidden = true; q('[data-complete]').hidden = false; window.AdminToast?.showSuccess?.('Configuração concluída com sucesso.'); return;
    }
    const payload = {}; new FormData(form).forEach((value,key) => { payload[key] = value; });
    steps[current - 1].fields.filter(x => x[2] === 'checkbox').forEach(x => { payload[x[0]] = form.elements[x[0]].checked; });
    const response = await window.AdminApiClient.adminPut(`/api/branch-onboarding/steps/${current}`, payload); window.AdminApiClient.setLoading(button, false);
    if (!response.success && response.httpStatus) { error.textContent = response.detail || response.message || 'Revise os campos.'; error.hidden = false; return; }
    saved[current] = payload; current += 1; render(); window.AdminToast?.showSuccess?.('Etapa salva.');
  });
  document.addEventListener('click', event => { const go = event.target.closest('[data-go]'); if (go && !go.disabled) { current = Number(go.dataset.go); render(); } if (event.target.closest('[data-back]') && current > 1) { current -= 1; render(); } if (event.target.closest('[data-retry]')) load(); });
  document.addEventListener('DOMContentLoaded', load);
})();
