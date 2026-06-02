(() => {
  const escapeHtml = value => String(value ?? '').replace(/[&<>'"]/g, ch => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;' }[ch]));

  function renderEmptyRow(tbody, message = 'Nenhum registro encontrado.') {
    if (!tbody) return;
    const cols = tbody.closest('table')?.querySelectorAll('thead th').length || 4;
    tbody.innerHTML = `<tr><td colspan="${cols}"><div class="empty-state-mini">${escapeHtml(message)}</div></td></tr>`;
  }

  function renderLoadingRows(tbody, count = 3) {
    if (!tbody) return;
    const cols = tbody.closest('table')?.querySelectorAll('thead th').length || 4;
    tbody.innerHTML = Array.from({ length: count }, () => `<tr>${Array.from({ length: cols }, () => '<td><span class="skeleton-line"></span></td>').join('')}</tr>`).join('');
  }

  function filterRows(tbody, term) {
    const normalized = String(term || '').toLowerCase();
    tbody?.querySelectorAll('tr').forEach(row => row.hidden = normalized && !row.textContent.toLowerCase().includes(normalized));
  }

  window.AdminTables = { renderEmptyRow, renderLoadingRows, filterRows };
})();
