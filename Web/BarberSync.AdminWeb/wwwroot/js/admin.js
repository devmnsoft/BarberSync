(async () => {
  const list = document.getElementById('entityList');
  if (!list) return;

  const path = location.pathname.toLowerCase();
  const routes = {
    '/clients': '/clients',
    '/professionals': '/professionals',
    '/services': '/services',
    '/appointments': '/appointments',
    '/serviceorders': '/service-orders',
    '/cash': '/cash/current',
    '/stock': '/stock/critical'
  };

  let endpoint = routes[path] || '/clients';
  if (path === '/copilot') {
    const tenantId = document.querySelector('meta[name="barbersync-tenant-id"]')?.content?.trim();
    if (!tenantId) {
      list.innerHTML = '<div class="alert alert-danger">A sessão não contém um tenantId válido. Entre novamente ou contate o administrador.</div>';
      return;
    }
    endpoint = `/copilot/suggestions?tenantId=${encodeURIComponent(tenantId)}`;
  }

  try {
    const data = await Api.get(endpoint);
    list.innerHTML = `<pre>${JSON.stringify(data, null, 2)}</pre>`;
  } catch (error) {
    const message = error?.message || 'Não foi possível carregar os dados operacionais.';
    list.textContent = message;
    list.className = 'alert alert-warning';
  }
})();
