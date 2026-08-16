(() => {
  'use strict';
  const form = document.getElementById('loginForm');
  const password = document.getElementById('password');
  const toggle = document.querySelector('[data-toggle-password]');
  const error = document.querySelector('[data-login-error]');
  const csrf = form?.querySelector('input[name="__RequestVerificationToken"]');
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
    error.hidden = true; button.disabled = true; button.querySelector('span').textContent = 'Entrando...';
    try {
      const response = await fetch('/Account/Login', {
        method: 'POST',
        credentials: 'same-origin',
        headers: { 'Content-Type': 'application/json', Accept: 'application/json', RequestVerificationToken: csrf?.value || '' },
        body: JSON.stringify({
          email: document.getElementById('email').value.trim(),
          password: password.value,
          returnUrl: document.getElementById('returnUrl')?.value || null
        })
      });
      const payload = await response.json().catch(() => null);
      if (!response.ok) {
        const unavailable = response.status >= 500;
        const message = payload?.message || (unavailable
          ? 'O BarberSync está temporariamente indisponível. Tente novamente em instantes.'
          : 'E-mail ou senha inválidos.');
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
