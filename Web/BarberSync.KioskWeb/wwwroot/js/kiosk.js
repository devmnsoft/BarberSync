(() => {
  const fallbackServices = [
    { name:"Corte Masculino", category:"Barbearia", description:"Corte moderno com acabamento profissional.", price:45, durationMinutes:40, icon:"✂️" },
    { name:"Barba Tradicional", category:"Barbearia", description:"Barba alinhada com toalha quente e navalha.", price:35, durationMinutes:30, icon:"🪒" },
    { name:"Corte + Barba", category:"Combo", description:"Experiência completa de corte e barba.", price:75, durationMinutes:70, icon:"💈" },
    { name:"Sobrancelha", category:"Estética", description:"Design e alinhamento de sobrancelha.", price:20, durationMinutes:15, icon:"👁️" },
    { name:"Hidratação Capilar", category:"Estética", description:"Tratamento capilar profissional.", price:60, durationMinutes:45, icon:"💧" },
    { name:"Manicure", category:"Beleza", description:"Cuidado completo para as unhas.", price:40, durationMinutes:50, icon:"💅" }
  ];

  const container = document.getElementById("kioskServices");
  const badge = document.getElementById("kioskDemoNotice");
  if (!container) return;

  const config = window.BarberSyncConfig || {};
  const deviceCode = (config.deviceCode || "KIOSK-DEMO-001").trim();
  const base = (config.apiBaseUrl || "http://localhost:8080").replace(/\/$/, "");

  const renderServices = (items, isDemo) => {
    if (badge) badge.style.display = isDemo ? "inline-flex" : "none";
    const safeItems = Array.isArray(items) && items.length ? items : fallbackServices;
    container.innerHTML = safeItems.map(s => `
      <article class='k-card'>
        <div class='k-icon'>${s.icon || "💈"}</div>
        <h3>${s.name}</h3>
        <p>${s.category}</p>
        <p>${s.description}</p>
        <strong>R$ ${Number(s.price ?? 0).toFixed(2).replace('.',',')}</strong>
        <span>${s.durationMinutes ?? 0} min</span>
        <a class='k-btn' href='/Kiosk/Client'>Selecionar</a>
      </article>`).join("");
  };

  (async () => {
    try {
      const response = await fetch(`${base}/api/kiosk/services?deviceCode=${encodeURIComponent(deviceCode)}`);
      if (!response.ok && [404, 409, 500].includes(response.status)) {
        console.warn("API indisponível ou retornou erro", response.status);
        renderServices(fallbackServices, true);
        return;
      }
      if (!response.ok) {
        renderServices(fallbackServices, true);
        return;
      }

      const payload = await response.json();
      const services = Array.isArray(payload) ? payload : (payload?.data || []);
      if (!Array.isArray(services) || services.length === 0) {
        renderServices(fallbackServices, true);
        return;
      }
      renderServices(services, Boolean(services[0]?.isDemo));
    } catch (error) {
      console.warn("Falha ao carregar serviços do totem", error);
      renderServices(fallbackServices, true);
    }
  })();
})();
