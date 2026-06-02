(() => {
  const resolve = (id) => typeof id === 'string' ? document.getElementById(id) : id;

  function openModal(id) {
    const el = resolve(id);
    if (!el) return;
    el.hidden = false;
    el.classList.add('is-open');
    document.body.classList.add('modal-open');
    el.querySelector('input,select,textarea,button')?.focus?.();
  }

  function closeModal(id) {
    const el = resolve(id);
    if (!el) return;
    el.classList.remove('is-open');
    el.hidden = true;
    if (!document.querySelector('.admin-modal-backdrop.is-open:not([hidden])')) document.body.classList.remove('modal-open');
  }

  function confirmAction(message, callback) {
    const modal = document.getElementById('AdminConfirmModal');
    if (!modal) {
      if (window.confirm(message)) callback?.();
      return;
    }
    modal.querySelector('[data-confirm-message]').textContent = message;
    const ok = modal.querySelector('[data-confirm-ok]');
    const cancel = modal.querySelector('[data-confirm-cancel]');
    const done = () => closeModal(modal);
    ok.onclick = () => { done(); callback?.(); };
    cancel.onclick = done;
    openModal(modal);
  }

  const api = { openModal, closeModal, confirmAction, open: openModal, close: closeModal };
  window.AdminModal = api;
  window.adminModal = api;
})();
