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
    error.hidden = true; button.disabled = true; button.querySelector('span').textContent = 'Entrando...';
    try {
      const response = await fetch('/Account/Login', { method: 'POST', headers: { 'Content-Type': 'application/json', Accept: 'application/json' }, body: JSON.stringify({ email: document.getElementById('email').value.trim(), password: password.value }) });
      const payload = await response.json().catch(() => null);
      if (!response.ok) throw new Error(payload?.message || 'E-mail ou senha inválidos.');
      location.assign(payload?.redirectUrl || '/Admin/Dashboard');
    } catch (reason) {
      error.textContent = reason.message || 'Não foi possível entrar. Tente novamente.'; error.hidden = false;
    } finally { button.disabled = false; button.querySelector('span').textContent = 'Entrar'; }
  });
})();
