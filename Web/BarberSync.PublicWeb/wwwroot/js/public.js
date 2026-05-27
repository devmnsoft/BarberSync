(() => {
  const fallbackServices = [
    { name: "Corte Masculino", price: 45 },{ name: "Barba Tradicional", price: 35 },{ name: "Corte + Barba", price: 75 },{ name: "Sobrancelha", price: 20 },{ name: "Hidratação Capilar", price: 60 },{ name: "Manicure", price: 40 }
  ];
  const fallbackPros = ["Rafael Barber","Lucas Navalha","Bruno Estilo","Camila Beauty","Amanda Nails"].map(name=>({name}));
  const render=(id,arr,fn)=>{const e=document.getElementById(id); if(!e) return; e.innerHTML=arr.map(fn).join("");};
  const base=(window.BarberSyncConfig?.apiBaseUrl||window.__publicApiBaseUrl||"http://localhost:8080").replace(/\/$/,"");
  const safeLoad=async (url,fallback)=>{try{const r=await fetch(`${base}${url}`); if(!r.ok){console.warn("API indisponível ou retornou erro",r.status); return fallback;} const j=await r.json(); return j.data||j.items||j||fallback;}catch(e){console.warn("Falha de API, usando fallback",e); return fallback;}};
  Promise.all([safeLoad('/api/services',fallbackServices),safeLoad('/api/professionals',fallbackPros)]).then(([services,pros])=>{
    render('services',services.slice(0,6),s=>`<article class='panel'><h4>${s.name||'Serviço'}</h4><p>${s.description||'Atendimento premium.'}</p><strong>R$ ${(s.price??45).toFixed? (s.price).toFixed(2).replace('.',','):'45,00'}</strong></article>`);
    render('pros',pros.slice(0,5),p=>`<article class='panel'><h4>${p.name||'Profissional'}</h4><p>Especialista BarberSync</p></article>`);
  });
})();
