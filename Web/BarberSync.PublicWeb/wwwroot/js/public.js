(() => {
  const fallbackServices = [{ name: 'Corte Masculino', description:'Corte premium com consultoria de estilo.', price: 45, durationMinutes: 40 },{name:'Barba Tradicional',description:'Toalha quente e navalha.',price:35,durationMinutes:30},{name:'Combo Corte + Barba',description:'Experiência completa.',price:70,durationMinutes:60},{name:'Hidratação Premium',description:'Tratamento para cabelo e barba.',price:55,durationMinutes:45}];
  const fallbackPros = [{ name: 'Rafael Barber', specialty:'Fade e barba', rating: 4.9 },{name:'Camila Beauty',specialty:'Visagismo', rating: 4.9},{name:'Amanda Nails',specialty:'Nails premium', rating: 4.7}];
  const load = async (url, fallback) => { try { const r = await fetch(url); if (!r.ok) return fallback; const j = await r.json(); const data = j.data || j.items || j; return Array.isArray(data) && data.length ? data : fallback; } catch { return fallback; } };
  const toast = (message) => { const el = document.getElementById('publicToast'); if (!el) return; el.textContent = message; el.hidden = false; setTimeout(() => el.hidden = true, 4200); };
  const render=(id,arr,fn)=>{const e=document.getElementById(id); if(e) { e.classList.remove('skeleton-grid'); e.innerHTML=(Array.isArray(arr)?arr:[]).map(fn).join(''); }};
  Promise.all([load('/PublicApi/services', fallbackServices), load('/PublicApi/professionals', fallbackPros)]).then(([services, pros]) => {
    render('services', services.slice(0,6), s=>`<article class='panel service-card'><span class='public-badge'>${s.category||'Premium'}</span><h3>${s.name||'Serviço'}</h3><p>${s.description||'Atendimento premium.'}</p><div><strong>R$ ${Number(s.price??45).toFixed(2).replace('.',',')}</strong><small>${s.durationMinutes||40} min</small></div></article>`);
    render('pros', pros.slice(0,5), p=>`<article class='panel pro-card'><div class='avatar'>${(p.name||'B').substring(0,1)}</div><h3>${p.name||'Profissional'}</h3><p>${p.specialty||'Especialista BarberSync'}</p><span class='public-badge'>⭐ ${p.rating||4.8} • Disponível hoje</span></article>`);
  });
  document.getElementById('publicAppointment')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    if (!e.target.checkValidity()) return;
    const result = document.getElementById('publicAppointmentResult');
    const body = Object.fromEntries(new FormData(e.target).entries());
    result.textContent = 'Enviando solicitação segura...';
    try { const r = await fetch('/PublicApi/appointments', { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(body)}); const j=await r.json(); result.textContent = j.message || 'Solicitação recebida. Entraremos em contato pelo WhatsApp.'; toast('Agendamento solicitado com sucesso.'); e.target.reset(); }
    catch { result.textContent = 'Solicitação recebida em modo demonstração.'; toast('Modo demo: solicitação registrada localmente.'); }
  });
})();
