document.addEventListener("DOMContentLoaded", function () {
  const body = document.body;
  const sidebar = document.querySelector('.admin-sidebar');
  const isDesktop = () => window.matchMedia('(min-width: 901px)').matches;
  const savedPreference = document.cookie.match(/(?:^|; )bs_sidebar=(expanded|collapsed)/)?.[1];
  if (savedPreference === 'collapsed' && isDesktop()) body.classList.add('sidebar-collapsed');

  document.querySelectorAll("[data-sidebar-toggle]").forEach(button => {
    button.addEventListener("click", () => {
      if (isDesktop()) {
        body.classList.toggle('sidebar-collapsed');
        document.cookie = `bs_sidebar=${body.classList.contains('sidebar-collapsed') ? 'collapsed' : 'expanded'};path=/;max-age=31536000;SameSite=Lax`;
      } else sidebar?.classList.toggle("is-open");
    });
  });
  document.querySelector('[data-sidebar-close]')?.addEventListener('click', () => sidebar?.classList.remove('is-open'));

  const menuButton = document.querySelector('[data-user-menu]');
  const dropdown = document.querySelector('.user-dropdown');
  menuButton?.addEventListener('click', () => {
    const open = menuButton.getAttribute('aria-expanded') !== 'true';
    menuButton.setAttribute('aria-expanded', String(open));
    if (dropdown) dropdown.hidden = !open;
  });
  document.addEventListener('click', event => {
    if (!event.target.closest('.user-menu-wrap') && dropdown && !dropdown.hidden) {
      dropdown.hidden = true;
      menuButton?.setAttribute('aria-expanded', 'false');
    }
  });
  document.addEventListener('keydown', event => {
    if (event.key === 'Escape') {
      sidebar?.classList.remove('is-open');
      if (dropdown) dropdown.hidden = true;
      menuButton?.setAttribute('aria-expanded', 'false');
    }
  });

  const cashIndicator = document.querySelector('[data-cash-status]');
  if (cashIndicator) fetch('/AdminApi/cash-registers/current', { headers: { Accept: 'application/json' } })
    .then(response => response.ok ? response.json() : Promise.reject())
    .then(payload => {
      const cash = payload?.data?.current ?? payload?.data ?? payload?.current ?? payload;
      const open = ['open', 'aberto'].includes(String(cash?.status || '').toLowerCase());
      cashIndicator.classList.toggle('is-open', open);
      cashIndicator.querySelector('strong').textContent = open ? 'aberto' : 'fechado';
    })
    .catch(() => { cashIndicator.querySelector('strong').textContent = 'indisponível'; });

  const notificationCount = document.querySelector('[data-topbar-notification-count]');
  if (notificationCount && !document.querySelector('[data-notifications-page]')) {
    fetch('/AdminApi/notifications', { headers: { Accept: 'application/json' } })
      .then(response => response.ok ? response.json() : Promise.reject())
      .then(payload => {
        const items = payload?.data?.items || payload?.data || payload?.items || payload || [];
        const unread = Array.isArray(items) ? items.filter(item => !(item.isRead === true || item.read === true || String(item.status).toLowerCase() === 'read')).length : 0;
        notificationCount.textContent = String(unread);
        notificationCount.hidden = unread === 0;
      })
      .catch(() => {
        notificationCount.textContent = '!';
        notificationCount.title = 'Notificações indisponíveis';
      });
  }
});
