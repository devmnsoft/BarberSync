(() => {
  'use strict';
  const form = document.getElementById('loginForm');
  const password = document.getElementById('password');
  const toggle = document.querySelector('[data-toggle-password]');
  const error = document.querySelector('[data-login-error]');
  toggle?.addEventListener('click', () => {
    const visible = password.type === 'text';
    password.type = visible ? 'password' : 'text';
    toggle.textContent = visible ? 'Mostrar' : 'Ocultar';
    toggle.setAttribute('aria-label', visible ? 'Mostrar senha' : 'Ocultar senha');
  });
  form?.addEventListener('submit', async event => {
    event.preventDefault();
    if (!form.reportValidity()) return;
    const button = form.querySelector('[type="submit"]');
    const requestToken = form.querySelector('[name="__RequestVerificationToken"]')?.value;
    error.hidden = true; button.disabled = true; button.querySelector('span').textContent = 'Entrando...';
    try {
      const response = await fetch('/Account/Login', { method: 'POST', headers: { 'Content-Type': 'application/json', Accept: 'application/json', RequestVerificationToken: requestToken || '' }, body: JSON.stringify({ email: document.getElementById('email').value.trim(), password: password.value }) });
      const payload = await response.json().catch(() => null);
      if (!response.ok) {
        const unavailable = response.status >= 500;
        const message = unavailable
          ? 'O BarberSync está temporariamente indisponível. Verifique o diagnóstico local e tente novamente.'
          : payload?.message || 'E-mail ou senha inválidos.';
        throw new Error(payload?.traceId ? `${message} Código de suporte: ${payload.traceId}` : message);
      }
      location.assign(payload?.redirectUrl || '/Admin/Dashboard');
    } catch (reason) {
      error.textContent = reason instanceof TypeError
        ? 'Não foi possível acessar a API. Sua tela continua disponível; tente novamente em instantes.'
        : reason.message || 'Não foi possível entrar. Tente novamente.';
      error.hidden = false;
    } finally { button.disabled = false; button.querySelector('span').textContent = 'Entrar'; }
  });
})();
