window.renderAdminCrudPage = async function(module){
  const endpointMap={Clients:'/AdminApi/clients',Professionals:'/AdminApi/professionals',Services:'/AdminApi/services',Appointments:'/AdminApi/appointments',ServiceOrders:'/AdminApi/service-orders',Stock:'/AdminApi/stock-critical',Campaigns:'/AdminApi/campaigns',Coupons:'/AdminApi/coupons',Loyalty:'/AdminApi/loyalty',Reviews:'/AdminApi/reviews',Copilot:'/AdminApi/copilot-suggestions',Products:'/AdminApi/products'};
  const demo=[{name:`Demo ${module} 1`,status:'Ativo'},{name:`Demo ${module} 2`,status:'Ativo'},{name:`Demo ${module} 3`,status:'Pendente'}];
  const {data,fallback}=await adminApiClient.get(endpointMap[module]||'/AdminApi/dashboard',demo);
  if(fallback) document.getElementById(`${module}Fallback`)?.classList.remove('d-none');
  const list = Array.isArray(data)?data:(data.items||demo);
  document.getElementById(`${module}Cards`).innerHTML = `<div class='col-md-3'><div class='card p-3'><small>Total</small><h4>${list.length||1}</h4></div></div><div class='col-md-3'><div class='card p-3'><small>Ativos</small><h4>${Math.max(1,list.length-1)}</h4></div></div>`;
  document.getElementById(`${module}Rows`).innerHTML = list.slice(0,8).map(i=>`<tr><td>${i.name||i.fullName||i.title||'Registro demo'}</td><td>${i.status||'Ativo'}</td><td><button class='btn btn-sm btn-outline-light'>Ver</button> <button class='btn btn-sm btn-warning'>Editar</button></td></tr>`).join('');
}
