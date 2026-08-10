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

  const labels = { success: 'Sucesso', error: 'Erro', warning: 'Atenção', info: 'Informação' };
  const messages = Object.freeze({
    created: 'Cadastro realizado com sucesso.',
    updated: 'Alteração salva com sucesso.',
    deleted: 'Registro excluído com sucesso.',
    deactivated: 'Registro inativado com sucesso.',
    validation: 'Não foi possível salvar. Revise os campos destacados.',
    forbidden: 'Você não tem permissão para executar esta ação.',
    expired: 'A sessão expirou. Faça login novamente.',
    connection: 'Não foi possível conectar à API.',
    unexpected: 'Erro inesperado. Tente novamente ou contate o suporte.'
  });

  const show = (message, type = 'info', options = {}) => {
    const toast = document.createElement('div');
    toast.className = `bs-toast bs-toast-${type}`;
    toast.setAttribute('role', type === 'error' ? 'alert' : 'status');
    toast.setAttribute('aria-live', type === 'error' ? 'assertive' : 'polite');
    const title = document.createElement('strong');
    title.textContent = labels[type] || labels.info;
    const content = document.createElement('span');
    content.textContent = String(message || messages.unexpected);
    const close = document.createElement('button');
    close.type = 'button';
    close.className = 'bs-toast-close';
    close.setAttribute('aria-label', 'Fechar notificação');
    close.textContent = '×';
    toast.append(title, content, close);
    ensureHost().appendChild(toast);
    requestAnimationFrame(() => toast.classList.add('is-visible'));
    const dismiss = () => {
      toast.classList.remove('is-visible');
      setTimeout(() => toast.remove(), 250);
    };
    close.addEventListener('click', dismiss);
    if (!options.persistent) setTimeout(dismiss, options.duration || 5000);
    return toast;
  };

  const api = {
    show,
    showSuccess: (message) => show(message, 'success'),
    showError: (message) => show(message, 'error'),
    showInfo: (message) => show(message, 'info'),
    showWarning: (message) => show(message, 'warning'),
    messages,
    created: () => show(messages.created, 'success'),
    updated: () => show(messages.updated, 'success'),
    deleted: () => show(messages.deleted, 'success'),
    validation: () => show(messages.validation, 'warning'),
    forbidden: () => show(messages.forbidden, 'error'),
    sessionExpired: () => show(messages.expired, 'error', { persistent: true }),
    connectionError: () => show(messages.connection, 'error')
  };

  window.AdminToast = api;
  window.adminToast = api;
})();
