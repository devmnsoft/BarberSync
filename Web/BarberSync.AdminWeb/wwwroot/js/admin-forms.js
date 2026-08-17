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
    form.querySelectorAll('.is-invalid').forEach(input => input.classList.remove('is-invalid'));
    const valid = form.checkValidity();
    form.querySelectorAll(':invalid').forEach(input => input.classList.add('is-invalid'));
    form.setAttribute('aria-invalid', valid ? 'false' : 'true');
    return valid;
  }

  function setSubmitting(form, submitting, label = 'Processando…') {
    if (!form) return;
    form.setAttribute('aria-busy', submitting ? 'true' : 'false');
    form.querySelectorAll('button[type="submit"]').forEach(button => {
      if (submitting) button.dataset.idleLabel ||= button.textContent.trim();
      button.disabled = submitting;
      button.classList.toggle('is-loading', submitting);
      button.textContent = submitting ? label : (button.dataset.idleLabel || button.textContent);
    });
  }

  function technicalError(message, traceId) {
    const safeMessage = message || 'Não foi possível concluir a operação. Tente novamente.';
    return traceId ? `${safeMessage} Código de suporte: ${traceId}` : safeMessage;
  }

  async function submit(form, operation, options = {}) {
    if (!validateForm(form)) {
      form.reportValidity();
      return { success: false, validationError: true };
    }
    setSubmitting(form, true, options.loadingLabel);
    try {
      const result = await operation();
      if (result?.success === false) {
        const message = technicalError(result.message, result.traceId);
        options.onError?.(message, result);
        window.AdminToast?.showError?.(message);
      }
      return result;
    } finally {
      setSubmitting(form, false);
    }
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

  const api = { validateRequired, validateEmail, applyMasks, serializeForm, validateForm, setSubmitting, technicalError, submit, fillForm, resetForm };
  window.AdminForms = api;
  Object.assign(window, api);
  document.addEventListener('DOMContentLoaded', applyMasks);
})();
