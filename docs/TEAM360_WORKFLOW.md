# Equipe & RH 360

## Escopo e isolamento

Todas as consultas e mutações usam `tenant_id` e `branch_id` derivados dos claims autenticados. IDs aparecem somente em rotas, opções carregadas da API ou payloads resultantes de uma seleção. O módulo não aceita tenant ou filial informados pela interface.

## Ciclos operacionais

1. **Perfil:** `Draft → Active → Suspended/Inactive → Archived`. Ativação exige cadastro mínimo; suspensão e arquivamento exigem motivo e removem a visibilidade para novas reservas sem apagar histórico.
2. **Disponibilidade e escala:** regras de disponibilidade formam a janela potencial. Escalas são eventos datados e são recusadas quando sobrepõem ausência ou férias aprovadas. Alterações futuras criam/auditam uma nova versão; cancelamento exige motivo.
3. **Metas e produtividade:** uma meta identifica métrica, período, alvo e profissional ou função. Snapshots são derivados das fontes Agenda, Atendimento 360, checkout, Qualidade e Financeiro. Ausência de snapshot retorna `source_status=Unavailable`; valores não são fabricados.
4. **Comissões e repasses:** preview é somente leitura. Eventos confirmados formam um settlement; aprovação é segregada por permissão. A baixa exige `payment_id` real ou autorização explícita de baixa manual. Reversão preserva o lançamento original e a auditoria.
5. **Treinamento:** matrícula, conclusão e certificação ficam vinculadas ao profissional. Conclusão registra o instante e o ator autenticado. Certificação expirada é sinalizável e uma skill que exige certificação deve ser bloqueada pela Agenda/Atendimento.

## Permissões

`Team360.Read`, `Team360.Manage`, `Team360.Professionals.Manage`, `Team360.Schedules.Manage`, `Team360.Absences.Manage`, `Team360.Goals.Manage`, `Team360.Productivity.Read`, `Team360.Commissions.Manage`, `Team360.Payroll.Manage`, `Team360.Training.Manage`, `Team360.Permissions.Manage` e `Team360.Reports.Export`.

## Integrações e sourceStatus

Agenda consome apenas profissionais ativos, visíveis, disponíveis e sem bloqueio. Atendimento/checkout originam produção e comissão; Catálogo fornece skills, certificações e regra de comissão; Qualidade fornece NPS; BI e Command Center leem snapshots e alertas. Quando uma fonte ainda não publicou snapshot, a API informa `Unavailable`, sem substituir por zero presumido. Mobile expõe somente os dados do perfil ligado ao usuário autenticado.

## Integração Financeiro 360 — Sprint 61

Este módulo publica/consome origens persistidas pelo razão Financeiro 360. Nenhum status pago é inferido: liquidação exige payment confirmado ou baixa manual autorizada; fontes indisponíveis retornam `sourceStatus` sem estimativa. Consulte `docs/FINANCE360_WORKFLOW.md`.
