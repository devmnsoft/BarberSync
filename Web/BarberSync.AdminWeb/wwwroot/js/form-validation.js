(() => {'use strict';
 const firstInvalid=form=>form.querySelector(':invalid,[aria-invalid="true"]');
 window.BarberSyncForms={
  bind(form){if(!form)return;form.addEventListener('submit',event=>{form.querySelectorAll('[data-money]').forEach(input=>input.setCustomValidity(Number(String(input.value).replace(',','.'))>0?'':'Informe um valor maior que zero.'));const start=form.querySelector('[data-date-start]'),end=form.querySelector('[data-date-end]');if(start&&end)end.setCustomValidity(start.value&&end.value&&end.value<start.value?'A data final deve ser posterior à inicial.':'');if(!form.checkValidity()){event.preventDefault();form.reportValidity();firstInvalid(form)?.scrollIntoView({behavior:'smooth',block:'center'});return;}const submit=event.submitter;if(submit){submit.disabled=true;submit.dataset.originalText=submit.textContent;submit.textContent='Salvando…';}});},
  release(form){const submit=form?.querySelector('button[type="submit"][disabled]');if(submit){submit.disabled=false;submit.textContent=submit.dataset.originalText||'Salvar';}}
 };
 document.querySelectorAll('form[data-validate]').forEach(window.BarberSyncForms.bind);
})();
