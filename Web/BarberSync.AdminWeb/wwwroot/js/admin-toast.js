(() => {
  const ensureHost = () => {
    let host = document.querySelector('[data-admin-toast-host]');
    if (!host) {
      host = document.createElement('div');
      host.className = 'bs-toast-host';
      host.setAttribute('data-admin-toast-host', 'true');
      document.body.appendChild(host);
    }
    return host;
  };

  const show = (message, type = 'info') => {
    const toast = document.createElement('div');
    toast.className = `bs-toast bs-toast-${type}`;
    toast.setAttribute('role', 'status');
    toast.innerHTML = `<strong>${type === 'success' ? 'Sucesso' : type === 'error' ? 'Erro' : type === 'warning' ? 'Atenção' : 'Info'}</strong><span>${message}</span>`;
    ensureHost().appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('is-visible'));
    setTimeout(() => {
      toast.classList.remove('is-visible');
      setTimeout(() => toast.remove(), 250);
    }, 4200);
  };

  const api = {
    show,
    showSuccess: (message) => show(message, 'success'),
    showError: (message) => show(message, 'error'),
    showInfo: (message) => show(message, 'info'),
    showWarning: (message) => show(message, 'warning')
  };

  window.AdminToast = api;
  window.adminToast = api;
})();
