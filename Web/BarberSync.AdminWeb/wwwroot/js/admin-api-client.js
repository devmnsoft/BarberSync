window.adminApiClient = {
  async get(url, fallback) {
    try {
      const res = await fetch(url);
      if (!res.ok) throw new Error('http');
      return { data: await res.json(), fallback: false };
    } catch {
      return { data: fallback, fallback: true };
    }
  }
};
