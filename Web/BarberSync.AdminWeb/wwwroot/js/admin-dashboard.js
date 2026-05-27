(async function(){
  const demoSummary = { revenueToday: 4850, revenueMonth: 92340, appointmentsToday: 26, activeClients: 412, ongoing: 4, openOrders: 7, avgTicket: 126, stockCritical: 10, rating: 4.8, kioskOnline: 1, activeCampaigns: 5, apiOnline: 1,
    revenue7:[1800,2400,2200,2600,3100,2900,4850], status:[['Confirmados',12],['Check-in',5],['Concluídos',8],['Cancelados',1]],
    topServices:[['Corte Masculino',48],['Barba Tradicional',34],['Corte + Barba',29]], payments:[['PIX',42],['Cartão',38],['Dinheiro',20]],
    occupation:[['Lucas',92],['Rafael',88],['João',80]], nextAppointments:[{time:'14:00',client:'Carlos N.'},{time:'14:30',client:'Mauro R.'}], suggestions:['Ative campanha para horários das 16h às 18h.','Reponha Pomada Modeladora (estoque crítico).'] };

  const {data, fallback} = await adminApiClient.get('/api/dashboard/summary', demoSummary);
  if(fallback) document.getElementById('adminFallbackWarning')?.classList.remove('d-none');
  const kpis = [
    ['Receita hoje',`R$ ${data.revenueToday}`],['Receita mês',`R$ ${data.revenueMonth}`],['Agendamentos hoje',data.appointmentsToday],['Clientes ativos',data.activeClients],
    ['Atendimentos em andamento',data.ongoing],['Comandas abertas',data.openOrders],['Ticket médio',`R$ ${data.avgTicket}`],['Estoque crítico',data.stockCritical],['Avaliação média',data.rating],['Totem online',data.kioskOnline?'Sim':'Não'],['Campanhas ativas',data.activeCampaigns],['API online',fallback?'Demo':'Online']];
  dashboardKpis.innerHTML = kpis.map(k=>`<div class='col-md-3'><div class='card p-3'><small>${k[0]}</small><h4>${k[1]}</h4></div></div>`).join('');
  chartRevenue.innerHTML = data.revenue7.map(v=>`<div class='bar' style='height:${Math.max(20,v/60)}px' title='R$ ${v}'></div>`).join('');
  const renderList=(el,arr,suffix='')=>el.innerHTML=arr.map(i=>`<div class='d-flex justify-content-between py-1 border-bottom'><span>${i[0]}</span><strong>${i[1]}${suffix}</strong></div>`).join('');
  renderList(chartStatus,data.status); renderList(topServices,data.topServices); renderList(paymentMix,data.payments,'%'); renderList(proOccupation,data.occupation,'%');
  nextAppointments.innerHTML = data.nextAppointments.map(a=>`<div>${a.time} • ${a.client}</div>`).join('');
  copilotSuggestions.innerHTML = data.suggestions.map(s=>`<div class='badge text-bg-secondary d-block text-start mb-2'>${s}</div>`).join('');
})();
