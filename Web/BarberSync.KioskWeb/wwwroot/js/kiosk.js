(() => {
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]);
  const money = value => Number(value || 0).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  const showError = (host, message, errorCode) => { const kioskSetup = errorCode === 'KIOSK_DEVICE_NOT_CONFIGURED'; host.innerHTML = `<article class="k-card kiosk-error" role="alert"><h3>${kioskSetup ? 'Totem não configurado' : 'Não foi possível carregar'}</h3><p>${escapeHtml(kioskSetup ? 'Configure Kiosk:DeviceCode para este dispositivo.' : message)}</p><button class="k-btn" type="button" onclick="location.reload()">Tentar novamente</button></article>`; };
  const setBusy = (button, busy) => { button.disabled = busy; button.setAttribute('aria-busy', String(busy)); };

  document.addEventListener('DOMContentLoaded', async () => {
    const flow = window.KioskFlow;
    try {
      await flow.initialize();
    } catch (error) {
      const host = document.querySelector('.kiosk-step, .kiosk-services-screen');
      if (host) showError(host, error.message, error.errorCode);
      return;
    }
    const renderSummary = () => {
      const state = flow.state;
      const host = document.querySelector('[data-kiosk-summary-lateral]');
      if (host) host.innerHTML = `<p><strong>Serviço:</strong> ${escapeHtml(state.serviceName || 'A escolher')}</p><p><strong>Cliente:</strong> ${escapeHtml(state.client?.name || 'A identificar')}</p><p><strong>Profissional:</strong> ${escapeHtml(state.professionalName || 'A escolher')}</p><p><strong>Destino:</strong> ${escapeHtml(state.paymentMethod || 'Caixa')}</p>`;
    };
    renderSummary();

    document.querySelectorAll('[data-kiosk-help]').forEach(button => button.addEventListener('click', () => location.href = '/Kiosk/Help'));
    document.querySelector('[data-kiosk-back]')?.addEventListener('click', () => history.length > 1 ? history.back() : location.href = '/Kiosk/Services');
    document.querySelector('[data-kiosk-cancel]')?.addEventListener('click', () => flow.reset());
    document.querySelectorAll('[data-kiosk-accessibility]').forEach(button => button.addEventListener('click', () => {
      document.body.classList.toggle('kiosk-accessible');
      document.body.classList.toggle('kiosk-high-contrast');
    }));

    const servicesHost = document.getElementById('kioskServices');
    if (servicesHost) {
      try {
        const payload = await flow.request(`/KioskApi/services?deviceCode=${encodeURIComponent(flow.deviceCode)}`);
        const services = Array.isArray(payload.data) ? payload.data : [];
        servicesHost.innerHTML = services.length ? services.map(service => `<article class="k-card"><h3>${escapeHtml(service.name)}</h3><p>${escapeHtml(service.description || service.category || '')}</p><strong>${money(service.price)}</strong><span>${Number(service.durationMinutes || 30)} min</span><a class="k-btn" data-service="${escapeHtml(service.id)}" data-name="${escapeHtml(service.name)}" data-price="${Number(service.price || 0)}" href="/Kiosk/Client">Selecionar</a></article>`).join('') : '<article class="k-card"><h3>Nenhum serviço disponível</h3><p>Peça ajuda a um atendente.</p></article>';
        servicesHost.addEventListener('click', async event => { const target = event.target.closest('[data-service]'); if (!target) return; event.preventDefault(); await flow.setState({ serviceId: target.dataset.service, serviceName: target.dataset.name, amount: Number(target.dataset.price) }); location.href = target.href; });
      } catch (error) { showError(servicesHost, error.message, error.errorCode); }
    }

    const professionalsHost = document.getElementById('kioskProfessionals');
    if (professionalsHost) {
      try {
        const payload = await flow.request(`/KioskApi/professionals?serviceId=${encodeURIComponent(flow.state.serviceId || '')}&deviceCode=${encodeURIComponent(flow.deviceCode)}`);
        const professionals = Array.isArray(payload.data) ? payload.data : [];
        professionalsHost.innerHTML = professionals.length ? professionals.map(item => `<article class="k-card"><h3>${escapeHtml(item.name)}</h3><p>${escapeHtml(item.specialty || 'Profissional')}</p><span>${item.estimatedWaitMinutes ? `Espera estimada: ${Number(item.estimatedWaitMinutes)} min` : 'Disponibilidade no caixa'}</span><a class="k-btn" data-professional="${escapeHtml(item.id)}" data-name="${escapeHtml(item.name)}" href="/Kiosk/Confirm">Escolher</a></article>`).join('') : '<article class="k-card"><h3>Nenhum profissional disponível</h3><p>Peça ajuda a um atendente.</p></article>';
        professionalsHost.addEventListener('click', async event => { const target = event.target.closest('[data-professional]'); if (!target) return; event.preventDefault(); await flow.setState({ professionalId: target.dataset.professional, professionalName: target.dataset.name }); location.href = target.href; });
      } catch (error) { showError(professionalsHost, error.message, error.errorCode); }
    }

    document.getElementById('kioskClientForm')?.addEventListener('submit', async event => {
      event.preventDefault(); const button = event.submitter; setBusy(button, true);
      try { const client = Object.fromEntries(new FormData(event.currentTarget)); const result = await flow.post('/KioskApi/client/quick-register', client); await flow.setState({ client: result.data || client, clientId: result.data?.id }); location.href = '/Kiosk/Professional'; }
      catch (error) { alert(error.message); setBusy(button, false); }
    });

    const summary = document.getElementById('kioskSummary');
    if (summary) { const state = flow.state; summary.innerHTML = `<strong>${escapeHtml(state.serviceName)}</strong><span>${escapeHtml(state.professionalName)}</span><span>${escapeHtml(state.client?.name)}</span><strong>${money(state.amount)}</strong>`; }

    document.querySelectorAll('.payment-options button').forEach(button => button.addEventListener('click', async () => { await flow.setState({ paymentMethod: button.dataset.payment || button.textContent.trim() }); renderSummary(); document.querySelectorAll('.payment-options button').forEach(item => item.classList.toggle('selected', item === button)); }));
    document.getElementById('kioskPay')?.addEventListener('click', async event => {
      const button = event.currentTarget; setBusy(button, true);
      try {
        const state = flow.state;
        const result = await flow.post('/KioskApi/session', { ...state, channel: 'Kiosk', status: 'WaitingPayment', createdAt: new Date().toISOString() });
        await flow.saveSummary({ ...state, session: result.data, status: 'Enviado ao caixa', paymentMethod: state.paymentMethod || 'Caixa' });
        location.href = '/Kiosk/Success';
      } catch (error) { alert(error.message); setBusy(button, false); }
    });

    document.getElementById('kioskReviewForm')?.addEventListener('submit', async event => { event.preventDefault(); const button = event.submitter; setBusy(button, true); try { const review = Object.fromEntries(new FormData(event.currentTarget)); await flow.post('/KioskApi/review', { ...review, kioskSessionId: flow.state.session?.id }); location.href = '/Kiosk/Summary'; } catch (error) { alert(error.message); setBusy(button, false); } });
    const final = document.getElementById('kioskFinalSummary');
    if (final) { const state = { ...flow.state, ...flow.summary }; final.innerHTML = `<p><strong>Serviço:</strong> ${escapeHtml(state.serviceName)}</p><p><strong>Cliente:</strong> ${escapeHtml(state.client?.name)}</p><p><strong>Profissional:</strong> ${escapeHtml(state.professionalName)}</p><p><strong>Status:</strong> ${escapeHtml(state.status || 'Enviado ao caixa')}</p><p><strong>Protocolo:</strong> ${escapeHtml(state.session?.id || 'Aguardando')}</p>`; }
  });
})();
