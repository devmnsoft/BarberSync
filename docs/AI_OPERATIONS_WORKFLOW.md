# IA Operacional — fluxo auditável

## Escopo e privacidade

O módulo observa **sinais operacionais** (cadeira, lavatório, toalha, máquina, navalha e produto), nunca identifica pessoas e proíbe reconhecimento facial, biometria e classificação de atributos físicos. `privacy_mode` é `MetadataOnly`, `BlurredSnapshot` ou `NoImage`; snapshots só podem existir quando autorizados, redigidos e com retenção limitada. Evidências priorizam evento, zona, horário, confiança, metadados e hash.

## Câmeras, zonas, sessões e regras

Câmeras são escolhidas por nome/local e zonas pelo catálogo da unidade. Regras associam um serviço ativo, sinais em checkbox, confiança entre 0 e 1 e revisão humana invariavelmente habilitada. Eventos carregam apenas IDs provenientes dessas seleções e metadados operacionais. O provider desabilitado responde `ProviderNotConfigured`/`NotConfigured`, registra execução ignorada e não fabrica uma sugestão.

## Sugestão e revisão humana

Toda correspondência nasce em `PendingReview`, com confiança, regra e expiração. A fila impede decisão duplicada ou fora do tenant/filial. Aprovar exige comanda aberta (ou pré-comanda criada por seleções reais), adiciona um item com origem `AiSuggestion` e registra usuário/traceId. Corrigir exige serviço do catálogo e motivo; rejeitar exige motivo. Nenhuma ação registra pagamento, baixa financeira, comissão ou consumo: esses efeitos continuam no fluxo normal após confirmação do PDV.

## Admin, relatórios e integrações

`/AiOperations` oferece dashboard, câmeras, zonas, regras, fila, evidências, relatórios e configurações. Os formulários usam `form-validation.js`, validação HTML/backend, bloqueio de envio duplicado, estados de loading/vazio/erro e traceId; IDs são somente values internos de selects. Operação do Dia mostra a contagem pendente. Métricas alimentam Analytics (volume, aprovação, rejeição, correção e tempo), e alertas de câmera/provider/fila podem gerar Inbox InApp. Totem e Mobile não ativam câmera sem dispositivo/provider explicitamente configurado.

## Segurança, permissões e limitações

Todos os endpoints usam autenticação, claims de tenant/filial e papéis operacionais; o catálogo inclui `AiOperations.Read`, `Manage`, `Review`, `Export` e `Settings`. Evidências e exportações dependem da permissão correspondente. A versão atual processa regras sobre metadados; provider visual externo e download de snapshots permanecem indisponíveis até configuração segura e avaliação LGPD.
