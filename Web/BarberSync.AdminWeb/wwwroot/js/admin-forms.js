(() => {
  function validateRequired(form, fields = []) {
    const errors = [];
    fields.forEach(field => {
      const input = form?.elements?.[field] || document.querySelector(`[name='${field}']`);
      if (!input || String(input.value || '').trim()) return;
      errors.push(`${field} é obrigatório.`);
      input.classList.add('is-invalid');
    });
    return { valid: errors.length === 0, errors };
  }

  function validateEmail(value) {
    if (!value) return true;
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(String(value).trim());
  }

  function applyMasks() {
    document.querySelectorAll('input[type="tel"], input[name*="phone"], input[name*="whatsapp"]').forEach(input => {
      input.addEventListener('input', () => {
        const digits = input.value.replace(/\D/g, '').slice(0, 11);
        input.value = digits.length > 10 ? digits.replace(/(\d{2})(\d{5})(\d{4})/, '($1) $2-$3') : digits.replace(/(\d{2})(\d{4})(\d{0,4})/, '($1) $2-$3').replace(/-$/, '');
      });
    });
  }

  function serializeForm(formOrId) {
    const form = typeof formOrId === 'string' ? (document.getElementById(formOrId) || document.querySelector(`[data-admin-form='${formOrId}']`)) : formOrId;
    return form ? Object.fromEntries(new FormData(form).entries()) : {};
  }

  function validateForm(formOrId) {
    const form = typeof formOrId === 'string' ? (document.getElementById(formOrId) || document.querySelector(`[data-admin-form='${formOrId}']`)) : formOrId;
    if (!form) return false;
    const valid = form.checkValidity();
    form.querySelectorAll(':invalid').forEach(input => input.classList.add('is-invalid'));
    return valid;
  }

  function fillForm(form, data = {}) {
    Object.entries(data).forEach(([key, value]) => {
      const field = form?.elements?.[key];
      if (!field || typeof value === 'object') return;
      field.value = value ?? '';
    });
  }

  function resetForm(form) {
    form?.reset?.();
    form?.querySelectorAll?.('.is-invalid').forEach(input => input.classList.remove('is-invalid'));
  }

  const api = { validateRequired, validateEmail, applyMasks, serializeForm, validateForm, fillForm, resetForm };
  window.AdminForms = api;
  Object.assign(window, api);
  document.addEventListener('DOMContentLoaded', applyMasks);
})();
