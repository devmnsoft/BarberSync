(() => {
  const normalize = (url = '') => {
    if (!url) return '/AdminApi/dashboard';
    if (/^https?:\/\//i.test(url)) {
      try {
        const parsed = new URL(url);
        if (parsed.pathname.startsWith('/api/')) return `/AdminApi/${parsed.pathname.substring(5)}${parsed.search}`;
      } catch { return '/AdminApi/dashboard'; }
    }
    if (url.startsWith('/AdminApi')) return url;
    if (url.startsWith('/api/')) return `/AdminApi/${url.substring(5)}`;
    return url.startsWith('/') ? `/AdminApi${url}` : `/AdminApi/${url}`;
  };

  const fallback = (message = 'Modo demonstração ativo.') => ({ success: true, message, data: null, isDemo: true });

  async function request(method, url, body) {
    try {
      const response = await fetch(normalize(url), {
        method,
        headers: body == null ? {} : { 'content-type': 'application/json' },
        body: body == null ? undefined : JSON.stringify(body)
      });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      const text = await response.text();
      return text ? JSON.parse(text) : fallback('Operação realizada com sucesso.');
    } catch (error) {
      window.AdminToast?.showWarning?.('API indisponível. Usando modo demonstração.');
      console.warn('[BarberSync Api legacy] fallback ativado', error?.message || error);
      return fallback();
    }
  }

  window.Api = {
    base: '/AdminApi',
    get: (url) => request('GET', url),
    post: (url, body) => request('POST', url, body),
    put: (url, body) => request('PUT', url, body),
    delete: (url) => request('DELETE', url)
  };
})();
