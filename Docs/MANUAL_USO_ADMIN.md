# Manual de uso — AdminWeb

## Acesso

- URL: `http://localhost:8081/Admin`

## Módulos principais

- Dashboard: indicadores, agenda, comandas e alertas.
- Clientes: listar, criar, editar, detalhes, cliente 360 e excluir em modo demo.
- Profissionais: listar, criar, editar, inativar e visualizar performance.
- Serviços: catálogo multicanal com site, totem e mobile.
- Agenda: confirmar, check-in, iniciar, finalizar e cancelar atendimentos.
- Comandas: abrir, adicionar itens, pagar, fechar e visualizar recibo demo.
- Estoque: produtos, entrada, saída, crítico e reposição demo.
- Campanhas/Cupons: criação visual, status e resultados demo.
- Copilot: perguntas, respostas e ações demo.

## Observação

As telas usam `/AdminApi/...`; se a API interna falhar, o proxy retorna fallback demo com status 200 para manter a apresentação.
