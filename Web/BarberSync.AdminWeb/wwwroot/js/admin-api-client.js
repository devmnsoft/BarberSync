window.adminApiClient = {
  normalize(url) {
    if (url.startsWith('/AdminApi')) return url;
    if (url.startsWith('/api/')) return '/AdminApi/' + url.substring(5);
    return url;
  },
  async request(method, url, body, fallback) {
    try {
      const res = await fetch(this.normalize(url), { method, headers: body ? { 'Content-Type': 'application/json' } : {}, body: body ? JSON.stringify(body) : undefined });
      if (!res.ok) throw new Error(`HTTP ${res.status}`);
      return { data: await res.json(), fallback: false };
    } catch {
      return { data: fallback, fallback: true };
    }
  },
  get(url, fallback) { return this.request('GET', url, null, fallback); },
  post(url, body, fallback) { return this.request('POST', url, body, fallback); },
  put(url, body, fallback) { return this.request('PUT', url, body, fallback); },
  delete(url, fallback) { return this.request('DELETE', url, null, fallback); }
};
