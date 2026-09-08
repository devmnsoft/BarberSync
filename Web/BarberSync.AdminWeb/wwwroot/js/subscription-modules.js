(() => {
  'use strict';
  const root = document.querySelector('[data-subscription-modules]');
  if (!root) return;
  const cards = root.querySelector('[data-module-cards]');
  const error = root.querySelector('[data-error]');
  const dialog = root.querySelector('[data-request-dialog]');
  const form = root.querySelector('[data-request-form]');
  let selected;
  const escape = value => String(value ?? '').replace(/[&<>'"]/g, character => ({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[character]));
  const money = value => value == null ? 'Preço não configurado' : new Intl.NumberFormat('pt-BR',{style:'currency',currency:'BRL'}).format(value);
  const status = contract => contract?.status ?? 'Não contratado';

  async function request(url, options) {
    const response = await fetch(url, {credentials:'same-origin', ...options});
    const body = await response.json().catch(() => ({}));
    if (!response.ok) throw new Error(body.message || body.detail || 'Não foi possível concluir a operação.');
    return body.data ?? body;
  }

  async function load() {
    error.hidden = true;
    const modules = await request('/Subscription/Data/Modules');
    cards.innerHTML = modules.map(module => {
      const current = status(module.contract);
      const canRequest = module.isSellable && !['Active','Trial','PendingActivation','Pending'].includes(current);
      return `<article class="module-card"><header><div><small>${escape(module.category)}</small><h2>${escape(module.name)}</h2></div><span class="status-pill">${escape(current)}</span></header><p>${escape(module.description || 'Benefícios e descrição em configuração.')}</p><div class="module-meta"><span>${module.price == null ? 'Preço não configurado' : money(module.price) + '/mês'}</span>${module.isCore ? '<span>Incluso no núcleo</span>' : ''}</div>${canRequest ? `<button type="button" class="platform-primary" data-request="${escape(module.id)}" data-name="${escape(module.name)}">Solicitar contratação</button>` : ''}</article>`;
    }).join('');
    root.querySelectorAll('[data-request]').forEach(button => button.addEventListener('click', () => {selected=button.dataset.request;root.querySelector('[data-module-name]').textContent=button.dataset.name;dialog.showModal();}));
  }

  function showError(exception) { error.hidden=false; error.querySelector('p').textContent=exception.message; }
  root.querySelector('[data-retry]').addEventListener('click',()=>load().catch(showError));
  root.querySelectorAll('[data-close]').forEach(button=>button.addEventListener('click',()=>dialog.close()));
  form.addEventListener('submit', async event => {
    event.preventDefault();
    const button=form.querySelector('[type="submit"]');button.disabled=true;
    try {
      const data=new FormData(form);
      await request(`/Subscription/Modules/${encodeURIComponent(selected)}/Request`,{method:'POST',body:data});
      dialog.close();form.reset();await load();
    } catch(exception) { const message=form.querySelector('[data-form-error]');message.hidden=false;message.textContent=exception.message; }
    finally {button.disabled=false;}
  });
  load().catch(showError);
})();
