(() => {
 const root=document.querySelector('[data-club-page]'); if(!root)return;
 const api=window.AdminApiClient; const error=root.querySelector('[data-club-error]');
 const request=async(path,options)=>{if(api?.request)return api.request(path,options);const response=await fetch(path,{credentials:'include',headers:{'Content-Type':'application/json'},...options});const body=await response.json();if(!response.ok)throw Object.assign(new Error(body.detail||'Falha na operação.'),{traceId:body.traceId});return body;};
 const fail=e=>{error.hidden=false;error.querySelector('[data-error-message]').textContent=e.message;error.querySelector('[data-trace-id]').textContent=e.traceId||'não informado';};
 Promise.all([request('/api/club/dashboard'),request('/api/club/filter-options')]).then(([summary,options])=>{const d=summary.data;root.querySelector('[data-club-kpis]').innerHTML=`<article><span>MRR</span><b>${Number(d.mrr).toLocaleString('pt-BR',{style:'currency',currency:'BRL'})}</b></article><article><span>Assinaturas ativas</span><b>${d.activeMemberships}</b></article><article><span>Suspensas</span><b>${d.suspendedMemberships}</b></article><article><span>Pedidos pendentes</span><b>${d.pendingSales}</b></article>`;Object.entries(options.data).forEach(([key,items])=>root.querySelectorAll(`[data-option="${key}"]`).forEach(select=>items.forEach(item=>select.add(new Option(item.label,item.value)))));}).catch(fail);
 const modal=root.querySelector('[data-club-modal]');root.querySelector('[data-club-open]').addEventListener('click',()=>modal.showModal());
 modal.addEventListener('close',()=>{if(modal.returnValue==='default')error.hidden=true;});
})();
