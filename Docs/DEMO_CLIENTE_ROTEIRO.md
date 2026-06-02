# Roteiro de Demonstração Comercial BarberSync

Este roteiro guia uma apresentação completa do BarberSync para salões, barbearias, estética e franquias. Use as URLs locais da demonstração e destaque que o navegador consome apenas proxies locais (`/AdminApi`, `/PublicApi` e `/KioskApi`).

## Sequência sugerida

1. **Abrir PublicWeb** em `http://localhost:8082/`.
   - Mostrar proposta de valor, benefícios, diferenciais, antes/depois, Totem inteligente, app para clientes, IA para gestão e planos.
2. **Mostrar benefícios**.
   - Explicar operação unificada: agenda, caixa, estoque, campanhas, fidelidade, avaliações e Copilot.
3. **Fazer agendamento público**.
   - Preencher o formulário em “Solicitar demonstração” e reforçar o uso do proxy `/PublicApi/appointments`.
4. **Abrir Admin** em `http://localhost:8081/Admin`.
   - Entrar pelo Dashboard Executivo e apresentar a navegação enterprise.
5. **Mostrar Dashboard**.
   - Apresentar KPIs, “Insights para hoje” e “Plano de ação sugerido”.
6. **Cadastrar cliente**.
   - Abrir Clientes, clicar em “Novo Cliente”, preencher campos obrigatórios, salvar e observar o toast.
7. **Ver Cliente 360**.
   - Clicar em “Ver Cliente 360” e demonstrar telefone, e-mail, VIP, cashback, total gasto, ticket médio, preferências, histórico e próxima melhor ação.
8. **Criar agendamento**.
   - Abrir Agenda, criar um agendamento demo e explicar status.
9. **Mudar status da agenda**.
   - Demonstrar: Agendado → Confirmado → Check-in → Em atendimento → Finalizado; usar Cancelar como exceção operacional.
10. **Abrir comanda**.
    - Acessar Comandas, criar uma comanda e explicar itens, desconto, cashback e total.
11. **Registrar pagamento**.
    - Usar “Registrar pagamento” e “Ver recibo” para mostrar o recibo visual com a mensagem “Obrigado pela preferência.”
12. **Ver estoque crítico**.
    - Abrir Estoque, mostrar barra visual, badge crítico/atenção/normal e sugestão de compra.
13. **Criar campanha**.
    - Abrir Campanhas/Cupons/Fidelidade, mostrar público-alvo, período, status, resultado, código copiável e cashback.
14. **Usar Copilot**.
    - Fazer uma pergunta rápida, apresentar prioridades Alta/Média/Baixa e converter resposta em ação: campanha, estoque ou agenda.
15. **Abrir Kiosk** em `http://localhost:8083/Kiosk/Services`.
    - Mostrar barra de progresso, acessibilidade, alto contraste, seleção de serviço e resumo lateral.
16. **Fazer autoatendimento**.
    - Selecionar serviço, cliente, profissional e pagamento; confirmar que o fluxo salva `selectedService`, `selectedClient`, `selectedProfessional` e `selectedPayment` em `sessionStorage`.
17. **Finalizar e avaliar**.
    - Exibir tela de sucesso, senha demo, animação simples e avaliação por estrelas grandes.

## Frases comerciais úteis

- “O BarberSync reduz fricção no balcão porque une PublicWeb, Totem, Admin e Mobile.”
- “Mesmo em modo demonstração, toda tela tem fallback visual e ação simulada com feedback.”
- “O Copilot transforma dados operacionais em campanhas, reposição e agenda mais eficiente.”

## Checklist de aceite para a apresentação

- Onboarding em `/Admin/Onboarding` concluído parcialmente ou totalmente.
- Ajuda contextual aberta pela topbar em cada módulo.
- Toasts visíveis após salvar, editar, excluir, confirmar agenda, registrar pagamento e perguntar ao Copilot.
- PublicWeb com CTAs para agendar serviço, ver painel administrativo, testar totem e solicitar demonstração.
- Kiosk finaliza o fluxo sem depender de URLs internas da API no navegador.
