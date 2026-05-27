(() => {
  const fallbackServices = [
    { name:"Corte Masculino", category:"Barbearia", description:"Corte moderno com acabamento profissional.", price:45, durationMinutes:40, icon:"✂️" },
    { name:"Barba Tradicional", category:"Barbearia", description:"Barba alinhada com toalha quente e navalha.", price:35, durationMinutes:30, icon:"🪒" }
  ];
  const container = document.getElementById('kioskServices');
  const badge = document.getElementById('kioskDemoNotice');
  if (!container) return;
  const deviceCode = window.BarberSyncKiosk?.deviceCode || 'KIOSK-DEMO-001';
  const renderServices = (items, isDemo) => {
    if (badge) badge.style.display = isDemo ? 'inline-flex' : 'none';
    const safe = Array.isArray(items) && items.length ? items : fallbackServices;
    container.innerHTML = safe.map(s => `<article class='k-card'><div class='k-icon'>${s.icon || '💈'}</div><h3>${s.name}</h3><p>${s.category || 'Barbearia'}</p><p>${s.description || ''}</p><strong>R$ ${Number(s.price ?? 0).toFixed(2).replace('.',',')}</strong><span>${s.durationMinutes ?? 0} min</span><a class='k-btn' href='/Kiosk/Client'>Selecionar</a></article>`).join('');
  };
  document.addEventListener('DOMContentLoaded', async () => {
    try {
      const response = await fetch(`/KioskApi/services?deviceCode=${encodeURIComponent(deviceCode)}`);
      if (!response.ok) return renderServices(fallbackServices, true);
      const payload = await response.json();
      const services = payload.data || payload || [];
      if (!Array.isArray(services) || services.length === 0) return renderServices(fallbackServices, true);
      renderServices(services, (payload.message || '').toLowerCase().includes('demonstra'));
    } catch {
      renderServices(fallbackServices, true);
    }
  });
})();
