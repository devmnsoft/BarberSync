# BarberSync Demo Enterprise — Runbook de Navegação

## Objetivo

Este roteiro consolida a demonstração navegável do BarberSync com API, AdminWeb, PublicWeb, KioskWeb e MobileApp conectados por proxies relativos e endpoints demo persistentes em memória/localStorage.

## Subida local/Docker

1. Suba a stack completa:
   ```bash
   docker compose up --build
   ```
2. Acesse os canais:
   - API/Swagger: `http://localhost:8080/swagger`
   - AdminWeb: `http://localhost:8081/Admin`
   - PublicWeb: `http://localhost:8082/`
   - KioskWeb: `http://localhost:8083/Kiosk`
   - Seq: `http://localhost:5341`

## Regras de integração

- O browser do Admin consome somente `/AdminApi/*`.
- O browser do PublicWeb consome somente `/PublicApi/*`.
- O browser do KioskWeb consome somente `/KioskApi/*`.
- `ApiSettings:BaseUrl` é server-side e aponta para `http://api:8080` apenas dentro do Docker.
- Eventos de fluxo são registrados no `BarberSyncEventBus`, em `dashboardEvents`, relatórios demo e logs Serilog/Seq.

## Roteiro comercial FullServiceFlow

1. AdminWeb → **Atendimento Completo** (`/Admin/FullServiceFlow`).
2. Criar ou selecionar cliente.
3. Criar agendamento com serviço/profissional.
4. Confirmar, fazer check-in e iniciar atendimento.
5. Abrir comanda, adicionar serviço e produto.
6. Pagar a comanda; o fluxo registra pagamento, baixa de estoque, recibo e cashback.
7. Gerar recibo, cashback e avaliação.
8. Concluir o fluxo e validar atualização em Dashboard, Cliente 360, Operações, Estoque, Fidelidade e Avaliações.

## Fluxo PublicWeb

1. Abra `http://localhost:8082/`.
2. Escolha um serviço e envie o formulário de agendamento.
3. O PublicWeb usa `/PublicApi/appointments`, recebe protocolo demo e registra fallback local quando offline.
4. Valide o lead/agendamento no Admin em Lead-to-Cash e CommercialFlow.

## Fluxo KioskWeb

1. Abra `http://localhost:8083/Kiosk/Services`.
2. Selecione serviço, cliente, profissional, confirme e simule pagamento.
3. O totem usa `/KioskApi/payment/mock`, grava resumo em `sessionStorage` e envia evento para API demo.
4. Finalize em sucesso e registre avaliação.

## MobileApp

- O app consulta `EXPO_PUBLIC_API_URL` ou `http://localhost:8080`.
- Quando a API está disponível, reflete agendamentos, cashback e snapshot do FullServiceFlow.
- Quando offline, mantém fallback demo seguro para apresentação.

## Checklist QA para demo

- Swagger abre sem autenticação em `/swagger`.
- `/health` retorna 200.
- Admin não possui tela vazia nos módulos principais.
- Nenhum fetch de browser aponta para `http://api:8080`.
- Assets principais de `css`, `js` e `img` retornam 200.
- Seq recebe logs de API, pagamentos mock, totem e auditoria.
- FullServiceFlow conclui de Cliente até Avaliação sem `ReferenceError` no console.
