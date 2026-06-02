(() => {
  const steps = [
    ['Dashboard', 'Visão executiva da operação, saúde do negócio e alertas prioritários.', '[data-tour-target="dashboard"], .page-header'],
    ['KPIs', 'Receita, ocupação, recorrência, satisfação, estoque e conversão do site em um só bloco.', '[data-tour-target="kpis"], .kpi-card'],
    ['Agenda do dia', 'O fluxo acompanha agendado, check-in, atendimento, comanda, pagamento e avaliação.', '[data-tour-target="agenda"]'],
    ['Clientes', 'Abrimos a base CRM e o Cliente 360 com segmento, score e próxima melhor ação.', 'a[href="/Admin/Clients"], [data-tour-target="clients"]', '/Admin/Clients'],
    ['Cliente 360', 'Drawer lateral reúne histórico, preferências, cashback e ações rápidas.', '[data-tour-target="clients"]'],
    ['Agenda', 'Status da operação mudam imediatamente: confirmar, check-in, iniciar e finalizar.', 'a[href="/Admin/Appointments"], [data-tour-target="appointments"]', '/Admin/Appointments'],
    ['Confirmar agendamento', 'A confirmação demo atualiza localStorage e mostra toast para a apresentação.', '[data-demo-smart-action]'],
    ['Comanda', 'PDV visual com itens, descontos, cupom, cashback e meios de pagamento.', 'a[href="/Admin/ServiceOrders"], [data-tour-target="serviceorders"]', '/Admin/ServiceOrders'],
    ['Pagamento demo', 'Registrar pagamento fecha a comanda e prepara recibo visual/impressão.', '[data-demo-smart-action]'],
    ['Estoque crítico', 'Reposição inteligente destaca mínimos, giro e sugestão de compra.', 'a[href="/Admin/Stock"], [data-tour-target="stock"]', '/Admin/Stock'],
    ['Campanhas', 'Campanhas mostram funil, público, impacto, receita estimada e cupons.', 'a[href="/Admin/Campaigns"], [data-tour-target="campaigns"]', '/Admin/Campaigns'],
    ['Copilot', 'O Copilot transforma métricas em ações recomendadas por prioridade.', 'a[href="/Admin/Copilot"], [data-tour-target="copilot"]', '/Admin/Copilot'],
    ['Totem', 'O totem reduz fila com autoatendimento, pagamento mock e avaliação.', 'a[href="/Admin/Kiosk"]'],
    ['Finalização', 'Demo Experience 2.0 pronta para roteiro comercial PublicWeb → Admin → Totem → Mobile.', '.admin-topbar']
  ].map(([title, text, selector, href]) => ({ title, text, selector, href }));
  let index = 0;
  const q = s => document.querySelector(s);
  const overlay = () => q('[data-tour-backdrop]');
  const high = () => q('[data-tour-highlight]');
  function render() {
    const step = steps[index];
    if (!overlay()) return;
    overlay().hidden = false;
    q('[data-tour-title]').textContent = step.title;
    q('[data-tour-text]').textContent = step.text;
    q('[data-tour-progress]').textContent = `Passo ${index + 1} de ${steps.length}`;
    q('[data-tour-progress-bar]').style.width = `${((index + 1) / steps.length) * 100}%`;
    document.querySelectorAll('.tour-target-active').forEach(el => el.classList.remove('tour-target-active'));
    const target = q(step.selector);
    if (target && high()) {
      const rect = target.getBoundingClientRect(); target.classList.add('tour-target-active'); high().hidden = false;
      high().style.left = `${Math.max(8, rect.left - 8)}px`; high().style.top = `${Math.max(8, rect.top - 8)}px`; high().style.width = `${Math.min(innerWidth - 16, rect.width + 16)}px`; high().style.height = `${Math.min(innerHeight - 16, rect.height + 16)}px`;
    }
    if (step.href && !location.pathname.endsWith(step.href.replace('/Admin/', ''))) setTimeout(() => { sessionStorage.setItem('BarberSync:TourIndex', String(index)); location.href = step.href; }, 700);
  }
  function close() { if (overlay()) overlay().hidden = true; if (high()) high().hidden = true; document.querySelectorAll('.tour-target-active').forEach(el => el.classList.remove('tour-target-active')); sessionStorage.removeItem('BarberSync:TourIndex'); }
  function next() { if (index >= steps.length - 1) return close(); index += 1; sessionStorage.setItem('BarberSync:TourIndex', String(index)); render(); }
  function prev() { index = Math.max(0, index - 1); sessionStorage.setItem('BarberSync:TourIndex', String(index)); render(); }
  document.addEventListener('click', event => { if (event.target.closest('[data-tour-start]')) { index = 0; render(); } if (event.target.closest('[data-tour-next]')) next(); if (event.target.closest('[data-tour-prev]')) prev(); if (event.target.closest('[data-tour-skip]')) close(); });
  document.addEventListener('DOMContentLoaded', () => { const saved = sessionStorage.getItem('BarberSync:TourIndex'); if (saved !== null) { index = Number(saved) || 0; setTimeout(render, 350); } });
  window.BarberSyncDemoTour = { start: () => { index = 0; render(); }, next, prev, close };
})();
