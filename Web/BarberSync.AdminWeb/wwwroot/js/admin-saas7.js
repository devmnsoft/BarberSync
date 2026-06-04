(() => {
  const now = () => new Date().toISOString();
  const money = value => Number(value || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  const q = selector => document.querySelector(selector);
  const all = selector => Array.from(document.querySelectorAll(selector));
  const toast = message => window.AdminToast?.showSuccess?.(message) || window.AdminToast?.showInfo?.(message) || console.info(message);
  const clone = value => JSON.parse(JSON.stringify(value));

  const defaults = {
    platform: {
      tradeName: 'Barbearia Elite Matriz', legalName: 'BarberSync Demo LTDA', cnpj: '12.345.678/0001-90', phone: '(91) 3333-3030', whatsapp: '(91) 98888-7777', email: 'contato@barbersync.demo', address: 'Av. Demo SaaS, 700', cityUf: 'Belém/PA', city: 'Belém', state: 'PA',
      logo: 'BarberSync', primary: '#111827', secondary: '#d4af37', theme: 'Claro', publicName: 'BarberSync Elite', kioskName: 'Totem BarberSync', slogan: 'Agenda, PDV, caixa, estoque e canais em uma experiência premium.',
      hours: 'Seg a Sex 08:00-20:00 • Sáb 09:00-18:00 • Dom 09:00-13:00', holidays: 'Feriado demo: atendimento reduzido', days: ['Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'], start: '08:00', end: '20:00', interval: '10 min',
      minAdvance: '30 min', serviceInterval: '10 min', delayTolerance: '15 min', cancelPolicy: 'Cancelamento até 2h antes.', autoConfirm: true,
      commission: '40', cashback: '5', maxDiscount: '20', payments: ['PIX', 'Cartão', 'Dinheiro', 'Recepção'], channels: { publicWeb: true, kiosk: true, mobile: true, copilot: true }
    },
    publicSite: {
      active: true, updatedAt: now(), leads: 42, appointmentRequests: 18, heroTitle: 'Barbearia premium conectada ao BarberSync', heroSubtitle: 'Agende online, ganhe cashback e experimente uma operação omnichannel.', ctaPrimary: 'Agendar agora', ctaSecondary: 'Solicitar demonstração', image: 'hero-salon-premium.svg',
      seoTitle: 'BarberSync Elite | Agendamento online', seoDescription: 'Site público demonstrativo integrado ao Admin BarberSync.', keywords: 'barbearia, agenda online, totem, cashback',
      promotions: [{ title: 'Combo Primeira Visita', description: 'Corte + Barba com 15% OFF', coupon: 'BEMVINDO15', validUntil: '2026-12-31', banner: 'Banner demo premium' }]
    },
    kiosk: {
      active: true, message: 'Bem-vindo ao Totem BarberSync. Escolha seu serviço para começar.', timeout: 90, highContrast: false, largeFont: false, attendant: true, autoReset: true,
      payments: { pix: true, card: true, cash: false, reception: true, mock: true },
      devices: [{ code: 'KIOSK-DEMO-001', name: 'Totem Entrada Premium', branch: 'Barbearia Elite Matriz', status: 'Online', lastSeen: 'agora', version: '8.0.0', demo: true }],
      logs: [{ type: 'Atendimento', text: 'Combo Corte + Barba iniciado no Totem', at: 'há 4 min' }, { type: 'Pagamento mock', text: 'PIX demo aprovado', at: 'há 7 min' }, { type: 'Avaliação', text: 'Cliente avaliou com 5 estrelas', at: 'há 12 min' }]
    },
    mobile: { active: true, cashback: true, notifications: true, history: true, promotions: true },
    users: [{ name: 'Ana Admin', email: 'ana@barbersync.demo', role: 'Administrador', branch: 'Todas', active: true, permissions: ['Dashboard', 'Clientes', 'Agenda', 'Comandas', 'Caixa', 'Estoque', 'Campanhas', 'Relatórios', 'Configurações', 'Totem', 'PublicWeb'] }, { name: 'Rafael Recepção', email: 'rafael@barbersync.demo', role: 'Recepção', branch: 'Barbearia Elite Matriz', active: true, permissions: ['Dashboard', 'Clientes', 'Agenda', 'Comandas'] }, { name: 'Bia Estoque', email: 'bia@barbersync.demo', role: 'Estoque', branch: 'BarberSync Shopping Demo', active: true, permissions: ['Estoque', 'Relatórios'] }],
    profiles: ['Administrador', 'Gerente', 'Recepção', 'Profissional', 'Financeiro', 'Estoque', 'Marketing', 'Consulta'],
    permissionModules: ['Dashboard', 'Clientes', 'Agenda', 'Comandas', 'Caixa', 'Estoque', 'Campanhas', 'Relatórios', 'Configurações', 'Totem', 'PublicWeb'],
    branches: [{ name: 'Barbearia Elite Matriz', status: 'Ativa', revenue: 68400, appointments: 730, professionals: 12, ticket: 94, rating: 4.9, occupancy: 88, critical: 2, leads: 34, kiosk: 88 }, { name: 'BarberSync Studio Umarizal', status: 'Ativa', revenue: 45200, appointments: 520, professionals: 8, ticket: 87, rating: 4.8, occupancy: 81, critical: 1, leads: 21, kiosk: 55 }, { name: 'BarberSync Estética Premium', status: 'Ativa', revenue: 51600, appointments: 410, professionals: 9, ticket: 126, rating: 4.7, occupancy: 77, critical: 3, leads: 18, kiosk: 39 }, { name: 'BarberSync Shopping Demo', status: 'Implantação', revenue: 29800, appointments: 330, professionals: 6, ticket: 90, rating: 4.6, occupancy: 69, critical: 4, leads: 12, kiosk: 22 }],
    notifications: [{ title: 'Agendamento confirmado', module: 'Agenda', type: 'success', read: false, route: '/Admin/Appointments' }, { title: 'Cliente chegou no Totem', module: 'Totem', type: 'info', read: false, route: '/Admin/Kiosk' }, { title: 'Estoque crítico de pomada', module: 'Estoque', type: 'danger', read: false, route: '/Admin/Stock' }, { title: 'Comanda pendente no caixa', module: 'Caixa', type: 'warning', read: false, route: '/Admin/Cash' }, { title: 'Pagamento recebido', module: 'PDV', type: 'success', read: true, route: '/Admin/ServiceOrders' }, { title: 'Copilot recomenda campanha de retorno', module: 'Copilot', type: 'info', read: false, route: '/Admin/Copilot' }, { title: 'PublicWeb recebeu lead', module: 'PublicWeb', type: 'success', read: false, route: '/Admin/PublicSite' }],
    services: [{ name: 'Corte Masculino', category: 'Cortes', price: 45, durationMinutes: 40, public: true, kiosk: true, mobile: true, highlight: true, order: 1, combo: false }, { name: 'Combo Corte + Barba', category: 'Combos', price: 78, durationMinutes: 70, public: true, kiosk: true, mobile: true, highlight: true, order: 2, combo: true }, { name: 'Barboterapia', category: 'Barba', price: 65, durationMinutes: 45, public: true, kiosk: false, mobile: true, highlight: false, order: 3, combo: false }, { name: 'Hidratação Premium', category: 'Estética', price: 89, durationMinutes: 50, public: false, kiosk: true, mobile: true, highlight: false, order: 4, combo: true }],
    professionals: [{ name: 'Rafael Barber', specialty: 'Corte e barba', rating: 4.9, public: true, highlight: true, order: 1 }, { name: 'Bruna Estética', specialty: 'Estética premium', rating: 4.8, public: true, highlight: true, order: 2 }, { name: 'Marcos Fade', specialty: 'Degradê e desenho', rating: 4.7, public: true, highlight: false, order: 3 }],
    audit: [{ user: 'Ana Admin', module: 'Configurações', type: 'Atualização', severity: 'info', text: 'Identidade visual alterada', at: 'há 3 min' }, { user: 'Totem', module: 'Totem', type: 'Atendimento', severity: 'success', text: 'Comanda mock criada via KIOSK-DEMO-001', at: 'há 8 min' }, { user: 'Copilot', module: 'Marketing', type: 'Ação', severity: 'warning', text: 'Campanha de retorno sugerida', at: 'há 14 min' }, { user: 'Caixa', module: 'PDV', type: 'Pagamento', severity: 'success', text: 'Comanda #1024 paga com PIX', at: 'há 20 min' }, { user: 'Estoque', module: 'Estoque', type: 'Movimentação', severity: 'danger', text: 'Produto abaixo do mínimo', at: 'há 31 min' }]
  };

  function fromDemoStore() {
    const demo = window.BarberSyncDemoStore;
    const settings = demo?.getSettings?.() || {};
    return {
      platform: { ...defaults.platform, ...(settings.company || {}), ...(settings.branding || {}), ...(settings.scheduleRules || {}), ...(settings.paymentRules || {}), commission: settings.paymentRules?.commission ?? defaults.platform.commission, cashback: settings.cashbackRules?.defaultPercent ?? defaults.platform.cashback, channels: { ...defaults.platform.channels, ...(settings.channels || {}) } },
      publicSite: { ...defaults.publicSite, ...(settings.publicWeb || {}) },
      kiosk: { ...defaults.kiosk, ...(settings.kiosk || {}), payments: { ...defaults.kiosk.payments, ...(settings.kiosk?.payments || {}) } },
      mobile: { ...defaults.mobile, ...(settings.mobile || {}) },
      users: settings.permissions?.users || defaults.users,
      profiles: settings.permissions?.profiles || defaults.profiles,
      permissionModules: settings.permissions?.modules || defaults.permissionModules,
      branches: settings.branches?.items || defaults.branches,
      notifications: settings.notifications?.items || defaults.notifications,
      services: settings.publicWeb?.services || settings.kiosk?.services || defaults.services,
      professionals: settings.publicWeb?.professionals || defaults.professionals,
      audit: settings.audit?.items || defaults.audit
    };
  }

  let state = fromDemoStore();

  function persist() {
    const demo = window.BarberSyncDemoStore;
    demo?.updateSettings?.('company', pick(state.platform, ['tradeName', 'legalName', 'cnpj', 'phone', 'whatsapp', 'email', 'address', 'city', 'state', 'cityUf']));
    demo?.updateSettings?.('branding', pick(state.platform, ['logo', 'primary', 'secondary', 'theme', 'slogan', 'publicName', 'kioskName']));
    demo?.updateSettings?.('scheduleRules', pick(state.platform, ['days', 'start', 'end', 'interval', 'hours', 'holidays', 'minAdvance', 'serviceInterval', 'delayTolerance', 'cancelPolicy', 'autoConfirm']));
    demo?.updateSettings?.('paymentRules', { commission: state.platform.commission, maxDiscount: state.platform.maxDiscount, methods: state.platform.payments });
    demo?.updateSettings?.('cashbackRules', { defaultPercent: state.platform.cashback, expirationDays: 90 });
    demo?.updateSettings?.('channels', state.platform.channels);
    demo?.updateSettings?.('publicWeb', { ...state.publicSite, services: state.services, professionals: state.professionals });
    demo?.updateSettings?.('kiosk', { ...state.kiosk, services: state.services });
    demo?.updateSettings?.('mobile', state.mobile);
    demo?.updateSettings?.('permissions', { profiles: state.profiles, modules: state.permissionModules, users: state.users, visualOnly: true });
    demo?.updateSettings?.('branches', { items: state.branches, multiUnit: true, active: state.branches[0]?.name });
    demo?.updateSettings?.('notifications', { enabled: true, unread: state.notifications.filter(n => !n.read).length, items: state.notifications });
    demo?.updateSettings?.('audit', { items: state.audit });
    try { localStorage.setItem('barbersync.saas8.snapshot', JSON.stringify(state)); } catch { /* storage unavailable */ }
    renderTopbarNotifications();
  }

  function pick(source, keys) { return keys.reduce((acc, key) => (source[key] !== undefined ? { ...acc, [key]: source[key] } : acc), {}); }
  function setForm(form, values) { if (!form) return; Object.entries(values).forEach(([key, value]) => { const field = form.elements[key]; if (!field) return; if (field.type === 'checkbox') field.checked = Boolean(value); else field.value = Array.isArray(value) ? value.join(', ') : value ?? ''; }); }
  function formData(form) { const data = Object.fromEntries(new FormData(form).entries()); all(`#${form.id} input[type=checkbox]`).forEach(input => { if (input.name) data[input.name] = input.checked; }); return data; }
  function logAudit(module, text, severity = 'info', user = 'Admin Demo') { state.audit.unshift({ user, module, type: 'Ação demo', severity, text, at: 'agora' }); state.audit = state.audit.slice(0, 80); }
  function notify(title, module, type = 'info', route = '/Admin/Dashboard') { state.notifications.unshift({ title, module, type, route, read: false }); state.notifications = state.notifications.slice(0, 80); }

  function renderTopbarNotifications() { const unread = state.notifications.filter(n => !n.read).length; all('[data-topbar-notification-count]').forEach(el => el.textContent = unread); }
  function renderPublicPreview() { all('[data-saas7-public-preview]').forEach(el => { el.innerHTML = `<div class="saas7-preview-card" style="border-color:${state.platform.secondary}"><span class="badge">${state.publicSite.active ? 'Publicado' : 'Inativo'}</span><h3>${state.publicSite.heroTitle}</h3><p>${state.publicSite.heroSubtitle}</p><button class="btn btn-primary" style="background:${state.platform.primary}">${state.publicSite.ctaPrimary}</button><button class="btn btn-light">${state.publicSite.ctaSecondary}</button></div>`; }); }
  function renderKioskPreview() { all('[data-saas7-kiosk-preview]').forEach(el => { el.innerHTML = `<div class="saas7-kiosk-preview ${state.kiosk.highContrast ? 'contrast' : ''}" style="--accent:${state.platform.secondary}"><strong>${state.platform.kioskName}</strong><p>${state.kiosk.message}</p><small>${state.kiosk.timeout}s • ${state.kiosk.largeFont ? 'Fonte grande' : 'Fonte padrão'} • PIX ${state.kiosk.payments.pix ? 'on' : 'off'}</small><a class="btn btn-primary" href="/Admin/Kiosk">Configurar</a></div>`; }); }

  function initPlatform() {
    const form = q('[data-platform-form]'); if (!form) return;
    setForm(form, state.platform); form.elements.publicWeb.checked = state.platform.channels.publicWeb; form.elements.kiosk.checked = state.platform.channels.kiosk; form.elements.mobile.checked = state.platform.channels.mobile; form.elements.copilot.checked = state.platform.channels.copilot;
    renderPublicPreview(); renderKioskPreview(); window.BarberSyncDemoStore?.applyBranding?.();
    form.addEventListener('submit', e => { e.preventDefault(); const data = formData(form); state.platform = { ...state.platform, ...data, channels: { publicWeb: data.publicWeb, kiosk: data.kiosk, mobile: data.mobile, copilot: data.copilot } }; persist(); window.BarberSyncDemoStore?.applyBranding?.(); renderPublicPreview(); renderKioskPreview(); logAudit('Configurações', 'Configurações da plataforma salvas', 'success'); notify('Configurações da plataforma salvas', 'Configurações', 'success', '/Admin/PlatformSettings'); toast('Configurações salvas no DemoStore v8.'); });
    q('[data-platform-reset]')?.addEventListener('click', () => { state.platform = clone(defaults.platform); persist(); setForm(form, state.platform); renderPublicPreview(); renderKioskPreview(); toast('Configurações padrão restauradas.'); });
  }

  function initPublicSite() {
    if (!q('[data-public-site-page]')) return;
    const form = q('[data-public-site-form]'); setForm(form, state.publicSite); renderPublicPreview();
    q('[data-public-metrics]').innerHTML = `<div class="saas7-metric"><strong>${state.publicSite.active ? 'Ativo' : 'Inativo'}</strong><span>Status</span></div><div class="saas7-metric"><strong>${state.publicSite.leads}</strong><span>Leads recebidos</span></div><div class="saas7-metric"><strong>${state.publicSite.appointmentRequests}</strong><span>Agendamentos solicitados</span></div><div class="saas7-metric"><strong>${Math.round((state.publicSite.appointmentRequests / Math.max(1, state.publicSite.leads)) * 100)}%</strong><span>Conversão</span></div>`;
    q('[data-public-services]').innerHTML = state.services.map((service, index) => `<label class="saas7-switch"><input type="checkbox" data-service-public="${index}" ${service.public ? 'checked' : ''}> #${service.order} ${service.name} • ${money(service.price)} • ${service.category} ${service.highlight ? '⭐' : ''}</label>`).join('');
    q('[data-public-professionals]').innerHTML = state.professionals.map((pro, index) => `<label class="saas7-switch"><input type="checkbox" data-pro-public="${index}" ${pro.public ? 'checked' : ''}> ${pro.name} • ${pro.specialty} • ⭐ ${pro.rating}</label>`).join('');
    form.addEventListener('submit', e => { e.preventDefault(); state.publicSite = { ...state.publicSite, ...formData(form), updatedAt: now() }; window.BarberSyncDemoStore?.publishPublicWebConfig?.(); persist(); renderPublicPreview(); logAudit('PublicWeb', 'Alterações publicadas no site público', 'success'); notify('PublicWeb recebeu atualização', 'PublicWeb', 'success', '/Admin/PublicSite'); toast('PublicWeb publicado e preview atualizado.'); });
    document.addEventListener('change', e => { if (e.target.dataset.servicePublic !== undefined) { state.services[e.target.dataset.servicePublic].public = e.target.checked; logAudit('PublicWeb', `Serviço ${state.services[e.target.dataset.servicePublic].name} ${e.target.checked ? 'publicado' : 'ocultado'}`, 'info'); persist(); renderPublicPreview(); } if (e.target.dataset.proPublic !== undefined) { state.professionals[e.target.dataset.proPublic].public = e.target.checked; logAudit('PublicWeb', `Profissional ${state.professionals[e.target.dataset.proPublic].name} ${e.target.checked ? 'publicado' : 'ocultado'}`, 'info'); persist(); } });
    q('[data-open-public]')?.addEventListener('click', () => { const params = new URLSearchParams({ title: state.publicSite.heroTitle, subtitle: state.publicSite.heroSubtitle, cta: state.publicSite.ctaPrimary, company: state.platform.publicName, slogan: state.platform.slogan, primary: state.platform.primary, secondary: state.platform.secondary }); window.open(`http://localhost:8082/?${params.toString()}`, '_blank', 'noopener'); });
    q('[data-refresh-preview]')?.addEventListener('click', () => { renderPublicPreview(); toast('Preview do PublicWeb atualizado.'); });
  }

  function initKiosk() {
    if (!q('[data-kiosk-settings-page]')) return;
    const form = q('[data-kiosk-form]'); setForm(form, { ...state.kiosk, ...state.kiosk.payments }); renderKioskPreview();
    q('[data-kiosk-devices]').innerHTML = state.kiosk.devices.map((d, index) => `<tr><td>${d.code}</td><td>${d.name}</td><td>${d.branch}</td><td><button class="btn btn-light" data-toggle-device="${index}">${d.status}</button></td><td>${d.lastSeen}</td><td>${d.version} ${d.demo ? '• demo' : ''}</td></tr>`).join('');
    q('[data-kiosk-services]').innerHTML = state.services.map((service, index) => `<label class="saas7-switch"><input type="checkbox" data-service-kiosk="${index}" ${service.kiosk ? 'checked' : ''}> #${service.order} ${service.name} • ${money(service.price)} • ${service.durationMinutes} min ${service.combo ? '• combo sugerido' : ''}</label>`).join('');
    q('[data-kiosk-logs]').innerHTML = state.kiosk.logs.map(log => `<div class="saas7-log"><strong>${log.type}</strong><p>${log.text}</p><small>${log.at}</small></div>`).join('');
    form.addEventListener('submit', e => { e.preventDefault(); const data = formData(form); state.kiosk = { ...state.kiosk, ...data, timeout: Number(data.timeout || 90), payments: { pix: data.pix, card: data.card, cash: data.cash, reception: data.reception, mock: data.mock } }; window.BarberSyncDemoStore?.publishKioskConfig?.(); persist(); renderKioskPreview(); logAudit('Totem', 'Configuração do Totem salva', 'success'); notify('Totem atualizado pelo Admin', 'Totem', 'success', '/Admin/Kiosk'); toast('Configuração do Totem salva.'); });
    document.addEventListener('click', e => { if (e.target.dataset.toggleDevice !== undefined) { const d = state.kiosk.devices[e.target.dataset.toggleDevice]; d.status = d.status === 'Online' ? 'Offline demo' : 'Online'; d.lastSeen = d.status === 'Online' ? 'agora' : 'há 1 min'; persist(); toast(`Totem ${d.status}.`); location.reload(); } });
    document.addEventListener('change', e => { if (e.target.dataset.serviceKiosk !== undefined) { state.services[e.target.dataset.serviceKiosk].kiosk = e.target.checked; logAudit('Totem', `Serviço ${state.services[e.target.dataset.serviceKiosk].name} ${e.target.checked ? 'liberado' : 'bloqueado'} no Totem`, 'info'); persist(); toast('Serviço liberado/ocultado no Totem.'); } });
    q('[data-open-kiosk]')?.addEventListener('click', () => { const params = new URLSearchParams({ message: state.kiosk.message, title: state.platform.kioskName, primary: state.platform.primary, secondary: state.platform.secondary, highContrast: String(state.kiosk.highContrast), largeFont: String(state.kiosk.largeFont) }); window.open(`http://localhost:8083/Kiosk/Services?${params.toString()}`, '_blank', 'noopener'); });
    q('[data-simulate-kiosk]')?.addEventListener('click', () => { state.kiosk.logs.unshift({ type: 'Atendimento', text: 'Simulação criada pelo Kiosk Manager', at: 'agora' }); logAudit('Totem', 'Atendimento simulado pelo Admin', 'success'); notify('Atendimento simulado no Totem', 'Totem', 'info', '/Admin/Kiosk'); persist(); toast('Atendimento do Totem simulado.'); location.reload(); });
  }

  function initUsers() {
    if (!q('[data-users-page]')) return;
    const render = () => {
      q('[data-users-rows]').innerHTML = state.users.map((u, i) => `<tr><td>${u.name}<br><small>${u.email}</small></td><td>${u.role}</td><td>${u.branch}</td><td>${u.permissions.join(', ')}</td><td>${u.active ? 'Ativo' : 'Inativo'}</td><td><button class="btn btn-light" data-edit-user="${i}">Editar</button><button class="btn btn-light" data-reset-user="${i}">Reset senha</button><button class="btn btn-secondary" data-toggle-user="${i}">${u.active ? 'Inativar' : 'Ativar'}</button></td></tr>`).join('');
      q('[data-permission-matrix]')?.remove(); q('[data-users-page]').insertAdjacentHTML('afterend', `<section class="saas7-card full" data-permission-matrix><h2>Matriz perfil x módulo</h2><table class="saas7-table"><thead><tr><th>Perfil</th>${state.permissionModules.map(m => `<th>${m}</th>`).join('')}</tr></thead><tbody>${state.profiles.map(p => `<tr><td>${p}</td>${state.permissionModules.map(m => `<td><label class="saas7-switch"><input type="checkbox" checked data-profile="${p}" data-module="${m}"></label></td>`).join('')}</tr>`).join('')}</tbody></table></section>`);
    };
    q('[data-user-form]').addEventListener('submit', e => { e.preventDefault(); const fd = new FormData(e.target); state.users.push({ name: fd.get('name'), email: fd.get('email'), role: fd.get('role'), branch: fd.get('branch'), active: true, permissions: fd.getAll('perm') }); persist(); render(); logAudit('Usuários', `Usuário ${fd.get('name')} criado`, 'success'); toast('Usuário demo criado.'); e.target.reset(); });
    document.addEventListener('click', e => { const i = e.target.dataset.toggleUser ?? e.target.dataset.resetUser ?? e.target.dataset.editUser; if (i === undefined) return; if (e.target.dataset.toggleUser !== undefined) state.users[i].active = !state.users[i].active; if (e.target.dataset.resetUser !== undefined) notify(`Senha demo resetada para ${state.users[i].name}`, 'Usuários', 'info', '/Admin/Users'); if (e.target.dataset.editUser !== undefined) state.users[i].role = state.users[i].role === 'Gerente' ? 'Administrador' : 'Gerente'; persist(); render(); toast('Ação de usuário executada.'); });
    render();
  }

  function initBranches() {
    if (!q('[data-branches-page]')) return;
    const render = () => { q('[data-branches-chart]').innerHTML = state.branches.map(b => `<div class="saas7-bar" title="${b.name}" style="height:${b.occupancy}%"></div>`).join(''); q('[data-branches-rows]').innerHTML = state.branches.sort((a, b) => b.revenue - a.revenue).map((b, i) => `<tr><td>#${i + 1}</td><td>${b.name}<br><small>${b.status} • ${b.professionals} profissionais • ${b.leads} leads • ${b.kiosk} atendimentos Totem</small></td><td>${money(b.revenue)}</td><td>${b.appointments}</td><td>${money(b.ticket)}</td><td>⭐ ${b.rating}</td><td>${b.occupancy}%</td><td>${b.critical}</td><td><button class="btn btn-light" data-edit-branch="${i}">Editar</button></td></tr>`).join(''); };
    q('[data-branch-form]').addEventListener('submit', e => { e.preventDefault(); const name = new FormData(e.target).get('name'); state.branches.push({ name, status: 'Implantação', revenue: 0, appointments: 0, professionals: 0, ticket: 0, rating: 0, occupancy: 0, critical: 0, leads: 0, kiosk: 0 }); persist(); render(); toast('Unidade demo criada.'); });
    render();
  }

  function initAudit() {
    if (!q('[data-audit-page]')) return;
    const render = () => { const module = q('[data-audit-module]').value; const severity = q('[data-audit-severity]').value; q('[data-audit-list]').innerHTML = state.audit.filter(a => (!module || a.module === module) && (!severity || a.severity === severity)).map(a => `<div class="saas7-log saas7-${a.severity}"><strong>${a.module} • ${a.type}</strong><p>${a.text}</p><small>${a.user} • ${a.at}</small><button class="btn btn-light" onclick="window.AdminToast?.showInfo('Detalhes de auditoria mock: ${a.text.replace(/'/g, '')}')">Ver detalhes</button></div>`).join('') || '<p>Nenhum evento encontrado.</p>'; };
    all('[data-audit-module],[data-audit-severity]').forEach(el => el.addEventListener('change', render)); q('[data-export-audit]')?.addEventListener('click', () => toast('Exportação mock de auditoria criada.')); q('[data-clear-audit]')?.addEventListener('click', () => { state.audit = []; persist(); render(); toast('Logs demo limpos.'); }); render();
  }

  function initNotifications() {
    if (!q('[data-notifications-page]')) return;
    const render = () => { const unread = state.notifications.filter(n => !n.read).length; q('[data-notification-count]').textContent = unread; q('[data-notifications-list]').innerHTML = state.notifications.map((n, i) => `<div class="saas7-log saas7-${n.type}"><strong>${n.title}</strong><p>${n.module} • ${n.read ? 'Lida' : 'Não lida'}</p><div class="saas7-actions"><button class="btn btn-light" data-read="${i}">Marcar como lida</button><a class="btn btn-light" href="${n.route || '/Admin/Dashboard'}">Abrir módulo</a></div></div>`).join(''); renderTopbarNotifications(); };
    q('[data-notifications-list]').addEventListener('click', e => { if (e.target.dataset.read !== undefined) { state.notifications[e.target.dataset.read].read = true; persist(); render(); } }); q('[data-read-all]')?.addEventListener('click', () => { state.notifications.forEach(n => n.read = true); persist(); render(); toast('Todas notificações marcadas como lidas.'); }); render();
  }

  function initReports() {
    if (!q('[data-reports-7]')) return;
    const reports = ['Financeiro', 'Agenda', 'Clientes', 'Profissionais', 'Serviços', 'Estoque', 'Caixa', 'Campanhas', 'Totem', 'PublicWeb', 'Avaliações', 'Multiunidade', 'Omnichannel', 'Lead to Cash'];
    q('[data-reports-tabs]').innerHTML = reports.map((r, i) => `<button class="btn ${i ? 'btn-light' : 'btn-primary'}" data-report="${r}">${r}</button>`).join('');
    const render = (name = 'Financeiro') => { const total = state.branches.reduce((sum, b) => sum + b.revenue, 0); q('[data-report-content]').innerHTML = `<h2>${name}</h2><div class="saas7-metrics"><div class="saas7-metric"><strong>${money(total)}</strong><span>Receita</span></div><div class="saas7-metric"><strong>${state.branches.reduce((s, b) => s + b.appointments, 0)}</strong><span>Agendamentos</span></div><div class="saas7-metric"><strong>${state.services.filter(s => s.public).length}</strong><span>Serviços publicados</span></div><div class="saas7-metric"><strong>${state.kiosk.logs.length}</strong><span>Eventos Totem</span></div></div><div class="saas7-chart">${state.branches.map(b => `<div class="saas7-bar" style="height:${b.occupancy}%"></div>`).join('')}</div><table class="saas7-table"><thead><tr><th>Relatório</th><th>Unidade</th><th>Canal</th><th>Indicador</th><th>Status</th></tr></thead><tbody>${state.branches.map(b => `<tr><td>${name}</td><td>${b.name}</td><td>Omnichannel</td><td>${money(b.revenue)}</td><td>Disponível</td></tr>`).join('')}</tbody></table>`; };
    q('[data-reports-tabs]').addEventListener('click', e => { if (e.target.dataset.report) render(e.target.dataset.report); }); all('[data-report-action]').forEach(btn => btn.addEventListener('click', () => { notify(`${btn.dataset.reportAction} de relatório executado`, 'Relatórios', 'info', '/Admin/Reports'); persist(); toast(`${btn.dataset.reportAction} mock executado.`); })); render();
  }

  function initDashboard() {
    if (!q('[data-dashboard-7]')) return;
    const render = () => { const mult = q('[data-period]').value === 'month' ? 1.8 : q('[data-period]').value === '7' ? 1.2 : 1; const revenue = state.branches.reduce((s, b) => s + b.revenue, 0) * mult; q('[data-dashboard-7-kpis]').innerHTML = `<div class="saas7-metric"><strong>${money(revenue)}</strong><span>Receita por canal</span></div><div class="saas7-metric"><strong>${Math.round((state.publicSite.appointmentRequests / state.publicSite.leads) * 100)}%</strong><span>Conversão PublicWeb</span></div><div class="saas7-metric"><strong>${state.kiosk.logs.length * 12}</strong><span>Uso do Totem</span></div><div class="saas7-metric"><strong>${state.branches.reduce((s, b) => s + b.critical, 0)}</strong><span>Estoque crítico</span></div>`; };
    all('[data-period],[data-unit],[data-channel]').forEach(el => el.addEventListener('change', render)); q('[data-dashboard-save-layout]')?.addEventListener('click', () => { localStorage.setItem('barbersync.dashboard.widgets.v8', 'saved'); toast('Preferência de widgets salva.'); }); q('[data-dashboard-toggle-widgets]')?.addEventListener('click', () => { all('.saas7-collapsible').slice(-2).forEach(x => x.hidden = !x.hidden); toast('Mostrar/ocultar widgets aplicado.'); }); q('[data-dashboard-reorder]')?.addEventListener('click', () => { const grid = q('[data-dashboard-widgets]'); if (grid?.lastElementChild) grid.prepend(grid.lastElementChild); toast('Reordenação visual mock aplicada.'); }); all('[data-collapse]').forEach(btn => btn.addEventListener('click', () => { const body = btn.closest('.saas7-card').querySelector('[data-saas7-body]'); body.hidden = !body.hidden; btn.textContent = body.hidden ? 'Expandir' : 'Colapsar'; })); render();
  }

  function initCopilot() {
    if (!q('[data-copilot-5]')) return;
    const answers = { Gestão: 'Diagnóstico: operação saudável, com oportunidade de padronizar unidades. Dados analisados: receita, ocupação, avaliações e canais. Risco/oportunidade: franquias podem replicar melhores práticas. Ação recomendada: abrir relatório multiunidade. Prioridade: média.', Financeiro: 'Diagnóstico: ticket médio crescente. Dados analisados: comandas, caixa e descontos. Risco: descontos acima da regra demo. Ação recomendada: revisar caixa e comissão. Prioridade: alta.', Estoque: 'Diagnóstico: itens próximos do mínimo. Dados analisados: estoque crítico por unidade e combos. Oportunidade: reposição inteligente. Ação recomendada: abrir estoque. Prioridade: alta.', Agenda: 'Diagnóstico: pico entre 18h e 20h. Dados analisados: ocupação e atrasos. Ação: confirmar automaticamente clientes. Prioridade: média.', Marketing: 'Diagnóstico: clientes inativos geram retorno potencial. Dados: recorrência, cashback e cupons. Ação: criar campanha e cupom. Prioridade: alta.', Equipe: 'Diagnóstico: profissionais com ocupação desigual. Dados: agenda e avaliação. Ação: redistribuir agenda. Prioridade: média.', Totem: 'Diagnóstico: Totem online com pagamentos mock. Dados: logs, serviços liberados e timeout. Ação: simular atendimento. Prioridade: baixa.', PublicWeb: 'Diagnóstico: site ativo com conversão demo. Dados: leads, CTA e serviços publicados. Ação: atualizar preview. Prioridade: média.', Multiunidade: 'Diagnóstico: unidades comparáveis com estoque crítico por praça. Dados: ranking, receita e canais. Ação: abrir Channel Manager. Prioridade: alta.' };
    q('[data-copilot-modes]').innerHTML = Object.keys(answers).map((mode, i) => `<button class="btn ${i ? 'btn-light' : 'btn-primary'}" data-mode="${mode}">${mode}</button>`).join('');
    const render = mode => { q('[data-copilot-answer-5]').innerHTML = `<div class="saas7-log saas7-success"><strong>${mode}</strong><p>${answers[mode]}</p><div class="saas7-actions"><a class="btn btn-primary" href="/Admin/Campaigns">Criar campanha</a><button class="btn btn-light" data-coupon>Gerar cupom</button><a class="btn btn-light" href="/Admin/Stock">Abrir estoque</a><a class="btn btn-light" href="/Admin/Appointments">Abrir agenda</a><a class="btn btn-light" href="/Admin/Reports">Abrir relatório</a><button class="btn btn-secondary" data-alert>Criar alerta</button><a class="btn btn-light" href="/Admin/ChannelManager">Channel Manager</a><a class="btn btn-light" href="/Admin/Kiosk">Kiosk Manager</a></div></div>`; };
    document.addEventListener('click', e => { if (e.target.dataset.mode) render(e.target.dataset.mode); if (e.target.dataset.alert !== undefined) { notify('Alerta criado pelo Copilot', 'Copilot', 'info', '/Admin/Notifications'); persist(); toast('Alerta criado.'); } if (e.target.dataset.coupon !== undefined) { notify('Cupom COPILOT20 gerado', 'Cupons', 'success', '/Admin/Coupons'); persist(); toast('Cupom COPILOT20 gerado.'); } }); render('Gestão');
  }

  document.addEventListener('DOMContentLoaded', () => { state = fromDemoStore(); renderTopbarNotifications(); initPlatform(); initPublicSite(); initKiosk(); initUsers(); initBranches(); initAudit(); initNotifications(); initReports(); initDashboard(); initCopilot(); });
  window.BarberSyncSaas8 = { getState: () => state, save: persist, defaults: clone(defaults) };
  window.BarberSyncSaas7 = window.BarberSyncSaas8;
})();
