(() => {
  const state=document.querySelector('#assistantState'), grid=document.querySelector('#assistantInsights'), button=document.querySelector('#refreshInsights');
  const esc=value=>{ const node=document.createElement('span'); node.textContent=value??''; return node.innerHTML; };
  async function load(){
    state.hidden=false; state.innerHTML='<p>Atualizando prioridades…</p>'; grid.innerHTML=''; window.AdminApiClient.setLoading(button,true);
    try { const rows=await window.AdminApiClient.adminGet('/api/growth/assistant/operational');
      if(!rows.length){ state.innerHTML='<h3>Operação em dia</h3><p>Nenhuma exceção operacional foi encontrada para esta unidade.</p>'; return; }
      state.hidden=true; grid.innerHTML=rows.map(x=>`<article class="panel"><div class="panel-header"><h3>${esc(x.title)}</h3><span class="badge badge-info">${esc(x.priority)}</span></div><p>${esc(x.description)}</p><small>${esc(x.reason)} · ${esc(x.relatedModule)}</small><div class="page-actions"><a class="btn btn-primary" href="${esc(x.suggestedAction.url)}">${esc(x.suggestedAction.label)}</a></div></article>`).join('');
    } catch(error){ state.innerHTML=`<h3>Não foi possível carregar</h3><p>${esc(error.message)}</p>`; window.AdminToast?.error('Falha ao consultar insights.'); }
    finally { window.AdminApiClient.setLoading(button,false); }
  }
  button.addEventListener('click',load); load();
})();
