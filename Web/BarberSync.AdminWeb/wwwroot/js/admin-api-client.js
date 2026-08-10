(() => {
  const DEFAULT_TIMEOUT_MS = 6500;

  const normalize = (url) => {
    if (!url) return '/AdminApi/dashboard';
    if (/^https?:\/\//i.test(url)) {
      try {
        const parsed = new URL(url);
        if (parsed.pathname.startsWith('/api/')) return `/AdminApi/${parsed.pathname.substring(5)}${parsed.search}`;
      } catch { return '/AdminApi/dashboard'; }
    }
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

  function isDemoResponse(payload) {
    const normalized = normalizeResponse(payload);
    return Boolean(normalized?.isDemo || normalized?.demo || normalized?.source === 'demo' || /demonstração|demo/i.test(normalized?.message || ''));
  }

  function handleApiError(error, message = 'Não foi possível conectar à API.') {
    console.error('[BarberSync AdminApiClient] API indisponível', error?.message || error);
    window.AdminToast?.showError?.(message);
    return { success: false, message, data: null, errors: [], connectionError: true };
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
      const text = await response.text();
      const payload = text ? JSON.parse(text) : { success: response.ok, data: null };
      const normalized = normalizeResponse(payload);
      if (!response.ok) {
        const statusMessages = {
          401: 'A sessão expirou. Faça login novamente.',
          403: 'Você não tem permissão para executar esta ação.',
          422: 'Não foi possível salvar. Revise os campos destacados.'
        };
        const traceSuffix = normalized.traceId ? ` Código: ${normalized.traceId}.` : '';
        window.AdminToast?.showError?.(`${statusMessages[response.status] || normalized.message || `Erro HTTP ${response.status}`}${traceSuffix}`);
        return { data: { ...normalized, success: false, httpStatus: response.status }, fallback: false, ok: false };
      }
      return { data: normalized, fallback: false, ok: true };
    } catch (error) {
      const failure = handleApiError(
        error,
        error?.name === 'AbortError'
          ? 'A API demorou para responder. Tente novamente.'
          : 'Não foi possível conectar à API.'
      );
      return { data: failure, fallback: false, ok: false };
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
    isDemoResponse,
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
  window.isDemoResponse = isDemoResponse;
})();
