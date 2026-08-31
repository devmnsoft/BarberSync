# BI Executivo e Analytics

## Dashboards e fontes

O Admin publica `/Analytics/Executive`, `/Operations`, `/Finance`, `/Team`, `/Relationship` e `/Inventory`. Os cartões consultam a API autenticada e derivam indicadores diretamente de `payments`, `service_orders`, `appointments`, `clients`, contas financeiras, comissões, fidelidade, produtos, compras, recebimentos, inventários e reposição. Snapshots são opcionais para histórico: nunca substituem a fonte canônica nem autorizam valores fabricados. Indicadores indisponíveis devem ser `null` ou zero acompanhados de `sourceStatus`.

O executivo compara a janela selecionada com a janela imediatamente anterior de igual duração. Rankings agregam comandas e pagamentos por profissional. Operação cobre agenda, check-in, conclusão, cancelamento, no-show, comandas e pagamentos; Financeiro cobre entradas, saídas, contas e conciliação; Equipe cobre comissões e metas; Relacionamento cobre clientes, cupons, pacotes e fidelidade; Estoque cobre mínimos, valor, compras, recebimentos, contagens e reposição.

## Filtros e validação

Unidade, profissional, cliente, serviço, produto, fornecedor e categoria são carregados como opções do tenant/filial autenticados. IDs existem somente em `option.value` e payloads. Períodos usam preset ou date picker, exigem início menor ou igual ao fim e limitam a janela a 366 dias. O Admin usa `form-validation.js`, validação inline, bloqueio de envio duplicado, skeleton, estado vazio e erro com `traceId`; o backend repete todas as regras e rejeita entidade fora do escopo.

## Alertas por regra

Regras persistidas usam escopo, métrica permitida, operador, limite, período e severidade. Eventos guardam valor, limite, mensagem e `source_json`, podendo passar de `Open` para `Acknowledged`, `Resolved` ou `Dismissed`; dispensa exige motivo. O mecanismo é determinístico e baseado em dados, sem alegação de IA.

## Relatórios, exportações e visões

`GET /api/analytics/reports/export` valida tipo, intervalo e escopo, produz CSV e registra a exportação. Os tipos incluem resumo executivo, operação diária, financeiro mensal, DRE, equipe/comissões, recorrência, estoque crítico, compras/fornecedores, pacotes/fidelidade e conciliação. Visões salvas pertencem ao usuário autenticado, armazenam apenas JSON produzido pelas seleções válidas e permitem uma visão padrão.

## Segurança, efeitos e limitações

Todos os endpoints exigem JWT, claims de tenant/filial e permissões `Analytics.Read`, `Analytics.Manage`, `Analytics.Export` ou `Analytics.Alerts`. Consultas são isoladas por tenant e filial; alterações geram auditoria. Exportar cria um registro, salvar visão persiste filtros e ações de alerta alteram o workflow do evento. Não há previsão estatística ou IA generativa; ocupação e recorrência avançada permanecem indisponíveis quando sua fonte canônica não oferece capacidade/segmentação suficiente.
# Métricas de comunicação

O escopo Relationship agrega `communication_outbox`, campanhas e preferências para mensagens enviadas, falhas, opt-outs e providers não configurados. Alertas recomendados cobrem taxa de falha, pendência envelhecida e `ProviderNotConfigured`.

## Indicadores de IA Operacional

Analytics classifica sugestões por unidade, serviço, profissional, câmera/zona e período, incluindo aprovação, rejeição, correção, confiança e tempo de revisão. Alertas cobrem provider não configurado, câmera inativa, fila acumulada, rejeição alta e queda de confiança; ausência de provider nunca é tratada como dado bem-sucedido.

## Governança SaaS
O acesso ao módulo e seus limites são definidos pela assinatura e por `tenant_module_settings`; módulo desabilitado deve falhar claramente, sem fallback. Consulte [GOVERNANCE_WORKFLOW.md](GOVERNANCE_WORKFLOW.md).

## Integração Clube & Vendas
Consulte `CLUB_AND_SALES_WORKFLOW.md` para contratos de assinatura, carteira, resgate, venda pendente, auditoria e regras de origem.

## Integração — Portal do Cliente (Sprint 51)

O fluxo client-scoped, seus limites de privacidade, eventos e comportamento sem provider estão documentados em [CLIENT_PORTAL_WORKFLOW.md](CLIENT_PORTAL_WORKFLOW.md). A integração não aceita identificadores técnicos digitados e não transforma intenção de pagamento em liquidação.

## Integração Qualidade & Retenção — Sprint 52

O contrato de integração, escopo, eventos e restrições está em [QUALITY_AND_RETENTION_WORKFLOW.md](QUALITY_AND_RETENTION_WORKFLOW.md). Os dados permanecem tenant/branch scoped; indisponibilidade não produz resultado fictício, e nenhuma integração usa biometria ou inferência de emoção.

## Integração Marketing Studio

O contrato de integração, atribuição e segurança está documentado em [MARKETING_STUDIO_WORKFLOW.md](MARKETING_STUDIO_WORKFLOW.md). Esta integração usa apenas dados persistidos, preserva o escopo tenant/unidade e não simula provider, pagamento ou conversão.

## Integração com Marketplace & Parceiros

A atribuição comercial usa referências rastreáveis e escopo tenant/unidade. Eventos pendentes ou cancelados não confirmam comissão/payout; detalhes e contratos estão em `docs/PARTNERS_MARKETPLACE_WORKFLOW.md`.

## Integração Sprint 57 — Catálogo & Precificação

A operação consome a fonte central de preço, custo, margem, duração, visibilidade e breakdown descrita em [CATALOG_PRICING_WORKFLOW.md](CATALOG_PRICING_WORKFLOW.md). Benefícios e comissões permanecem pendentes até o evento comercial real; escopo de tenant/unidade e trilha de auditoria são obrigatórios.
