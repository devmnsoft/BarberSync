(() => {
  const fallbackServices = [{ name: 'Corte Masculino', description:'Corte premium com consultoria de estilo.', price: 45 },{name:'Barba Tradicional',description:'Toalha quente e navalha.',price:35},{name:'Combo Corte + Barba',description:'Experiência completa.',price:70}];
  const fallbackPros = [{ name: 'Rafael Barber', specialty:'Fade' },{name:'Camila Beauty',specialty:'Visagismo'},{name:'Amanda Nails',specialty:'Nails premium'}];
  const load = async (url, fallback) => { try { const r = await fetch(url); if (!r.ok) return fallback; const j = await r.json(); return j.data || j.items || j || fallback; } catch { return fallback; } };
  const render=(id,arr,fn)=>{const e=document.getElementById(id); if(e) e.innerHTML=(Array.isArray(arr)?arr:fallbackServices).map(fn).join('');};
  Promise.all([load('/PublicApi/services', fallbackServices), load('/PublicApi/professionals', fallbackPros)]).then(([services, pros]) => {
    render('services', services.slice(0,6), s=>`<article class='panel'><h3>${s.name||'Serviço'}</h3><p>${s.description||'Atendimento premium.'}</p><strong>R$ ${Number(s.price??45).toFixed(2).replace('.',',')}</strong></article>`);
    render('pros', pros.slice(0,5), p=>`<article class='panel'><h3>${p.name||'Profissional'}</h3><p>${p.specialty||'Especialista BarberSync'}</p><span class='public-badge'>Disponível hoje</span></article>`);
  });
  document.getElementById('publicAppointment')?.addEventListener('submit', async (e) => {
    e.preventDefault();
    const result = document.getElementById('publicAppointmentResult');
    const body = Object.fromEntries(new FormData(e.target).entries());
    try { const r = await fetch('/PublicApi/appointments', { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(body)}); const j=await r.json(); result.textContent = j.message || 'Solicitação recebida.'; }
    catch { result.textContent = 'Solicitação recebida em modo demonstração.'; }
  });
})();
