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

  async function request(method, url, body) {
    const response = await fetch(normalize(url), {
      method,
      headers: body == null ? { accept: 'application/json' } : { accept: 'application/json', 'content-type': 'application/json' },
      body: body == null ? undefined : JSON.stringify(body)
    });
    const text = await response.text();
    let payload = null;
    if (text) {
      try { payload = JSON.parse(text); }
      catch { payload = { message: text }; }
    }
    if (!response.ok) {
      const traceId = payload?.traceId || payload?.extensions?.traceId;
      const message = payload?.detail || payload?.message || payload?.title || `A operação falhou (HTTP ${response.status}).`;
      const error = new Error(traceId ? `${message} Código de suporte: ${traceId}` : message);
      error.status = response.status;
      error.traceId = traceId;
      error.payload = payload;
      throw error;
    }
    return payload;
  }

  window.Api = {
    base: '/AdminApi',
    get: (url) => request('GET', url),
    post: (url, body) => request('POST', url, body),
    put: (url, body) => request('PUT', url, body),
    delete: (url) => request('DELETE', url)
  };
})();
