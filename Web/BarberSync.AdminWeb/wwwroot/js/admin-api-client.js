window.adminApiClient = {
  normalize(url) {
    if (url.startsWith('/AdminApi')) return url;
    if (url.startsWith('/api/')) return '/AdminApi/' + url.substring(5);
    return url;
  },
  async get(url, fallback) {
    try {
      const res = await fetch(this.normalize(url));
      if (!res.ok) throw new Error('http');
      return { data: await res.json(), fallback: false };
    } catch {
      return { data: fallback, fallback: true };
    }
  }
};
