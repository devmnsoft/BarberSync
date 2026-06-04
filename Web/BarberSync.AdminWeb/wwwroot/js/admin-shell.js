document.addEventListener("DOMContentLoaded", function () {
  document.querySelectorAll("[data-sidebar-toggle]").forEach(button => {
    button.addEventListener("click", () => document.body.classList.toggle("sidebar-collapsed"));
  });
});

(() => {
  document.addEventListener('click', event => {
    const button = event.target.closest('[data-demo-action]');
    if (!button) return;
    const action = button.dataset.demoAction || button.textContent.trim();
    window.BarberSyncDemoStore?.updateSettings?.('lastDemoAction', { action, at: new Date().toISOString(), path: location.pathname });
    window.BarberSyncToast?.show?.(`Ação demo executada: ${action}`);
  });
})();
