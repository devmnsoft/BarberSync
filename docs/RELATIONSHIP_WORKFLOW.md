# Fluxo de Relacionamento

## Escopo
O módulo **Relacionamento** consolida CRM, perfil 360, segmentos comportamentais, pacotes, cupons, fidelidade e campanhas internas. Toda consulta administrativa atravessa `/AdminApi`; a API deriva tenant e unidade das claims autenticadas.

## Perfil e linha do tempo
`GET /api/clients/{id}/profile` combina cliente, perfil complementar, pacotes, resgates e fidelidade. O `PUT` mantém preferências e observações. `GET /api/clients/{id}/timeline` ordena agendamentos, comandas e movimentações reais, sem armazenamento no navegador.

## Pacotes, cupons e fidelidade no PDV
Pacotes usam `/api/packages` e `/api/client-packages`: venda e consumo validam vigência e saldo. A comanda aplica cupom em `/api/service-orders/{id}/coupon` e cashback em `/api/service-orders/{id}/cashback`; pagamento, resgate, baixa e acúmulo pertencem à transação PostgreSQL. Expiração, status, limites, saldo e duplicidade são validados antes de alterar a comanda.

## Segmentos e campanhas
Os segmentos `new`, `recurring`, `inactive-30`, `inactive-60`, `birthdays`, `no-show`, `vip`, `active-package` e `cashback` são calculados na unidade. Campanhas são internas: criar ou marcar manualmente não envia WhatsApp, SMS ou e-mail e não simula entrega.

## Mobile
`GET /api/mobile/summary` identifica o cliente pela claim `client_id` (ou sujeito autenticado) e retorna seus agendamentos, pacotes, assinaturas e fidelidade. Cupons pertencem ao mesmo tenant/unidade. Professional recebe agenda e comissões próprias, nunca saldos financeiros de clientes.

## Permissões, auditoria e erros
CRM exige `Client.Read`/`Client.Update`; campanhas exigem `Campaign.Read`/`Campaign.Create`; o PDV mantém `Coupon.Redeem`/`Loyalty.Redeem`. Ações críticas usam `audit_logs` com usuário, escopo, entidade, ação e motivo. Erros incluem `traceId`; o gateway propaga `X-Trace-Id`.

## Limitações
Não há envio externo de campanhas, IA em segmentos, token de demonstração, sucesso artificial ou persistência local alternativa nesta sprint.

## Integração com BI

Os indicadores gerenciais deste domínio são agregados pelo módulo descrito em `docs/ANALYTICS_WORKFLOW.md`, sem duplicar a fonte operacional canônica.
# Integração de comunicação

Campanhas de relacionamento selecionam segmentos e templates reais. Preferências e suppression list são verificadas antes da fila; o operador nunca informa IDs técnicos. O histórico de entrega permanece auditável na outbox e suas tentativas.
