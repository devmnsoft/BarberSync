(() => {
  const content = {
    Dashboard:['Dashboard executivo','KPIs, agenda, comandas, estoque e Copilot reunidos para leitura rápida da operação.',['Ler indicadores','Abrir novo agendamento','Consultar sugestões do Copilot'],['Mostre receita, ocupação, estoque crítico e plano de ação.'],'Avance para Clientes e demonstre a visão 360º.'],
    Clients:['Clientes','Cadastro, histórico, VIP, cashback, preferências e campanhas de retorno.',['Novo cliente','Ver Cliente 360','Editar dados','Excluir demo'],['Clique em Ver para apresentar total gasto, ticket médio e próxima melhor ação.'],'Crie um agendamento para o cliente.'],
    Appointments:['Agenda','Fluxo de status do agendamento: Agendado, Confirmado, Check-in, Em atendimento e Finalizado.',['Confirmar','Check-in','Iniciar','Finalizar','Cancelar'],['Atualize status ao vivo e mostre o toast de cada etapa.'],'Abra uma comanda após iniciar atendimento.'],
    ServiceOrders:['Comandas','Fluxo de atendimento, itens, descontos, cashback, pagamento e recibo visual.',['Ver recibo','Registrar pagamento','Fechar comanda'],['Mostre subtotal, desconto e mensagem de agradecimento.'],'Registre pagamento e confira caixa/estoque.'],
    Stock:['Estoque','Produtos com barras, níveis críticos e sugestão de compra.',['Atualizar estoque','Gerar reposição','Filtrar crítico'],['Mostre Pomada Modeladora crítica e reposição demo.'],'Crie campanha ou pedido de compra.'],
    Copilot:['Copilot','Assistente inteligente para sugestões de retenção, estoque, agenda e campanhas.',['Enviar pergunta','Criar campanha','Ver estoque','Abrir agenda'],['Use pergunta rápida para gerar resposta com prioridade.'],'Converta a sugestão em campanha de retorno.'],
    Kiosk:['Totem','Autoatendimento guiado para serviço, cliente, profissional, pagamento e avaliação.',['Testar totem','Configurar dispositivo','Ativar acessibilidade'],['Explique redução de fila e padronização de atendimento.'],'Abra o totem em localhost:8083.'],
    Default:['Ajuda contextual','Tela demonstrável BarberSync com fallback seguro e ações visíveis.',['Buscar','Criar','Editar','Detalhar','Salvar'],['Mostre que o navegador usa apenas proxies locais.'],'Siga o roteiro de demonstração.']
  };
  function moduleName(){ const part = location.pathname.split('/').filter(Boolean).pop() || 'Dashboard'; return content[part] ? part : 'Default'; }
  function fill(){ const [title,purpose,actions,tips,next]=content[moduleName()];
    document.querySelector('[data-help-title]').textContent=title;
    document.querySelector('[data-help-purpose]').textContent=purpose;
    document.querySelector('[data-help-actions]').innerHTML=actions.map(x=>`<li>${x}</li>`).join('');
    document.querySelector('[data-help-tips]').innerHTML=tips.map(x=>`<li>${x}</li>`).join('');
    document.querySelector('[data-help-next]').textContent=next;
  }
  function open(){ fill(); const d=document.getElementById('AdminHelpDrawer'); const s=document.querySelector('.admin-help-scrim'); if(!d)return; d.hidden=false; d.classList.add('is-open'); d.setAttribute('aria-hidden','false'); if(s)s.hidden=false; }
  function close(){ const d=document.getElementById('AdminHelpDrawer'); const s=document.querySelector('.admin-help-scrim'); if(!d)return; d.classList.remove('is-open'); d.hidden=true; d.setAttribute('aria-hidden','true'); if(s)s.hidden=true; }
  document.addEventListener('click',e=>{ if(e.target.closest('[data-help-open]')) open(); if(e.target.closest('[data-help-close]')) close(); });
  window.AdminHelp={open,close,fill};
})();
