# BarberSync Demo Experience 1.0 — Checklist

Use este checklist antes de apresentar o BarberSync para clientes reais.

## Ambiente

- [ ] API abre em `http://localhost:8080/swagger`.
- [ ] Admin abre em `http://localhost:8081/Admin`.
- [ ] Dashboard abre em `http://localhost:8081/Admin/Dashboard`.
- [ ] PublicWeb abre em `http://localhost:8082/`.
- [ ] Totem abre em `http://localhost:8083/Kiosk/Services`.
- [ ] Browser não chama `http://api:8080`; somente proxies locais.

## Fluxo comercial

- [ ] PublicWeb mostra headline, CTA, serviços, profissionais, planos e formulário.
- [ ] Agendamento público funciona via `/PublicApi/appointments`.
- [ ] Admin tem command palette com Ctrl + K.
- [ ] Ajuda contextual abre no Admin.
- [ ] Onboarding mostra passos de demonstração.
- [ ] Dashboard tem saúde da operação, painel Hoje, ações recomendadas e botões de demo.

## Operação Admin

- [ ] Cliente cria/edita/exclui e aparece na tela via demo store.
- [ ] Cliente 360 mostra tags, jornada, cashback e ações rápidas.
- [ ] Serviço cria e mostra canais Site/Totem/Mobile.
- [ ] Agenda muda status: confirmar, check-in, iniciar, finalizar e cancelar.
- [ ] Comanda abre, simula pagamento e imprime recibo visual.
- [ ] Estoque mostra produto crítico e gera reposição demo.
- [ ] Campanhas, cupons, fidelidade e avaliações têm dados e ações.
- [ ] Copilot responde e oferece ações para campanhas, estoque, agenda e clientes.

## Totem

- [ ] Totem salva estado em `sessionStorage`.
- [ ] Totem permite voltar, cancelar e pedir ajuda.
- [ ] Totem mostra resumo lateral.
- [ ] Pagamento mock conclui o fluxo.
- [ ] Sucesso redireciona após 10 segundos.
- [ ] Avaliação por estrelas funciona.

## Validação técnica

- [ ] `dotnet build` concluído.
- [ ] `npm install` concluído no MobileApp.
- [ ] `npm test` concluído no MobileApp.
- [ ] `docker compose build --no-cache` concluído.
- [ ] `docker compose up -d` concluído.
- [ ] Logs não mostram erro crítico da aplicação.
