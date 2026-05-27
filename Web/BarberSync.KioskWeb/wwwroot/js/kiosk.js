(() => {
  const fallback = [
    { name:"Corte Masculino", category:"Barbearia", description:"Corte moderno com acabamento profissional.", price:45, durationMinutes:40 },
    { name:"Barba Tradicional", category:"Barbearia", description:"Barba alinhada com toalha quente e navalha.", price:35, durationMinutes:30 },
    { name:"Corte + Barba", category:"Combo", description:"Experiência completa de corte e barba.", price:75, durationMinutes:70 },
    { name:"Sobrancelha", category:"Estética", description:"Design e alinhamento de sobrancelha.", price:20, durationMinutes:15 },
    { name:"Hidratação Capilar", category:"Estética", description:"Tratamento capilar profissional.", price:60, durationMinutes:45 },
    { name:"Manicure", category:"Beleza", description:"Cuidado completo para as unhas.", price:40, durationMinutes:50 }
  ];
  const container=document.getElementById('kioskServices'); if(!container) return;
  const base=(window.__kioskApiBaseUrl||"http://localhost:8080").replace(/\/$/,"");
  fetch(`${base}/api/kiosk/services?deviceCode=KIOSK-DEMO-001`).then(async r=>{
    if(!r.ok){console.warn('API indisponível ou retornou erro',r.status); return fallback;}
    const j=await r.json(); return j.data||fallback;
  }).catch(()=>fallback).then(arr=>{
    const list=Array.isArray(arr)&&arr.length?arr:fallback;
    container.innerHTML=list.map(s=>`<article class='k-card'><div>💈</div><h3>${s.name}</h3><p>${s.category}</p><p>${s.description}</p><strong>R$ ${Number(s.price).toFixed(2).replace('.',',')}</strong><span>${s.durationMinutes} min</span><a class='k-btn' href='/Kiosk/Client'>Selecionar</a></article>`).join('');
  });
})();
