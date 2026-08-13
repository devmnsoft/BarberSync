(() => {
  const summary = document.querySelector('[data-health-summary]');
  if (!summary) return;
  const dependenciesBody = document.querySelector('[data-health-dependencies]');
  const errorState = document.querySelector('[data-health-error]');
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' })[char]);
  const statusLabel = healthy => `<span class="badge ${healthy ? 'badge-success' : 'badge-danger'}">${healthy ? 'Disponível' : 'Indisponível'}</span>`;

  async function load() {
    errorState.hidden = true;
    summary.classList.add('is-loading');
    const [healthResult, versionResult, dependenciesResult] = await Promise.all([
      window.AdminApiClient.get('/api/system/health/database'),
      window.AdminApiClient.get('/api/system/version'),
      window.AdminApiClient.get('/api/system/dependencies')
    ]);
    const health = healthResult.data;
    const version = versionResult.data;
    const dependencyData = dependenciesResult.data;
    summary.classList.remove('is-loading');

    if (!healthResult.ok || !versionResult.ok || !dependenciesResult.ok) {
      const failure = [health, version, dependencyData].find(item => item?.success === false || item?.traceId);
      errorState.hidden = false;
      errorState.querySelector('[data-health-error-message]').textContent = failure?.message || failure?.title || 'Confirme se a API e o banco estão em execução e tente novamente.';
      errorState.querySelector('[data-health-trace]').textContent = failure?.traceId || 'não informado';
    }

    summary.innerHTML = [
      ['API', dependenciesResult.ok, dependenciesResult.ok ? 'Respondendo' : 'Indisponível'],
      ['Banco', Boolean(health?.databaseConnected), health?.databaseStatus || 'Indisponível'],
      ['Schema barber', Boolean(health?.schemaReady), health?.schemaReady ? `${health.schemaVersions ?? 0} versões` : 'Não preparado'],
      ['Versão', versionResult.ok, version?.version || 'Não identificada']
    ].map(([label, healthy, value]) => `<article class="metric-card"><span>${escapeHtml(label)}</span><strong>${escapeHtml(value)}</strong>${statusLabel(healthy)}</article>`).join('');

    const rows = dependencyData?.dependencies || [];
    dependenciesBody.innerHTML = rows.length ? rows.map(item => `<tr><td>${escapeHtml(item.name)}</td><td>${statusLabel(item.healthy)} <small>${escapeHtml(item.status)}</small></td><td>${item.required ? 'Obrigatória' : 'Opcional'}</td></tr>`).join('') : '<tr><td colspan="3">Nenhuma dependência retornada.</td></tr>';
    document.querySelector('[data-health-updated]').textContent = dependencyData?.checkedAtUtc ? `Atualizado ${new Date(dependencyData.checkedAtUtc).toLocaleTimeString('pt-BR')}` : 'Falha na atualização';
  }

  document.querySelectorAll('[data-health-retry]').forEach(button => button.addEventListener('click', load));
  load();
})();
