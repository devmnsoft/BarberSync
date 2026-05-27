(() => {
  const fallbackServices = [{ name: 'Corte Masculino', price: 45 }];
  const fallbackPros = [{ name: 'Rafael Barber' }];
  const load = async (url, fallback) => { try { const r = await fetch(url); if (!r.ok) return fallback; const j = await r.json(); return j.data || j.items || j || fallback; } catch { return fallback; } };
  const render=(id,arr,fn)=>{const e=document.getElementById(id); if(e) e.innerHTML=arr.map(fn).join('');};
  Promise.all([load('/PublicApi/services', fallbackServices), load('/PublicApi/professionals', fallbackPros)]).then(([services, pros]) => {
    render('services', services.slice(0,6), s=>`<article class='panel'><h4>${s.name||'Serviço'}</h4><p>${s.description||'Atendimento premium.'}</p><strong>R$ ${Number(s.price??45).toFixed(2).replace('.',',')}</strong></article>`);
    render('pros', pros.slice(0,5), p=>`<article class='panel'><h4>${p.name||'Profissional'}</h4><p>Especialista BarberSync</p></article>`);
  });
})();
