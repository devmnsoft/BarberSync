(() => {
  const steps = [
    ['welcome','👋','Bem-vindo ao BarberSync','Entenda a proposta: agenda, caixa, estoque, PublicWeb, Totem, Mobile e Copilot no mesmo SaaS.','/Admin/Dashboard'],
    ['company','🏢','Configure sua empresa','Revise unidade, horários, política de atendimento e dados comerciais.','/Admin/Settings'],
    ['services','✂️','Cadastre serviços','Crie serviços com preço, duração e canais PublicWeb, Totem e Mobile.','/Admin/Services'],
    ['professionals','💈','Cadastre profissionais','Configure agenda, especialidades, comissão e metas.','/Admin/Professionals'],
    ['clients','👥','Cadastre clientes','Mostre CRM 360º com VIP, cashback, histórico e próxima ação.','/Admin/Clients'],
    ['appointments','📅','Crie seu primeiro agendamento','Demonstre o status de agendado até finalizado.','/Admin/Appointments'],
    ['orders','🧾','Abra uma comanda','Mostre itens, pagamento e recibo visual.','/Admin/ServiceOrders'],
    ['kiosk','🖥️','Configure o Totem','Teste o fluxo guiado de autoatendimento.','/Admin/Kiosk'],
    ['publicweb','🌐','Veja o PublicWeb','Apresente a landing vendável e o agendamento público.','http://localhost:8082/'],
    ['copilot','🤖','Use o Copilot','Faça uma pergunta e converta sugestão em ação.','/Admin/Copilot']
  ];
  const key = 'BarberSync:onboarding:v1';
  const read = () => JSON.parse(localStorage.getItem(key) || '{}');
  const write = value => localStorage.setItem(key, JSON.stringify(value));
  function render() {
    const host = document.querySelector('[data-onboarding-steps]');
    if (!host) return;
    const state = read();
    const done = steps.filter(([id]) => state[id] === 'done').length;
    document.querySelector('[data-onboarding-percent]').textContent = `${Math.round(done / steps.length * 100)}%`;
    document.querySelector('[data-onboarding-done]').textContent = done;
    host.innerHTML = steps.map(([id, icon, title, desc, href]) => {
      const status = state[id] === 'done' ? 'Concluído' : state[id] === 'demo' ? 'Em demonstração' : 'Pendente';
      const cls = state[id] === 'done' ? 'done' : state[id] === 'demo' ? 'demo' : 'pending';
      return `<article class="onboarding-step ${cls}"><div class="step-icon">${icon}</div><div><h4>${title}</h4><p>${desc}</p><span class="step-status">${status}</span></div><div class="step-actions"><a class="btn btn-light" href="${href}">Ação</a><button class="btn btn-primary" data-onboarding-toggle="${id}">Marcar</button></div></article>`;
    }).join('');
  }
  document.addEventListener('click', e => {
    const toggle = e.target.closest('[data-onboarding-toggle]');
    const complete = e.target.closest('[data-onboarding-complete]');
    if (toggle || complete) {
      const id = toggle?.dataset.onboardingToggle || complete?.dataset.onboardingComplete;
      const state = read(); state[id] = state[id] === 'done' ? 'demo' : 'done'; write(state); render();
      window.AdminToast?.showSuccess?.('Ação simulada com sucesso em modo demonstração.');
    }
    if (e.target.closest('[data-onboarding-reset]')) { localStorage.removeItem(key); render(); window.AdminToast?.showInfo?.('Onboarding reiniciado.'); }
  });
  document.addEventListener('DOMContentLoaded', render); render();
})();
