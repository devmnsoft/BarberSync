(() => {
  'use strict';
  const root = document.querySelector('[data-client-id]');
  if (!root) return;
  const id = root.dataset.clientId;
  const $ = selector => root.querySelector(selector);
  const escapeHtml = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
  const money = value => Number(value || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  const date = value => value ? new Date(value).toLocaleString('pt-BR', { dateStyle: 'medium', timeStyle: 'short' }) : 'Não registrado';
  const fact = (label, value) => `<div><dt>${escapeHtml(label)}</dt><dd>${escapeHtml(value)}</dd></div>`;

  function configureActions() {
    $('[data-action-appointment]').href = `/Admin/Appointments?clientId=${encodeURIComponent(id)}`;
    $('[data-action-order]').href = `/Admin/ServiceOrders?clientId=${encodeURIComponent(id)}`;
    $('[data-action-coupon]').href = `/Admin/Coupons?clientId=${encodeURIComponent(id)}`;
    $('[data-action-campaign]').href = `/Admin/Campaigns?clientId=${encodeURIComponent(id)}`;
  }

  function render(client) {
    $('[data-client-name]').textContent = client.name || 'Cliente sem nome';
    $('[data-client-contact]').textContent = [client.phone, client.email].filter(Boolean).join(' • ') || 'Contato não informado';
    const kpis = [
      ['Gasto total', money(client.totalSpent)], ['Ticket médio', money(client.averageTicket)],
      ['Visitas', Number(client.totalVisits || 0)], ['Dias sem retornar', Number(client.daysWithoutVisit || 0)],
      ['Avaliação média', client.averageRating == null ? 'Sem avaliações' : `${Number(client.averageRating).toFixed(1)} / 5`]
    ];
    $('[data-client-kpis]').innerHTML = kpis.map(([label, value]) => `<article class="kpi-card"><p>${escapeHtml(label)}</p><strong>${escapeHtml(value)}</strong></article>`).join('');
    $('[data-client-profile]').innerHTML = fact('Status', client.status || 'Não informado') + fact('Origem', client.origin || 'Não informada') + fact('WhatsApp', client.whatsapp || 'Não informado') + fact('Cadastro', date(client.registeredAt)) + fact('Etiquetas', (client.tags || []).join(', ') || 'Nenhuma');
    $('[data-client-loyalty]').innerHTML = fact('Saldo', money(client.loyaltyBalance)) + fact('Última visita', date(client.lastVisit)) + fact('Próximo agendamento', date(client.nextAppointment)) + fact('Retorno recomendado', date(client.recommendedReturnAt)) + fact('No-shows', Number(client.noShows || 0));
    const days = Number(client.daysWithoutVisit || 0);
    const risk = days >= 60 ? ['Alto', 'danger'] : days >= 30 ? ['Atenção', 'warning'] : ['Baixo', 'success'];
    $('[data-client-risk]').innerHTML = `<span class="badge badge-${risk[1]}">Risco de abandono: ${risk[0]}</span><p>Calculado a partir de atendimentos concluídos, sem dados simulados.</p>`;
    const events = [
      client.registeredAt && ['Cliente cadastrado', client.registeredAt],
      client.lastVisit && ['Último atendimento concluído', client.lastVisit],
      client.nextAppointment && ['Próximo agendamento', client.nextAppointment],
      client.recommendedReturnAt && ['Retorno recomendado', client.recommendedReturnAt]
    ].filter(Boolean).sort((a, b) => new Date(b[1]) - new Date(a[1]));
    $('[data-client-timeline]').className = events.length ? 'client360-timeline' : 'bs-premium-empty';
    $('[data-client-timeline]').innerHTML = events.length ? events.map(([title, at]) => `<div><i></i><p><strong>${escapeHtml(title)}</strong><time>${escapeHtml(date(at))}</time></p></div>`).join('') : 'Nenhum evento registrado para este cliente.';
  }

  async function load() {
    $('[data-client-loading]').hidden = false; $('[data-client-error]').hidden = true; $('[data-client-content]').hidden = true;
    try {
      const response = await fetch(`/AdminApi/growth/clients/${encodeURIComponent(id)}/360`, { headers: { Accept: 'application/json' } });
      const payload = await response.json().catch(() => ({}));
      if (!response.ok) throw Object.assign(new Error(payload.detail || payload.title || 'A API recusou a consulta.'), { traceId: payload.traceId });
      render(payload.data || payload);
      $('[data-client-content]').hidden = false;
    } catch (error) {
      $('[data-client-error-detail]').textContent = error.message || 'Verifique a API e tente novamente.';
      $('[data-client-trace]').textContent = error.traceId || 'não informado';
      $('[data-client-error]').hidden = false;
    } finally { $('[data-client-loading]').hidden = true; }
  }
  configureActions();
  $('[data-client-refresh]').addEventListener('click', load);
  $('[data-client-retry]').addEventListener('click', load);
  load();
})();
