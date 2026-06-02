(() => {
  const fallbackServices = [
    { id:'demo-corte', name:'Corte Masculino', category:'Barbearia', description:'Corte moderno com acabamento profissional.', price:45, durationMinutes:40, icon:'✂️' },
    { id:'demo-barba', name:'Barba Tradicional', category:'Barbearia', description:'Toalha quente e navalha.', price:35, durationMinutes:30, icon:'🪒' },
    { id:'demo-combo', name:'Corte + Barba', category:'Combo', description:'Experiência completa.', price:70, durationMinutes:60, icon:'💈' }
  ];
  const fallbackPros = [{id:'rafael',name:'Rafael Barber',specialty:'Fade',estimatedWaitMinutes:10},{id:'camila',name:'Camila Beauty',specialty:'Visagismo',estimatedWaitMinutes:15}];
  const unwrap=(payload,fallback)=> Array.isArray(payload?.data) ? payload.data : (Array.isArray(payload) ? payload : fallback);
  const loadJson=async(url,fallback)=>{ try{ const r=await fetch(url); if(!r.ok) throw new Error('http'); return await r.json(); }catch{return {data:fallback,message:'Modo demonstração'}} };
  document.addEventListener('DOMContentLoaded', async () => {
    const renderSideSummary = () => {
      const s = KioskFlow.state;
      const side = document.querySelector('[data-kiosk-summary-lateral]');
      if (side) side.innerHTML = `<p><strong>Serviço:</strong> ${s.serviceName || 'A escolher'}</p><p><strong>Cliente:</strong> ${s.client?.name || 'A identificar'}</p><p><strong>Profissional:</strong> ${s.professionalName || 'A escolher'}</p><p><strong>Pagamento:</strong> ${s.paymentMethod || 'A selecionar'}</p>`;
    };
    renderSideSummary();
    document.querySelectorAll('[data-kiosk-help]').forEach(button => button.addEventListener('click',()=> { KioskFlow.setState({ helpRequestedAt: new Date().toISOString() }); location.href='/Kiosk/Help'; }));
    document.querySelector('[data-kiosk-back]')?.addEventListener('click',()=> history.length > 1 ? history.back() : location.href='/Kiosk/Services');
    document.querySelector('[data-kiosk-accessibility]')?.addEventListener('click',()=> { document.body.classList.toggle('kiosk-accessible'); document.body.classList.toggle('kiosk-high-contrast'); });
    document.querySelector('[data-kiosk-cancel]')?.addEventListener('click',()=> sessionStorage.removeItem('kiosk-flow'));
    if (document.querySelector('.kiosk-step.success')) setTimeout(()=>{ sessionStorage.removeItem('kiosk-flow'); location.href='/Kiosk/Services'; }, 10000);

    const servicesEl=document.getElementById('kioskServices');
    if(servicesEl){ const payload=await loadJson(`/KioskApi/services?deviceCode=${encodeURIComponent(KioskFlow.deviceCode)}`,fallbackServices); const list=unwrap(payload,fallbackServices); document.getElementById('kioskDemoNotice')?.toggleAttribute('hidden', !(payload.message||'').toLowerCase().includes('demonstra')); servicesEl.innerHTML=list.map(s=>`<article class='k-card'><div class='k-icon'>${s.icon||'💈'}</div><h3>${s.name}</h3><p>${s.description||s.category}</p><strong>R$ ${Number(s.price??0).toFixed(2).replace('.',',')}</strong><span>${s.durationMinutes||30} min</span><a class='k-btn' data-service='${s.id||s.name}' href='/Kiosk/Client'>Selecionar</a></article>`).join(''); servicesEl.addEventListener('click',e=>{const a=e.target.closest('[data-service]'); if(a) { KioskFlow.setState({serviceId:a.dataset.service, serviceName:a.closest('.k-card').querySelector('h3').textContent}); renderSideSummary(); }}); }
    const prosEl=document.getElementById('kioskProfessionals');
    if(prosEl){ const payload=await loadJson(`/KioskApi/professionals?serviceId=${encodeURIComponent(KioskFlow.state.serviceId||'demo-corte')}&deviceCode=${encodeURIComponent(KioskFlow.deviceCode)}`,fallbackPros); const list=unwrap(payload,fallbackPros); prosEl.innerHTML=list.map(p=>`<article class='k-card'><div class='k-icon'>👤</div><h3>${p.name}</h3><p>${p.specialty||'Especialista'}</p><span>Espera ${p.estimatedWaitMinutes||10} min</span><a class='k-btn' data-pro='${p.id||p.name}' href='/Kiosk/Confirm'>Escolher</a></article>`).join(''); prosEl.addEventListener('click',e=>{const a=e.target.closest('[data-pro]'); if(a) { KioskFlow.setState({professionalId:a.dataset.pro, professionalName:a.closest('.k-card').querySelector('h3').textContent}); renderSideSummary(); }}); }
    document.getElementById('kioskClientForm')?.addEventListener('submit',async(e)=>{e.preventDefault(); const body=Object.fromEntries(new FormData(e.target).entries()); KioskFlow.setState({client:body}); renderSideSummary(); await KioskFlow.post('/KioskApi/client/quick-register',body,{success:true}); location.href='/Kiosk/Professional';});
    const summary=document.getElementById('kioskSummary'); if(summary){ const s=KioskFlow.state; summary.innerHTML=`<strong>${s.serviceName||'Corte Masculino'}</strong><span>${s.professionalName||'Primeiro disponível'}</span><span>${s.client?.name||'Cliente demo'}</span>`; }
    document.querySelectorAll('.payment-options button').forEach(b=>b.addEventListener('click',()=>{ KioskFlow.setState({paymentMethod:b.textContent.trim()}); renderSideSummary(); document.querySelectorAll('.payment-options button').forEach(x=>x.classList.remove('selected')); b.classList.add('selected'); }));
    document.getElementById('kioskPay')?.addEventListener('click',async()=>{KioskFlow.setState({paymentMethod:KioskFlow.state.paymentMethod||'PIX'}); await KioskFlow.post('/KioskApi/payment/mock',KioskFlow.state,{success:true}); location.href='/Kiosk/Success';});
    document.querySelectorAll('.kiosk-stars button').forEach(b=>b.addEventListener('click',()=>{ document.querySelectorAll('.kiosk-stars button').forEach(x=>x.classList.toggle('selected', Number(x.dataset.rating)<=Number(b.dataset.rating))); document.querySelector('input[name=rating]').value=b.dataset.rating; }));
    document.getElementById('kioskReviewForm')?.addEventListener('submit',async(e)=>{e.preventDefault(); await KioskFlow.post('/KioskApi/review',Object.fromEntries(new FormData(e.target).entries()),{success:true}); location.href='/Kiosk/Services';});
  });
})();
