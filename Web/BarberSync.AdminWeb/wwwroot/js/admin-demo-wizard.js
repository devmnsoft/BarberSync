(() => {
  const STORAGE_KEY = 'barbersync.demoWizard.progress.v18';
  const root = () => document.querySelector('[data-demo-wizard]');
  const bus = () => window.BarberSyncEventBus;
  const store = () => window.BarberSyncDemoStore;
  const toast = (message, type = 'success') => window.AdminToast?.show?.(message, type) || window.BarberSyncToast?.show?.(message, type);
  const statusLabels = { pending: 'Pendente', running: 'Em execução', done: 'Concluído', attention: 'Atenção' };
  const steps = [
    { id:'prepare', title:'Preparar ambiente', description:'Confirme API, Admin, PublicWeb, Kiosk, Docker e assets antes da narrativa comercial.', action:'Preparar', href:'/Admin/Diagnostics', tip:'Mostre que a demo tem um ponto único de partida e não depende de improviso.' },
    { id:'diagnostics', title:'Validar diagnóstico', description:'Execute o Diagnostics para ver API, Swagger, proxies, assets, DemoStore, EventBus e testes JS.', action:'Rodar diagnóstico', href:'/Admin/Diagnostics', tip:'Explique os semáforos: verde OK, amarelo atenção controlada, vermelho falha real.' },
    { id:'scenario', title:'Carregar cenário demo', description:'Restaure dados de clientes, agenda, comandas, estoque, cashback, avaliações e eventos.', action:'Carregar cenário', href:'/Admin/Dashboard', tip:'Destaque que a operação já nasce preenchida para treinamento e venda consultiva.' },
    { id:'dashboard', title:'Abrir Dashboard', description:'Mostre KPIs executivos, status da demonstração, receita, agenda, estoque e eventos recentes.', action:'Abrir Dashboard', href:'/Admin/Dashboard', tip:'Comece pelo resumo de negócio antes dos detalhes operacionais.' },
    { id:'flow', title:'Executar FullServiceFlow', description:'Rode cliente, agendamento, check-in, atendimento, comanda, pagamento, recibo, estoque, cashback e avaliação.', action:'Executar fluxo', href:'/Admin/FullServiceFlow', tip:'Use o botão de fluxo automático quando quiser provar consistência rapidamente.' },
    { id:'client360', title:'Conferir Cliente 360', description:'Valide histórico, segmento, recorrência, cashback, notas e jornada do cliente.', action:'Abrir clientes', href:'/Admin/Clients', tip:'Mostre visão 360 para personalização e retenção.' },
    { id:'agenda', title:'Conferir Agenda', description:'Veja o agendamento criado e os status da operação ao vivo.', action:'Abrir agenda', href:'/Admin/Appointments', tip:'Conecte agenda com check-in e capacidade da equipe.' },
    { id:'order', title:'Conferir Comanda', description:'Abra comandas para serviço/produto, desconto, pagamento e recibo.', action:'Abrir comandas', href:'/Admin/ServiceOrders', tip:'Demonstre PDV integrado com baixa de estoque.' },
    { id:'stock', title:'Conferir Estoque', description:'Valide produtos, movimentos e estoque crítico depois da venda.', action:'Abrir estoque', href:'/Admin/Stock', tip:'Mostre controle de perdas e reposição inteligente.' },
    { id:'review', title:'Conferir Avaliação', description:'Mostre NPS, estrelas, comentário e impacto na reputação.', action:'Abrir avaliações', href:'/Admin/Reviews', tip:'Feche o ciclo de atendimento com satisfação e qualidade.' },
    { id:'publicweb', title:'Abrir PublicWeb', description:'Apresente site público com serviços, profissionais e formulário de agendamento.', action:'Abrir PublicWeb', href:'http://localhost:8082/', external:true, tip:'Mostre o canal de captação sem expor a API interna.' },
    { id:'publicbooking', title:'Realizar agendamento público', description:'Simule envio do formulário e protocolo demo via /PublicApi/appointments.', action:'Validar agendamento', href:'http://localhost:8082/', external:true, tip:'Explique que o lead vira agenda e alimenta a operação.' },
    { id:'kiosk', title:'Abrir Kiosk', description:'Inicie o Totem em /Kiosk/Services com serviços e profissionais via /KioskApi.', action:'Abrir Kiosk', href:'http://localhost:8083/Kiosk/Services', external:true, tip:'Mostre autoatendimento para reduzir fila da recepção.' },
    { id:'kioskflow', title:'Concluir fluxo do Totem', description:'Valide cliente, profissional, confirmação, pagamento mock, sucesso e avaliação.', action:'Marcar Totem OK', href:'http://localhost:8083/Kiosk/Services', external:true, tip:'Destaque sessionStorage, botão voltar, cancelar e reiniciar.' },
    { id:'finish', title:'Finalizar demonstração', description:'Revise resultados, eventos, relatório e próximos passos comerciais.', action:'Finalizar', href:'/Admin/Diagnostics', tip:'Encerre com checklist e roteiro de implantação.' }
  ];
  const safeParse = (value, fallback) => { try { return JSON.parse(value || '') || fallback; } catch { return fallback; } };
  const read = () => ({ statuses: {}, log: [], ...safeParse(localStorage.getItem(STORAGE_KEY), {}) });
  const write = (state) => { try { localStorage.setItem(STORAGE_KEY, JSON.stringify({ ...state, updatedAt: new Date().toISOString() })); } catch { /* localStorage pode estar indisponível */ } };
  const classFor = status => status === 'done' ? 'is-done' : status === 'running' ? 'is-running' : status === 'attention' ? 'is-attention' : '';
  function render() {
    if (!root()) return;
    const state = read();
    const completed = steps.filter(step => state.statuses[step.id] === 'done').length;
    const list = document.querySelector('[data-wizard-steps]');
    if (list) list.innerHTML = steps.map((step, index) => {
      const status = state.statuses[step.id] || 'pending';
      return `<article class="demo-wizard-step ${classFor(status)}" data-wizard-step="${step.id}"><div class="demo-wizard-step-number">${index + 1}</div><div><span class="demo-wizard-step-status"><i class="wizard-dot ${status}"></i>${statusLabels[status]}</span><h3>${step.title}</h3><p>${step.description}</p><div class="demo-wizard-tip">Dica: ${step.tip}</div></div><div class="demo-wizard-step-actions"><button class="btn btn-gold" type="button" data-wizard-action="${step.id}">${step.action}</button><a class="btn btn-light" href="${step.href}" ${step.external ? 'target="_blank" rel="noopener"' : ''}>Abrir tela</a></div></article>`;
    }).join('');
    const text = document.querySelector('[data-wizard-progress-text]');
    const bar = document.querySelector('[data-wizard-progress-bar]');
    const log = document.querySelector('[data-wizard-log]');
    if (text) text.textContent = `${completed}/${steps.length}`;
    if (bar) bar.style.width = `${Math.round((completed / steps.length) * 100)}%`;
    if (log) log.innerHTML = (state.log || []).slice(-5).reverse().map(item => `<div><strong>${item.title}</strong><br><small>${new Date(item.at).toLocaleString('pt-BR')}</small></div>`).join('') || 'Nenhum evento do wizard ainda.';
  }
  function mark(id, status, title) {
    const state = read();
    state.statuses[id] = status;
    state.log = [...(state.log || []), { id, status, title, at: new Date().toISOString() }].slice(-30);
    write(state);
    bus()?.emit?.('demoWizard:stepUpdated', { module:'Demo Wizard', title, step:id, status, severity: status === 'attention' ? 'warning' : 'success' });
    render();
  }
  function execute(id) {
    mark(id, 'running', `Etapa em execução: ${id}`);
    const s = store();
    if (id === 'scenario') s?.resetAll?.();
    if (id === 'flow') {
      const autoButton = document.querySelector('[data-flow-auto-validate]');
      if (autoButton) autoButton.click(); else s?.createFullServiceFlow?.();
    }
    if (id === 'publicbooking') {
      s?.createLeadFromPublicWeb?.({ name:'Cliente PublicWeb Demo Wizard', phone:'(11) 97777-1919', service:'Corte + Barba', protocol:`WIZ-${Date.now()}`, status:'Novo' });
    }
    if (id === 'kioskflow') {
      s?.createAttendanceFromKiosk?.({ client:'Cliente Totem Demo Wizard', service:'Corte Masculino', professional:'Rafael Barber', status:'Completed', protocol:`KIOSK-${Date.now()}` });
    }
    setTimeout(() => {
      mark(id, 'done', `${steps.find(step => step.id === id)?.title || id} concluído`);
      toast(`Demo Wizard: ${steps.find(step => step.id === id)?.title || id} concluído.`);
    }, 220);
  }
  document.addEventListener('click', event => {
    const action = event.target.closest('[data-wizard-action]');
    if (action) { event.preventDefault(); execute(action.dataset.wizardAction); }
    if (event.target.closest('[data-wizard-start]')) { event.preventDefault(); execute('prepare'); }
    if (event.target.closest('[data-wizard-reset]')) { event.preventDefault(); write({ statuses:{}, log:[] }); bus()?.emit?.('demoWizard:reset', { module:'Demo Wizard', title:'Progresso do Demo Wizard resetado', severity:'warning' }); render(); toast('Progresso do Demo Wizard resetado.', 'warning'); }
  });
  window.addEventListener('barbersync:event', render);
  document.addEventListener('DOMContentLoaded', render);
})();
