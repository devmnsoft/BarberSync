# BarberSync — Roteiro de Demonstração Comercial 1.0

## Objetivo
Apresentar o BarberSync como SaaS real para barbearias, salões, estética e franquias, cobrindo jornada pública, operação administrativa, totem, fidelidade e Copilot.

## Sequência recomendada
1. **PublicWeb**: abrir `http://localhost:8082/`, mostrar proposta de valor, diferenciais, planos e CTA.
2. **Agendamento público**: preencher o formulário em `/PublicApi/appointments` e destacar o toast de confirmação.
3. **Admin Dashboard**: abrir `http://localhost:8081/Admin/Dashboard`, explicar KPIs, insights para hoje e plano de ação.
4. **Cliente 360**: abrir Clientes, buscar um cliente, clicar em “Ver Cliente 360” e mostrar histórico, cashback e próxima melhor ação.
5. **Agenda**: abrir Agenda e evoluir status: Agendado, Confirmado, Check-in, Em atendimento e Finalizado.
6. **Comanda**: abrir Comandas, registrar pagamento e exibir recibo visual com itens, descontos, cashback e total.
7. **Estoque**: abrir Estoque, mostrar barras, status crítico/atenção/normal e gerar reposição demo.
8. **Campanha**: abrir Campanhas/Cupons, criar campanha de retorno e copiar cupom.
9. **Copilot**: abrir Copilot, fazer pergunta rápida e converter sugestão em ação.
10. **Totem**: abrir `http://localhost:8083/Kiosk/Services`, selecionar serviço, cliente, profissional, pagamento mock e sucesso.
11. **Avaliação**: concluir no totem com estrelas grandes e explicar NPS/Reviews no Admin.

## Mensagem comercial
O BarberSync reduz fricção operacional ao unificar agenda, atendimento, comandas, estoque, campanhas, cashback, reputação, totem e app mobile com proxies seguros para a API.

## Checklist antes da apresentação
- Swagger abre em `http://localhost:8080/swagger`.
- Admin abre em `http://localhost:8081/Admin`.
- PublicWeb abre em `http://localhost:8082/`.
- Totem abre em `http://localhost:8083/Kiosk/Services`.
- Navegador usa apenas `/AdminApi`, `/PublicApi` e `/KioskApi`.

## Validação da versão demo madura — 2026-06-02
- Durante a apresentação, explique que Admin, PublicWeb e Kiosk possuem fallback demo para manter a experiência comercial fluida mesmo sem API externa.
- Demonstre uma mutação visual em cada etapa crítica: criar cliente, alterar status da agenda, pagar comanda, gerar reposição, criar campanha e registrar avaliação.
- Ao encerrar, reforçar que a segurança de proxy evita chamadas diretas do browser para a URL interna do container.
