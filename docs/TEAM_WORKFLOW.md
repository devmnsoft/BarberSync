# Equipe e RH operacional — Sprint 36

## Profissionais e serviços

`professional_profiles` complementa o cadastro-base de `professionals` sem duplicá-lo: documento, contato, biografia, foto, admissão/desligamento, regime, especialidades e configurações ficam ligados ao mesmo profissional. O vínculo `professional_services` determina quais serviços podem ser agendados e admite overrides de preço, duração e percentual. Inativação impede novos agendamentos, sem apagar histórico.

## Escala, pausas e afastamentos

A escala semanal possui dia ISO, início/fim, pausa e vigência. Uma substituição inativa a configuração anterior, preservando registros. `professional_schedule_blocks` cobre bloqueios pontuais; `professional_time_off` cobre folga, férias e indisponibilidade aprovada. A disponibilidade real verifica, no servidor, status do profissional, vínculo do serviço, escala/vigência/pausa, afastamentos e sobreposição de agendamentos. Mobile, Totem e Admin consomem o mesmo contrato.

## Comissões e repasses

O PDV gera `commissions` somente depois do pagamento confirmado. A chave única `(payment_id, service_order_item_id)` torna retries inofensivos. A base é o total líquido persistido do item; o percentual resolve override do vínculo, serviço e padrão do profissional. Regras adicionais modelam serviço, produto ou pacote, em percentual/fixo e com vigência.

O fechamento trava e seleciona apenas comissões `Available` do profissional/período. Cada comissão pertence a no máximo um settlement. Desconto exige motivo; settlement segue `Closed → Approved → Paid`, não permite pagar cancelado ou editar pago, e o pagamento cria exatamente um `professional_payout` com método e referência. Estornos permanecem correlacionados ao pagamento e devem cancelar/reverter a comissão antes de qualquer fechamento.

## Metas e performance

Metas suportam faturamento, atendimentos, ticket médio, produtos e retenção, com período, alvo, realizado e status. Dashboard/perfil consultam dados reais de agenda, comandas, pagamentos, comissões e repasses; snapshots permitem retenção de faturamento, volume, ticket, produtos, retorno, no-show e ocupação. Nenhum KPI é fabricado quando não há registros.

## Mobile Profissional e Operação do Dia

O resumo do papel `Professional` filtra pelo identificador autenticado e entrega somente sua agenda do dia, produção mensal, comissões abertas/pagas, metas, ocupação, folgas/bloqueios e alertas. Ele não expõe financeiro geral, comissão alheia ou dados de cliente além do atendimento. A Operação mostra link do profissional, situação de escala/bloqueio e comissão prevista; a comissão gerada continua sendo a do PDV.

## Segurança, auditoria e efeitos colaterais

Todas as APIs exigem `[Authorize]` e `RequirePermission`. Tenant/unidade vêm de `ICurrentUserContext`, nunca do payload. Perfil, escala, afastamento, regra, meta, fechamento, aprovação e pagamento gravam `audit_logs` com usuário, escopo, entidade, ação e motivo quando aplicável. Falhas retornam `traceId`; não existe sucesso alternativo, banco no navegador, token de demonstração ou dado inventado.

## Limitações conhecidas

O cálculo de retenção/no-show/ocupação histórico depende da materialização periódica de snapshots. Integrações bancárias externas não são simuladas: `mark-paid` registra a comprovação informada por usuário autorizado. Execução SQL e evidência autenticada permanecem no gate de production readiness.
