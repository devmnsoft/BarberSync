# Manual de uso — AdminWeb BarberSync 11.0

## Acesso

URL: `http://localhost:8081/Admin`.

## Módulos de operação

- Dashboard: visão executiva de receita, agenda, canais e alertas.
- Clientes: listar, criar, editar, excluir e abrir visão 360.
- Profissionais: cadastro, disponibilidade, performance e inativação visual.
- Serviços: preço, duração, disponibilidade em Site/Totem/Mobile e status.
- Agenda: criar, confirmar, check-in, iniciar, finalizar e cancelar.
- Comandas: abrir, adicionar itens, pagar, fechar e emitir recibo demo.
- Estoque: produtos, entrada, saída, crítico e reposição demo.
- Campanhas/Cupons: criar, editar, status e resultado demo.
- Copilot: perguntar, receber sugestão e executar ação demo.

## Integração segura

O JavaScript do Admin chama apenas `/AdminApi/...`. A URL interna da API é lida server-side pelo proxy MVC.
