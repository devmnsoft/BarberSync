(() => {
  const DEFAULT_TIMEOUT_MS = 6500;
  const normalize = (url) => {
    if (!url) return '/AdminApi/dashboard';
    if (url.startsWith('/AdminApi') || url.startsWith('/PublicApi') || url.startsWith('/KioskApi')) return url;
    if (url.startsWith('/api/')) return `/AdminApi/${url.substring(5)}`;
    return url;
  };

  async function request(method, url, body, fallback, timeoutMs = DEFAULT_TIMEOUT_MS) {
    const controller = new AbortController();
    const timeout = setTimeout(() => controller.abort(), timeoutMs);
    try {
      const response = await fetch(normalize(url), {
        method,
        signal: controller.signal,
        headers: body === undefined || body === null ? {} : { 'Content-Type': 'application/json' },
        body: body === undefined || body === null ? undefined : JSON.stringify(body)
      });
      if (!response.ok) throw new Error(`HTTP ${response.status}`);
      const text = await response.text();
      const payload = text ? JSON.parse(text) : { success: true, data: null };
      return { data: payload, fallback: false, ok: true };
    } catch (error) {
      console.warn('[BarberSync AdminApiClient] fallback ativado', { method, url: normalize(url), error: error?.message });
      return { data: fallback ?? { success: true, message: 'Modo demonstração ativo.', data: null, isDemo: true }, fallback: true, ok: true };
    } finally {
      clearTimeout(timeout);
    }
  }

  const client = {
    normalize,
    request,
    getJson: (url, fallback, timeoutMs) => request('GET', url, null, fallback, timeoutMs),
    postJson: (url, payload, fallback, timeoutMs) => request('POST', url, payload, fallback, timeoutMs),
    putJson: (url, payload, fallback, timeoutMs) => request('PUT', url, payload, fallback, timeoutMs),
    deleteJson: (url, fallback, timeoutMs) => request('DELETE', url, null, fallback, timeoutMs),
    get: (url, fallback, timeoutMs) => request('GET', url, null, fallback, timeoutMs),
    post: (url, payload, fallback, timeoutMs) => request('POST', url, payload, fallback, timeoutMs),
    put: (url, payload, fallback, timeoutMs) => request('PUT', url, payload, fallback, timeoutMs),
    delete: (url, fallback, timeoutMs) => request('DELETE', url, null, fallback, timeoutMs)
  };

  window.adminApiClient = client;
  window.AdminApiClient = client;
})();
