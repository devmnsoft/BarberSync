# Clube & Vendas

## Princípios
O módulo é escopado por tenant e unidade, exige autenticação no Admin e nunca libera assinatura, gift card ou pedido online sem confirmação real. Pedidos públicos nascem `PendingPayment`. Códigos são gerados com CSPRNG, exibidos uma única vez e persistidos somente como SHA-256 e máscara.

## Jornadas
1. O gestor publica um plano e benefícios com validade e limite.
2. A assinatura administrativa nasce pendente, exceto quando o operador possui permissão e declara ativação administrativa. Ativação abre ciclo; suspensão, vencimento ou cancelamento bloqueiam resgates e preservam histórico.
3. A carteira mantém saldos não negativos. Crédito, débito, cashback, expiração e reversão sempre geram uma transação com origem e motivo, sob bloqueio transacional.
4. Gift cards aguardam pagamento, aceitam resgate parcial quando a regra autoriza e recusam códigos expirados, cancelados ou sem saldo. Vouchers validam janela, escopo, limite total e por cliente.
5. A loja cria pedido pendente. Sem gateway, informa que a confirmação deve ocorrer no estabelecimento; não existe sucesso simulado.

## Integrações
O PDV consulta resumo comercial antes de calcular descontos e grava a origem do resgate. Cancelar a comanda cria reversão, nunca remove o lançamento. Cliente 360 apresenta assinatura, próximo ciclo, saldos e extrato. Agenda usa prioridade apenas como critério de ordenação, sem contornar conflitos. Financeiro relaciona ciclo, recebível e pagamento; BI calcula MRR, churn e conversão das tabelas comerciais ou devolve `sourceStatus: unavailable`. Comunicação publica eventos InApp e só usa canal externo com provider e consentimento. Mobile e Kiosk aplicam o mesmo escopo e regras; o Kiosk preserva `Kiosk:DeviceCode` obrigatório.

## Operação e segurança
Permissões `Club.Read`, `Club.*.Manage` e `Club.Reports.Export` separam leitura, gestão e exportação. Erros usam ProblemDetails e `traceId`. Comprador e beneficiário são dados pessoais sujeitos a retenção, minimização, auditoria e solicitações LGPD. Nenhuma interface solicita UUID; seleções usam opções carregadas da unidade, enquanto códigos comerciais permanecem campos textuais legítimos.
