(() => {
  const endpointMap={Clients:'/AdminApi/clients',Professionals:'/AdminApi/professionals',Services:'/AdminApi/services',Appointments:'/AdminApi/appointments',ServiceOrders:'/AdminApi/service-orders',Stock:'/AdminApi/products',Campaigns:'/AdminApi/campaigns',Coupons:'/AdminApi/coupons',Loyalty:'/AdminApi/loyalty',Reviews:'/AdminApi/reviews',Copilot:'/AdminApi/copilot-suggestions'};
  const writeToast=(msg,type='success')=> window.AdminToast?.show ? window.AdminToast.show(msg,type) : alert(msg);
  const normalizeList=(data)=> Array.isArray(data) ? data : (Array.isArray(data?.items) ? data.items : (data ? [data] : []));
  const detail=(i)=> i.service || i.specialty || i.category || i.channel || i.comment || i.code || i.phone || i.description || i.client || i.priority || 'Operação BarberSync';
  const status=(i)=> i.status || (i.isDemo ? 'Demo' : 'Ativo');
  const name=(i)=> i.name || i.fullName || i.title || i.client || i.code || i.label || 'Registro demo';
  window.renderAdminCrudPage = async function(module){
    const endpoint=endpointMap[module]||'/AdminApi/dashboard';
    const demo=[{id:'demo-1',name:`${module} demo 1`,status:'Ativo',category:'Demonstração'},{id:'demo-2',name:`${module} demo 2`,status:'Pendente',category:'Operação'},{id:'demo-3',name:`${module} demo 3`,status:'Ativo',category:'SaaS'}];
    const load=async()=>{
      const {data,fallback}=await adminApiClient.get(endpoint,demo);
      if(fallback) document.getElementById(`${module}Fallback`)?.classList.remove('d-none');
      const list=normalizeList(data).length ? normalizeList(data) : demo;
      document.getElementById(`${module}Cards`).innerHTML = `<article class='kpi-card'><div class='kpi-icon'>📦</div><div><p class='kpi-label'>Total</p><strong class='kpi-value'>${list.length}</strong><span class='kpi-variation'>registros</span></div></article><article class='kpi-card'><div class='kpi-icon'>✅</div><div><p class='kpi-label'>Ativos</p><strong class='kpi-value'>${list.filter(x=>status(x).toLowerCase().includes('ativo')||status(x).toLowerCase().includes('dispon')).length||list.length}</strong><span class='kpi-variation'>online</span></div></article><article class='kpi-card'><div class='kpi-icon'>⚡</div><div><p class='kpi-label'>Fallback</p><strong class='kpi-value'>${fallback?'Demo':'API'}</strong><span class='kpi-variation'>seguro</span></div></article><article class='kpi-card'><div class='kpi-icon'>💎</div><div><p class='kpi-label'>SaaS</p><strong class='kpi-value'>Enterprise</strong><span class='kpi-variation'>pronto</span></div></article>`;
      document.getElementById(`${module}Rows`).innerHTML = list.slice(0,12).map((i,idx)=>`<tr><td><strong>${name(i)}</strong></td><td>${detail(i)}</td><td><span class='badge badge-success'>${status(i)}</span></td><td><button class='btn btn-secondary' data-admin-edit='${module}' data-id='${i.id||idx}'>Editar</button> <button class='btn btn-danger' data-admin-remove='${module}' data-id='${i.id||idx}'>Remover</button></td></tr>`).join('');
    };
    await load();
    document.querySelector(`[data-admin-new='${module}']`)?.addEventListener('click',()=>document.getElementById(`${module}Modal`).hidden=false);
    document.querySelector(`[data-admin-refresh='${module}']`)?.addEventListener('click',load);
    document.querySelectorAll(`[data-admin-close='${module}']`).forEach(b=>b.addEventListener('click',()=>document.getElementById(`${module}Modal`).hidden=true));
    document.querySelector(`[data-admin-form='${module}']`)?.addEventListener('submit',async(e)=>{e.preventDefault(); const body=Object.fromEntries(new FormData(e.target).entries()); await adminApiClient.post(endpoint,body,{success:true,message:'Salvo em modo demonstração.'}); document.getElementById(`${module}Modal`).hidden=true; writeToast('Registro salvo com sucesso.'); await load();});
    document.getElementById(`${module}Rows`)?.addEventListener('click',async(e)=>{ const edit=e.target.closest('[data-admin-edit]'); const remove=e.target.closest('[data-admin-remove]'); if(edit){document.getElementById(`${module}Modal`).hidden=false;} if(remove){ await adminApiClient.delete(`${endpoint}/${remove.dataset.id}`,{success:true}); writeToast('Registro removido em modo demonstração.'); await load(); }});
  };
})();
