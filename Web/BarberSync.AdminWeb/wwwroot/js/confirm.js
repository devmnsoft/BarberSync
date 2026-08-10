(() => {
  let activeDialog;

  function ask(options, callback) {
    const settings = typeof options === 'string' ? { title: options } : (options || {});
    activeDialog?.remove();

    const overlay = document.createElement('div');
    overlay.className = 'bs-confirm-overlay';
    overlay.innerHTML = `
      <section class="bs-confirm-dialog" role="alertdialog" aria-modal="true" aria-labelledby="bs-confirm-title" aria-describedby="bs-confirm-description">
        <div class="bs-confirm-icon" aria-hidden="true">!</div>
        <div>
          <h2 id="bs-confirm-title"></h2>
          <p id="bs-confirm-description"></p>
        </div>
        <div class="bs-confirm-actions">
          <button type="button" class="btn btn-light" data-confirm-cancel></button>
          <button type="button" class="btn btn-danger" data-confirm-accept></button>
        </div>
      </section>`;
    overlay.querySelector('#bs-confirm-title').textContent = settings.title || 'Deseja confirmar esta ação?';
    overlay.querySelector('#bs-confirm-description').textContent = settings.description || 'Confira as informações antes de continuar.';
    overlay.querySelector('[data-confirm-cancel]').textContent = settings.cancelText || 'Cancelar';
    overlay.querySelector('[data-confirm-accept]').textContent = settings.confirmText || 'Confirmar';
    document.body.appendChild(overlay);
    activeDialog = overlay;

    return new Promise(resolve => {
      const finish = accepted => {
        overlay.remove();
        if (activeDialog === overlay) activeDialog = null;
        if (accepted) callback?.();
        resolve(accepted);
      };
      overlay.querySelector('[data-confirm-cancel]').addEventListener('click', () => finish(false));
      overlay.querySelector('[data-confirm-accept]').addEventListener('click', () => finish(true));
      overlay.addEventListener('click', event => { if (event.target === overlay) finish(false); });
      overlay.addEventListener('keydown', event => {
        if (event.key === 'Escape') finish(false);
      });
      requestAnimationFrame(() => overlay.classList.add('is-visible'));
      overlay.querySelector('[data-confirm-cancel]').focus();
    });
  }

  window.BarberSyncConfirm = { ask };
})();
