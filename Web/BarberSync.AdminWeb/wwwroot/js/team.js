(() => {
  'use strict';
  const root = document.querySelector('[data-team-page]'); if (!root) return;
  const content = root.querySelector('[data-team-content]'); const error = root.querySelector('[data-team-error]');
  const escape = value => String(value ?? '').replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
  const label = value => String(value ?? '').replaceAll('_', ' ');
  async function api(path) { const response = await fetch(`/AdminApi/${path}`, { credentials: 'same-origin', headers: { Accept: 'application/json' } }); const body = await response.json().catch(() => ({})); if (!response.ok || body.success === false) throw Object.assign(new Error(body.message || body.title || 'Falha ao consultar a API.'), { traceId: body.traceId || response.headers.get('X-Trace-Id') }); return body.data; }
  const card = (title, body) => `<article class="team-card"><h2>${escape(title)}</h2>${body}</article>`;
  const table = rows => !rows?.length ? '<p class="team-empty">Nenhum registro encontrado.</p>' : `<div class="team-table"><table><thead><tr>${Object.keys(rows[0]).slice(0, 7).map(key => `<th>${escape(label(key))}</th>`).join('')}</tr></thead><tbody>${rows.map(row => `<tr>${Object.keys(row).slice(0, 7).map(key => `<td>${escape(row[key])}</td>`).join('')}</tr>`).join('')}</tbody></table></div>`;
  async function load() {
    error.hidden = true; content.setAttribute('aria-busy', 'true');
    try {
      const page = root.dataset.teamPage;
      if (page === 'dashboard' && location.pathname.toLowerCase().includes('/professionals')) { const rows = await api('professionals'); content.innerHTML = card('Profissionais', table(rows)); return; }
      if (page === 'dashboard') { const data = await api('team/dashboard'); content.innerHTML = Object.entries(data).map(([key, value]) => `<article><span>${escape(label(key))}</span><strong>${escape(value)}</strong></article>`).join(''); return; }
      if (page === 'professional') { const id = root.dataset.professionalId; const [profile, services, schedule, timeOff, performance, goals, payouts] = await Promise.all([api(`professionals/${id}/profile`), api(`professionals/${id}/services`), api(`professionals/${id}/schedule`), api(`professionals/${id}/time-off`), api(`professionals/${id}/performance`), api(`professional-goals?professionalId=${id}`), api(`professional-payouts?professionalId=${id}`)]); content.innerHTML = card('Dados cadastrais e especialidades', table([profile])) + card('Serviços', table(services)) + card('Escala semanal e pausas', table(schedule)) + card('Folgas, férias e bloqueios', table(timeOff)) + card('Produção', table([performance])) + card('Metas', table(goals)) + card('Repasses', table(payouts)); return; }
      if (page === 'commissions') { const [rules, settlements] = await Promise.all([api('commissions/rules'), api('commissions/settlements')]); content.innerHTML = card('Regras vigentes', table(rules)) + card('Settlements', table(settlements)); return; }
      if (page === 'goals') { content.innerHTML = card('Metas e progresso', table(await api('professional-goals'))); return; }
      if (page === 'payouts') { content.innerHTML = card('Extrato de repasses', table(await api('professional-payouts'))); return; }
      const professionals = await api('professionals'); content.innerHTML = card('Selecione um profissional', table(professionals));
    } catch (failure) { error.innerHTML = `<strong>Não foi possível carregar.</strong><p>${escape(failure.message)}</p><small>Trace ID: ${escape(failure.traceId || 'não informado')}</small>`; error.hidden = false; }
    finally { content.removeAttribute('aria-busy'); }
  }
  root.querySelector('[data-team-refresh]')?.addEventListener('click', load); load();
})();
