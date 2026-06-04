(() => {
  const cfg = (() => { try { return JSON.parse(localStorage.getItem('barbersync.demo.state.v8') || '{}').settings || {}; } catch { return {}; } })();
  const params = new URLSearchParams(location.search);
  const branding = { ...(cfg.branding || {}), publicName: params.get('company') || cfg.branding?.publicName, slogan: params.get('slogan') || cfg.branding?.slogan, primary: params.get('primary') || cfg.branding?.primary, secondary: params.get('secondary') || cfg.branding?.secondary };
  const publicWeb = { ...(cfg.publicWeb || {}), heroTitle: params.get('title') || cfg.publicWeb?.heroTitle, heroSubtitle: params.get('subtitle') || cfg.publicWeb?.heroSubtitle, ctaPrimary: params.get('cta') || cfg.publicWeb?.ctaPrimary };
  const fallbackServices = publicWeb.services || [{name:'Corte Masculino',price:45,durationMinutes:40,category:'Barbearia',description:'Corte premium com finalização.'},{name:'Combo Corte + Barba',price:78,durationMinutes:70,category:'Combo',description:'Experiência completa com cashback.'},{name:'Barboterapia',price:65,durationMinutes:50,category:'Premium',description:'Relaxamento e cuidado da barba.'}];
  const fallbackPros = publicWeb.professionals || [{name:'Rafael Barber',specialty:'Corte e barba',rating:4.9},{name:'Bruna Estética',specialty:'Estética premium',rating:4.8},{name:'Marcos Fade',specialty:'Degradê e desenho',rating:4.7}];
  if (branding.primary) document.documentElement.style.setProperty('--public-primary', branding.primary);
  if (branding.secondary) document.documentElement.style.setProperty('--public-gold', branding.secondary);
  const setText = (selector, value) => { const el = document.querySelector(selector); if (el && value) el.textContent = value; };
  setText('[data-public-title]', publicWeb.heroTitle);
  setText('[data-public-subtitle]', publicWeb.heroSubtitle);
  setText('[data-public-cta-primary]', publicWeb.ctaPrimary);
  setText('[data-public-cta-secondary]', publicWeb.ctaSecondary);
  setText('[data-public-company]', branding.publicName);
  setText('[data-public-slogan]', branding.slogan);
  const toast = (message) => { const el = document.getElementById('publicToast'); if (!el) return; el.textContent = message; el.hidden = false; setTimeout(() => el.hidden = true, 4200); };
  const render=(id,arr,fn)=>{const e=document.getElementById(id); if(e) { e.classList.remove('skeleton-grid'); e.innerHTML=(Array.isArray(arr)?arr:[]).map(fn).join(''); }};
  const load = async (url, fallback) => { try { const r = await fetch(url); if (!r.ok) return fallback; const j = await r.json(); const data = j.data || j.items || j; return Array.isArray(data) && data.length ? data : fallback; } catch { return fallback; } };
  Promise.all([load('/PublicApi/services', fallbackServices), load('/PublicApi/professionals', fallbackPros)]).then(([services, pros]) => {
    render('services', services.slice(0,6), s=>`<article class='panel service-card'><span class='public-badge'>${s.category||'Publicado'}</span><h3>${s.name||'Serviço'}</h3><p>${s.description||'Atendimento premium publicado no Admin.'}</p><div><strong>R$ ${Number(s.price??45).toFixed(2).replace('.',',')}</strong><small>${s.durationMinutes||40} min</small></div></article>`);
    render('pros', pros.slice(0,5), p=>`<article class='panel pro-card'><div class='avatar'>${(p.name||'B').substring(0,1)}</div><h3>${p.name||'Profissional'}</h3><p>${p.specialty||'Especialista BarberSync'}</p><span class='public-badge'>⭐ ${p.rating||4.8} • Publicado</span></article>`);
  });
  document.getElementById('publicAppointment')?.addEventListener('submit', async (e) => {
    e.preventDefault(); if (!e.target.checkValidity()) return; const result = document.getElementById('publicAppointmentResult'); const body = Object.fromEntries(new FormData(e.target).entries()); result.textContent = 'Enviando solicitação segura...';
    const saved = JSON.parse(localStorage.getItem('barbersync.public.leads') || '[]'); const protocol = `PUB-2026-${String(saved.length + 1).padStart(4, '0')}`;
    const saveLocal = (status, response = {}) => { saved.unshift({ id: protocol, protocol, ...body, status, response, createdAt: new Date().toISOString(), channel:'PublicWeb' }); localStorage.setItem('barbersync.public.leads', JSON.stringify(saved)); };
    try { const r = await fetch('/PublicApi/appointments', { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(body)}); const j=await r.json().catch(()=>({})); saveLocal(r.ok?'Solicitado':'Demo local', j); }
    catch { saveLocal('Demo local'); }
    result.innerHTML = `<strong>Protocolo ${protocol}</strong><br>Solicitação registrada. <a class='btn-primary' href='http://localhost:8081/Admin/LeadToCash' target='_blank' rel='noopener'>Abrir Admin</a>`; toast('Solicitação enviada com sucesso.'); e.target.reset();
  });
})();
