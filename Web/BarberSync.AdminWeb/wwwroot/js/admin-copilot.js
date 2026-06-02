(() => {
  const answers = document.getElementById('CopilotAnswers');
  const responseHtml = (question) => `<article class="feature-card"><span class="priority-high">Alta</span><h4>Resposta para: ${question}</h4><p>Priorize reposição de estoque, confirmação da agenda e campanha para clientes inativos. A recomendação foi gerada em modo demonstração seguro.</p><div class="quick-actions-grid"><a class="btn btn-light" href="/Admin/Campaigns">Criar campanha</a><a class="btn btn-light" href="/Admin/Stock">Abrir estoque</a><a class="btn btn-light" href="/Admin/Appointments">Abrir agenda</a><a class="btn btn-light" href="/Admin/Clients">Ver clientes</a></div></article>`;
  document.getElementById('CopilotAsk')?.addEventListener('submit', async e => {
    e.preventDefault();
    const question = new FormData(e.target).get('question');
    try { await fetch('/AdminApi/copilot/ask', { method:'POST', headers:{'Content-Type':'application/json'}, body: JSON.stringify({ question }) }); } catch {}
    answers?.insertAdjacentHTML('afterbegin', responseHtml(question));
    window.AdminToast?.showSuccess?.('Copilot respondeu com ações navegáveis.');
    e.target.reset();
  });
  document.querySelectorAll('[data-quick-question]').forEach(button => button.addEventListener('click', () => {
    answers?.insertAdjacentHTML('afterbegin', responseHtml(button.dataset.quickQuestion));
    window.AdminToast?.showInfo?.('Pergunta rápida enviada ao Copilot.');
  }));
})();
