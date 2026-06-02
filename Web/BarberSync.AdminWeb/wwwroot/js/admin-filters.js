(() => {
  function ensurePeriodFilters(){
    document.querySelectorAll('.module-toolbar').forEach(tb=>{
      if(tb.dataset.filtersReady) return; tb.dataset.filtersReady='true';
      tb.insertAdjacentHTML('beforeend','<input class="form-control" type="date" data-period-from aria-label="Período inicial"><input class="form-control" type="date" data-period-to aria-label="Período final"><button class="btn btn-light" data-clear-filters>Limpar filtros</button><span class="badge badge-info" data-record-counter>Registros</span>');
    });
  }
  document.addEventListener('click',e=>{ if(e.target.closest('[data-clear-filters]')){ const tb=e.target.closest('.module-toolbar'); tb?.querySelectorAll('input,select').forEach(i=>i.value=''); window.AdminToast?.showInfo?.('Filtros limpos.'); tb?.querySelector('input')?.dispatchEvent(new Event('input',{bubbles:true})); }});
  setInterval(ensurePeriodFilters,1000); document.addEventListener('DOMContentLoaded',ensurePeriodFilters);
})();
