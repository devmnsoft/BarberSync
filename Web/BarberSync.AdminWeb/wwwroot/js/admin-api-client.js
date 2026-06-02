(() => {
  const DEFAULT_TIMEOUT_MS = 6500;

  const normalize = (url) => {
    if (!url) return '/AdminApi/dashboard';
    if (url.startsWith('/AdminApi') || url.startsWith('/PublicApi') || url.startsWith('/KioskApi')) return url;
    if (url.startsWith('/api/')) return `/AdminApi/${url.substring(5)}`;
    return url;
  };

  function normalizeResponse(payload) {
    if (payload && typeof payload === 'object' && 'success' in payload) return payload;
    return { success: true, message: 'Dados carregados com sucesso.', data: payload };
  }

  function setLoading(target, enabled) {
    const el = typeof target === 'string' ? document.querySelector(target) : target;
    if (!el) return;
    el.classList.toggle('is-loading', Boolean(enabled));
    el.setAttribute('aria-busy', enabled ? 'true' : 'false');
    if ('disabled' in el) el.disabled = Boolean(enabled);
  }

  function handleApiError(error, fallbackMessage = 'Modo demonstração ativo.') {
    console.warn('[BarberSync AdminApiClient] fallback ativado', error?.message || error);
    window.AdminToast?.showWarning?.(fallbackMessage);
    return { success: true, message: fallbackMessage, data: null, isDemo: true };
  }

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
      return { data: normalizeResponse(payload), fallback: false, ok: true };
    } catch (error) {
      const safe = fallback ?? handleApiError(error, 'Modo demonstração ativo.');
      return { data: normalizeResponse(safe), fallback: true, ok: true };
    } finally {
      clearTimeout(timeout);
    }
  }

  const adminGet = (url, fallback, timeoutMs) => request('GET', url, null, fallback, timeoutMs).then(r => r.data);
  const adminPost = (url, data, fallback, timeoutMs) => request('POST', url, data, fallback, timeoutMs).then(r => r.data);
  const adminPut = (url, data, fallback, timeoutMs) => request('PUT', url, data, fallback, timeoutMs).then(r => r.data);
  const adminDelete = (url, fallback, timeoutMs) => request('DELETE', url, null, fallback, timeoutMs).then(r => r.data);

  const client = {
    normalize,
    request,
    normalizeResponse,
    setLoading,
    handleApiError,
    adminGet,
    adminPost,
    adminPut,
    adminDelete,
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
  window.adminGet = adminGet;
  window.adminPost = adminPost;
  window.adminPut = adminPut;
  window.adminDelete = adminDelete;
  window.setLoading = setLoading;
  window.normalizeResponse = normalizeResponse;
  window.handleApiError = handleApiError;
})();
