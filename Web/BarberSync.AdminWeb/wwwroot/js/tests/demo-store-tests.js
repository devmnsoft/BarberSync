(() => {
  const measure = (name, fn) => {
    const started = performance.now();
    try {
      const value = fn();
      return { name, passed: Boolean(value), detail: value ? 'Passou' : 'Condição retornou falso', durationMs: Math.round(performance.now() - started) };
    } catch (error) {
      return { name, passed: false, detail: error?.message || String(error), durationMs: Math.round(performance.now() - started) };
    }
  };

  const requireStore = () => {
    if (!window.BarberSyncDemoStore) throw new Error('BarberSyncDemoStore não está disponível.');
    return window.BarberSyncDemoStore;
  };

  async function runDemoStoreTests() {
    const store = requireStore();
    const before = store.exportState();
    const flowStart = store.resetFullServiceFlow?.() || store.createFullServiceFlow?.();
    const client = store.createFlowClient({ name: 'Cliente Diagnóstico', phone: '(11) 90000-1600', email: 'diag@barbersync.demo', vip: true });
    const appointment = store.createFlowAppointment({ clientId: client.id, serviceId: 'srv-003', professionalId: 'pro-001', date: new Date().toISOString().slice(0, 10), time: '16:00' });
    store.confirmFlowAppointment(appointment.id);
    store.checkInFlowAppointment(appointment.id);
    store.startFlowAttendance(appointment.id);
    store.finishFlowAttendance(appointment.id);
    const order = store.openFlowServiceOrder(appointment.id);
    store.addFlowService(order.id, 'srv-003');
    store.addFlowProduct(order.id, 'prd-001', 1);
    const paid = store.payFlowServiceOrder(order.id, { method: 'PIX', amount: 999, discount: 0, note: 'diagnóstico' });
    const receipt = store.generateFlowReceipt(order.id);
    store.confirmFlowStock?.();
    const cashback = store.generateFlowCashback(client.id, order.id);
    const review = store.createFlowReview(client.id, order.id, 5, 'Diagnóstico automático aprovado.');
    const completedFlow = store.completeFullServiceFlow?.();
    const event = window.BarberSyncEventBus?.emit?.('diagnostics:demoStoreTested', { module: 'Diagnóstico', title: 'DemoStore validado' });
    const summary = store.dashboardSummary();
    let persisted = null;
    try { persisted = JSON.parse(localStorage.getItem('barbersync.demo.state.v9') || 'null'); } catch (error) { throw new Error(`localStorage inválido: ${error.message}`); }

    return [
      measure('DemoStore inicializa', () => Boolean(before && before.schemaVersion)),
      measure('DemoStore adiciona cliente', () => Boolean(client?.id)),
      measure('DemoStore cria agendamento', () => Boolean(appointment?.id)),
      measure('DemoStore abre comanda', () => Boolean(order?.id)),
      measure('DemoStore registra pagamento', () => paid?.status === 'Paid' || paid?.paymentId),
      measure('DemoStore gera recibo', () => Boolean(receipt?.id || receipt?.number)),
      measure('DemoStore gera cashback', () => Number(cashback?.generated || cashback?.balance || 0) >= 0),
      measure('DemoStore cria avaliação', () => Number(review?.rating) === 5),
      measure('EventBus registra evento', () => Boolean(event?.id || window.BarberSyncEventBus?.last?.(5).some(x => x.eventName === 'diagnostics:demoStoreTested'))),
      measure('Dashboard consegue ler dados', () => Number(summary.revenue || 0) >= 0 && Number(summary.appointments || 0) > 0),
      measure('localStorage persiste dados', () => Boolean(persisted?.clients?.some(x => x.id === client.id))),
      measure('FullServiceFlow consegue iniciar', () => Boolean(flowStart?.id || store.getFullServiceFlow?.()?.id)),
      measure('FullServiceFlow consegue concluir', () => completedFlow?.status === 'Completed' || Boolean(completedFlow?.completedAt))
    ];
  }

  window.BarberSyncDemoStoreTests = { run: runDemoStoreTests };
})();
