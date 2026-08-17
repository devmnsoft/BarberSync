(() => {
  'use strict';
  const $ = selector => document.querySelector(selector);
  const money = value => Number(value || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  const number = value => Number(value || 0).toLocaleString('pt-BR');
  const escape = value => String(value ?? '').replace(/[&<>'"]/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[character]));
  const unwrap = payload => payload?.data ?? payload;
  const list = payload => {
    const value = unwrap(payload);
    return Array.isArray(value) ? value : (value?.items || []);
  };
  const get = (object, ...keys) => keys.reduce((value, key) => value ?? object?.[key], undefined);
  const status = item => String(get(item, 'status', 'appointmentStatus') || '').toLowerCase().replace(/[^a-z]/g, '');

  async function request(path) {
    const response = await fetch(path, { headers: { Accept: 'application/json' } });
    const payload = await response.json().catch(() => null);
    if (!response.ok || !payload || payload.isDemo || payload?.data?.isDemo) throw Object.assign(new Error(payload?.detail || payload?.message || 'API indisponível'), { traceId: payload?.traceId });
    return payload;
  }

  function metric(name, value, explanation, href, trend = null) {
    const trendClass = !trend ? 'neutral' : Number(trend) < 0 ? 'negative' : '';
    const trendText = trend == null ? 'Hoje' : `${Number(trend) > 0 ? '+' : ''}${trend}%`;
    return `<article class="premium-kpi"><div class="kpi-top"><span class="kpi-name">${name}</span><span class="trend ${trendClass}">${trendText}</span></div><strong>${value}</strong><small>${explanation}</small><a href="${href}">Ver detalhes →</a></article>`;
  }

  function operationGroups(appointments, orders) {
    const now = new Date();
    const groups = { waiting: [], inService: [], payment: [], late: [], next: [] };
    appointments.forEach(item => {
      const state = status(item);
      const start = new Date(get(item, 'startAt', 'scheduledAt', 'dateTime', 'startTime'));
      if (['checkedin', 'waiting', 'aguardando'].includes(state)) groups.waiting.push(item);
      if (['inservice', 'inprogress', 'ematendimento'].includes(state)) groups.inService.push(item);
      if (start < now && ['scheduled', 'confirmed', 'agendado', 'confirmado'].includes(state)) groups.late.push(item);
      if (start >= now && start.toDateString() === now.toDateString()) groups.next.push(item);
    });
    orders.filter(item => ['open', 'awaitingpayment', 'aberta', 'aguardandopagamento'].includes(status(item))).forEach(item => groups.payment.push(item));
    return groups;
  }

  function renderOperations(groups, selected = 'waiting') {
    Object.entries(groups).forEach(([key, values]) => { const counter = $(`[data-count="${key}"]`); if (counter) counter.textContent = values.length; });
    const host = $('[data-operation-list]');
    const items = groups[selected] || [];
    host.innerHTML = items.length ? items.slice(0, 8).map(item => {
      const start = new Date(get(item, 'startAt', 'scheduledAt', 'dateTime', 'startTime'));
      const time = Number.isNaN(start.getTime()) ? 'Agora' : start.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
      const client = get(item, 'clientName', 'customerName', 'name') || 'Cliente não informado';
      const service = get(item, 'serviceName', 'description', 'service') || 'Atendimento';
      const professional = get(item, 'professionalName', 'barberName', 'professional') || 'Primeiro disponível';
      const id = encodeURIComponent(get(item, 'id', 'appointmentId', 'code') || '');
      const action = selected === 'waiting' ? ['Iniciar', `/Admin/Operations?id=${id}`] : selected === 'payment' ? ['Receber', `/Admin/ServiceOrders?id=${id}`] : ['Abrir', `/Admin/Appointments?id=${id}`];
      return `<article class="operation-item"><time class="operation-time">${escape(time)}</time><div class="operation-info"><strong>${escape(client)}</strong><small>${escape(service)} · ${escape(professional)}</small></div><a class="operation-action" href="${action[1]}">${action[0]}</a></article>`;
    }).join('') : '<div class="empty-operation"><strong>Nenhum item nesta etapa</strong><p>A operação está em dia.</p></div>';
  }

  function renderInsights(appointments, products, clients) {
    const insights = [];
    const critical = products.filter(product => Number(get(product, 'stock', 'quantity', 'currentStock')) <= Number(get(product, 'minStock', 'minimumStock') || 0));
    if (critical.length) insights.push(`${critical.length} ${critical.length === 1 ? 'produto precisa' : 'produtos precisam'} de reposição, incluindo ${get(critical[0], 'name', 'productName')}.`);
    const byProfessional = appointments.reduce((map, item) => { const name = get(item, 'professionalName', 'barberName'); if (name) map[name] = (map[name] || 0) + 1; return map; }, {});
    const busiest = Object.entries(byProfessional).sort((a, b) => b[1] - a[1])[0];
    if (busiest) insights.push(`${busiest[0]} concentra ${busiest[1]} agendamentos no período selecionado.`);
    const inactive = clients.filter(client => { const date = new Date(get(client, 'lastVisitAt', 'lastVisit')); return !Number.isNaN(date.getTime()) && (Date.now() - date.getTime()) / 86400000 >= 30; });
    if (inactive.length) insights.push(`${inactive.length} clientes estão há mais de 30 dias sem retornar.`);
    if (!insights.length) insights.push('Nenhuma exceção operacional foi identificada nos dados de hoje.');
    $('[data-dashboard-insights]').innerHTML = insights.map(text => `<div class="insight"><span class="insight-marker"></span><p>${escape(text)}</p></div>`).join('');

    const alerts = critical.slice(0, 3).map(product => ({ text: `${get(product, 'name', 'productName')} está no estoque mínimo.`, href: '/Admin/Stock', label: 'Repor produto', priority: 'high' }));
    $('[data-dashboard-alerts]').innerHTML = alerts.length ? alerts.map(alert => `<div class="action-alert"><span class="alert-marker ${alert.priority}"></span><div><p>${escape(alert.text)}</p><a href="${alert.href}">${escape(alert.label)} →</a></div></div>`).join('') : '<p class="dashboard-empty">Nenhum alerta acionável no momento.</p>';
  }

  async function load() {
    $('[data-dashboard-loading]').hidden = false;
    $('[data-dashboard-error]').hidden = true;
    $('[data-dashboard-kpis]').hidden = true;
    $('[data-dashboard-content]').hidden = true;
    try {
      const [dashboardPayload, appointmentsPayload, ordersPayload, productsPayload, clientsPayload] = await Promise.all([
        request('/AdminApi/executive/owner'), request('/AdminApi/appointments'), request('/AdminApi/service-orders'), request('/AdminApi/products'), request('/AdminApi/clients')
      ]);
      const summary = unwrap(dashboardPayload)?.metrics || {};
      const appointments = list(appointmentsPayload), orders = list(ordersPayload), products = list(productsPayload), clients = list(clientsPayload);
      const completed = appointments.filter(item => ['finished', 'completed', 'finalizado', 'concluido'].includes(status(item))).length;
      const openOrders = orders.filter(item => ['open', 'aberta', 'awaitingpayment', 'aguardandopagamento'].includes(status(item))).length;
      const revenue = Number(get(summary, 'revenueToday', 'todayRevenue', 'revenue') || 0);
      const tickets = Number(get(summary, 'paidOrders', 'completedOrders') || 0);
      const critical = products.filter(product => Number(get(product, 'stock', 'quantity', 'currentStock')) <= Number(get(product, 'minStock', 'minimumStock') || 0)).length;
      const metrics = [
        ['Faturamento hoje', money(revenue), 'Recebimentos confirmados no dia.', '/Admin/Financial', get(summary, 'revenueVariation')],
        ['Faturamento do mês', money(get(summary, 'revenueMonth') || 0), 'Recebimentos confirmados no mês.', '/Admin/Financial', null],
        ['Clientes novos', number(get(summary, 'newClients') || 0), 'Cadastros realizados no mês.', '/Admin/Clients', null],
        ['Clientes recorrentes', number(get(summary, 'recurringClients') || 0), 'Clientes com mais de uma visita.', '/Admin/Clients', null],
        ['Agendamentos hoje', number(get(summary, 'appointmentsToday') ?? appointments.length), 'Horários registrados na agenda.', '/Admin/Appointments', get(summary, 'appointmentsVariation')],
        ['Atendimentos concluídos', number(get(summary, 'completedAttendances') ?? completed), 'Serviços finalizados pela equipe.', '/Admin/Operations', get(summary, 'attendanceVariation')],
        ['Comandas abertas', number(get(summary, 'openOrders') ?? openOrders), 'Comandas que ainda exigem conclusão.', '/Admin/ServiceOrders', null],
        ['Ticket médio', money(get(summary, 'averageTicket') ?? (tickets ? revenue / tickets : 0)), 'Média das vendas recebidas hoje.', '/Admin/Financial', get(summary, 'averageTicketVariation')],
        ['Ocupação da agenda', `${number(get(summary, 'occupancyRate', 'occupancy') || 0)}%`, 'Capacidade reservada no expediente.', '/Admin/Appointments', get(summary, 'occupancyVariation')],
        ['Tempo médio de espera', `${number(get(summary, 'averageWaitMinutes') || 0)} min`, 'Do check-in ao início do serviço.', '/Admin/Operations', null],
        ['Estoque crítico', number(critical), 'Produtos no mínimo ou sem saldo.', '/Admin/Stock', null]
      ];
      $('[data-dashboard-kpis]').innerHTML = metrics.map(values => metric(...values)).join('');
      const groups = operationGroups(appointments, orders);
      renderOperations(groups);
      document.querySelectorAll('[data-operation-filter]').forEach(button => button.onclick = () => {
        document.querySelectorAll('[data-operation-filter]').forEach(item => item.classList.remove('active'));
        button.classList.add('active'); renderOperations(groups, button.dataset.operationFilter);
      });
      renderInsights(appointments, products, clients);
      $('[data-dashboard-kpis]').hidden = false; $('[data-dashboard-content]').hidden = false;
    } catch (error) {
      $('[data-dashboard-error]').hidden = false;
      const paragraph = $('[data-dashboard-error] p');
      if (paragraph) paragraph.textContent = `${error.message} · traceId: ${error.traceId || 'não informado pela API'}`;
    } finally { $('[data-dashboard-loading]').hidden = true; }
  }
  document.addEventListener('DOMContentLoaded', load);
  $('[data-dashboard-retry]')?.addEventListener('click', load);
})();
