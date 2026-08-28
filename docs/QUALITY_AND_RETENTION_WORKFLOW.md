# Qualidade & Retenção

## Objetivo e escopo

O módulo fecha o ciclo pós-atendimento usando apenas avaliações, atendimentos, suporte e retornos persistidos. Não estima satisfação, não cria avaliações e não considera imagem, rosto, câmera ou biometria. Toda consulta administrativa é autenticada e limitada ao `tenant_id` e à `branch_id` do usuário.

## Jornada

1. Uma avaliação submetida no Portal, Mobile ou Totem é persistida em `client_reviews`. Rating de 1–5 é obrigatório; NPS de 0–10 é opcional.
2. NPS 0–6 é **Detrator**, 7–8 **Neutro** e 9–10 **Promotor**. NPS detrator ou rating até 3 é elegível a caso de recuperação, sem apagar o feedback original.
3. Um caso nasce `Open`, com cliente, categoria, severidade, motivo e responsável selecionados. Pode seguir por `InProgress` e `WaitingClient` até `Recovered`, `Lost` ou `Cancelled`.
4. Ações (`Call`, `Message`, `Coupon`, `GiftCredit`, `Reschedule`, `Apology`, `ManualNote`) mantêm trilha. `Recovered` exige ação tomada; `Lost` exige motivo. Cupom/crédito depende de confirmação e permissão no Clube & Vendas.
5. Follow-ups têm prazo e responsável quando manuais. A mesma origem não é duplicada na janela de sete dias. Pendências vencidas passam para `Overdue`; conclusão registra usuário e horário.
6. Regras de retorno são configuradas por serviço com mínimo, máximo e dia recomendado — os intervalos não são hardcoded.

## Dashboard, retenção e reputação

O dashboard calcula rating, NPS, promotores, neutros e detratores diretamente de `client_reviews`, além da fila e recuperação. Snapshots guardam o histórico por operação, profissional ou serviço. Segmentos guardam critérios em JSON e associação calculada ao cliente; alertas usam regra, limite, severidade e ciclo de reconhecimento/resolução. Fonte indisponível deve retornar erro ou `sourceStatus: unavailable`, nunca zero inventado.

## Integrações

- **Portal do Cliente:** reviews e reclamações são fontes; a sessão do portal continua limitada ao próprio cliente.
- **Cliente 360:** perfil consolida última avaliação, NPS, recuperação, follow-ups e segmentos; timeline inclui eventos de qualidade.
- **Comunicação:** eventos `QualityReviewSubmitted`, `QualityNpsDetractor`, `QualityRecoveryCaseOpened`, `QualityRecoveryCaseAssigned`, `QualityFollowUpDue`, `QualityFollowUpOverdue`, `QualityClientRecovered` e `RetentionCampaignSuggested` podem alimentar automações. Canal externo requer provider, opt-in e ausência na suppression list; `ProviderNotConfigured` é falha explícita.
- **Agenda:** integrações podem exibir sinal discreto de retorno ou recuperação, sem texto ofensivo e sem bloquear atendimento.
- **Clube & Vendas:** cupom, gift credit ou carteira são ações confirmadas; não existe crédito automático.
- **BI Executivo:** os KPIs publicados são NPS, rating, avaliações, classificação, recuperação, follow-ups, inatividade e retorno.
- **IA Operacional:** apenas eventos operacionais agregados podem contextualizar análise. É proibido inferir emoção/satisfação por câmera.
- **Mobile/Totem:** contratos autenticados permitem avaliação e follow-up próprios; o Totem mantém `Kiosk:DeviceCode` do servidor e não aceita device code por query string.

## API e segurança

Rotas Admin usam `/api/quality`, `[Authorize]`, permissões `Quality.*`, Problem Details com `traceId` e filtros por escolhas reais. UUIDs existem somente em rota, option, estado interno e payload posterior a uma seleção; nenhum formulário solicita ID técnico digitável. Exports CSV mantêm o escopo autenticado.

## Operação sem comunicação falsa

A criação de caso ou follow-up não afirma que mensagem externa foi enviada. O operador deve escolher uma automação/configuração válida em Comunicação. Falhas preservam o registro e o `traceId`; não há fallback demo ou sucesso sintético.

## Integração Marketing Studio

O contrato de integração, atribuição e segurança está documentado em [MARKETING_STUDIO_WORKFLOW.md](MARKETING_STUDIO_WORKFLOW.md). Esta integração usa apenas dados persistidos, preserva o escopo tenant/unidade e não simula provider, pagamento ou conversão.
