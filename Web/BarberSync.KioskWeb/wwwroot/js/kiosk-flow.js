window.KioskFlow={
  deviceCode:'KIOSK-DEMO-001',
  get state(){ try{return JSON.parse(sessionStorage.getItem('kiosk-flow')||'{}')}catch{return{}} },
  setState(patch){ const next={...this.state,...patch}; sessionStorage.setItem('kiosk-flow',JSON.stringify(next)); if(next.serviceId) sessionStorage.setItem('selectedService', next.serviceId); if(next.client) sessionStorage.setItem('selectedClient', JSON.stringify(next.client)); if(next.professionalId) sessionStorage.setItem('selectedProfessional', next.professionalId); if(next.paymentMethod) sessionStorage.setItem('selectedPayment', next.paymentMethod); return next; },
  async post(url, body, fallback){ try{ const r=await fetch(url,{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(body)}); if(!r.ok) throw new Error('http'); return await r.json(); } catch { return fallback; } }
};
