(() => {
  const answers = document.getElementById('CopilotAnswers');
  function renderSuggestions() {
    if (!answers || !window.BarberSyncDemoStore) return;
    answers.innerHTML = window.BarberSyncDemoStore.get('copilotSuggestions').map(s => `<article class="feature-card"><span class="badge badge-info">${s.type}</span><h4>${s.title}</h4><p>${s.description}</p><button class="btn btn-primary" data-copilot-action="${s.action}">${s.title}</button></article>`).join('');
  }
  document.addEventListener('click', e => {
    const b=e.target.closest('[data-copilot-action]'); if(!b) return; const store=window.BarberSyncDemoStore;
    if (b.dataset.copilotAction === 'createCampaign') store.createCampaign({ name:'Campanha de retorno Copilot', audience:'Clientes inativos', createCoupon:true });
    if (b.dataset.copilotAction === 'stockReplenishment') { store.add('copilotSuggestions', { type:'stock', title:'Pedido de reposição criado', description:'Comprar 12 Pomada Modeladora.', action:'stockReplenishment' }); window.BarberSyncEventBus?.emit('stock:changed', { message:'Copilot criou sugestão de reposição de estoque' }); }
    if (b.dataset.copilotAction === 'fillSchedule') store.createCampaign({ name:'Preencher horários vazios', audience:'Todos', createCoupon:true, code:'AGENDA15' });
    renderSuggestions();
  });
  document.getElementById('CopilotAsk')?.addEventListener('submit', e => { e.preventDefault(); window.AdminToast?.show?.('Copilot analisou o DemoStore e atualizou as sugestões.', 'success'); renderSuggestions(); });
  document.addEventListener('DOMContentLoaded', renderSuggestions);
})();
