window.KioskFlow={
  deviceCode:'KIOSK-DEMO-001',
  get state(){ try{return JSON.parse(sessionStorage.getItem('kiosk-flow')||'{}')}catch{return{}} },
  setState(patch){ const next={...this.state,...patch}; sessionStorage.setItem('kiosk-flow',JSON.stringify(next)); if(next.serviceId) sessionStorage.setItem('selectedService', next.serviceId); if(next.client) sessionStorage.setItem('selectedClient', JSON.stringify(next.client)); if(next.professionalId) sessionStorage.setItem('selectedProfessional', next.professionalId); if(next.paymentMethod) sessionStorage.setItem('selectedPayment', next.paymentMethod); return next; },
  async post(url, body, fallback){ try{ const r=await fetch(url,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(body)}); if(!r.ok) throw new Error('http'); return await r.json(); } catch { return fallback; } },
  finish(paymentMethod='PIX'){
    const attendanceId=`KIOSK-${Date.now().toString().slice(-6)}`;
    const serviceOrderNumber=`TK-${Math.floor(1000+Math.random()*9000)}`;
    const payment={ id:`PAY-${Date.now().toString().slice(-6)}`, method:paymentMethod, status:'APPROVED', amount:this.state.amount||70, at:new Date().toISOString() };
    const summary={...this.state, attendanceId, serviceOrderNumber, payment, message:'Atendimento registrado. A comanda foi enviada para o painel.', channel:'Totem'};
    sessionStorage.setItem('barbersync.kiosk.summary', JSON.stringify(summary));
    this.setState(summary);
    return summary;
  }
};

document.addEventListener('click', async event => {
  if (!event.target.closest('#kioskPay')) return;
  const selected = document.querySelector('.payment-options button.active')?.textContent || 'PIX';
  const summary = window.KioskFlow.finish(selected);
  await window.KioskFlow.post('/KioskApi/payment/mock', summary, { success:true, data:summary });
  window.location.href = '/Kiosk/Success';
});
document.addEventListener('click', event => {
  const option = event.target.closest('.payment-options button');
  if (!option) return;
  document.querySelectorAll('.payment-options button').forEach(b => b.classList.remove('active'));
  option.classList.add('active');
  window.KioskFlow.setState({ paymentMethod: option.textContent });
});
