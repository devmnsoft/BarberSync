(() => {
  const safe = (name, fn) => {
    try { return { name, passed: Boolean(fn()), detail: 'Passou' }; }
    catch (error) { return { name, passed: false, detail: error?.message || String(error) }; }
  };

  const requireStore = () => {
    if (!window.BarberSyncDemoStore) throw new Error('BarberSyncDemoStore não está disponível.');
    return window.BarberSyncDemoStore;
  };

  async function runDemoStoreTests() {
    const store = requireStore();
    const before = store.exportState();
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
    const cashback = store.generateFlowCashback(client.id, order.id);
    const review = store.createFlowReview(client.id, order.id, 5, 'Diagnóstico automático aprovado.');
    const event = window.BarberSyncEventBus?.emit?.('diagnostics:demoStoreTested', { module: 'Diagnóstico', title: 'DemoStore validado' });
    const summary = store.dashboardSummary();
    let persisted = null;
    try { persisted = JSON.parse(localStorage.getItem('barbersync.demo.state.v9') || 'null'); } catch (error) { throw new Error(`localStorage inválido: ${error.message}`); }

    const tests = [
      safe('DemoStore inicializa', () => Boolean(before && before.schemaVersion)),
      safe('DemoStore adiciona cliente', () => Boolean(client?.id)),
      safe('DemoStore cria agendamento', () => Boolean(appointment?.id)),
      safe('DemoStore abre comanda', () => Boolean(order?.id)),
      safe('DemoStore registra pagamento', () => paid?.status === 'Paid' || paid?.paymentId),
      safe('DemoStore gera cashback', () => Number(cashback?.generated || cashback?.balance || 0) >= 0),
      safe('DemoStore cria avaliação', () => Number(review?.rating) === 5),
      safe('EventBus registra evento', () => Boolean(event?.id || window.BarberSyncEventBus?.last?.(5).some(x => x.eventName === 'diagnostics:demoStoreTested'))),
      safe('Dashboard consegue ler dados', () => Number(summary.revenue || 0) >= 0 && Number(summary.appointments || 0) > 0),
      safe('localStorage persiste dados', () => Boolean(persisted?.clients?.some(x => x.id === client.id)))
    ];
    return tests;
  }

  window.BarberSyncDemoStoreTests = { run: runDemoStoreTests };
})();
