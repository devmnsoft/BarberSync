# BarberSync — Runbook de estabilização e demonstração

Este runbook consolida a execução demonstrável do BarberSync com AdminWeb, PublicWeb, KioskWeb, Totem/Mobile e API. A orientação principal é manter o navegador sempre em rotas web/proxy e deixar `ApiSettings:BaseUrl` restrito ao server-side.

## Mapa de acesso local

| Módulo | URL local | Observação |
| --- | --- | --- |
| API | `http://localhost:8080` | Swagger nativo em `/swagger` quando a API estiver exposta. |
| AdminWeb | `http://localhost:8081/Admin` | Painel operacional, Dashboard, FullServiceFlow e Central da Demo. |
| PublicWeb | `http://localhost:8082/` | Landing/agendamento público demo. |
| KioskWeb | `http://localhost:8083/Kiosk/Services` | Fluxo Totem ASP.NET MVC ponta a ponta. |
| Totem Vite | `http://localhost:5173/` | Usa `/KioskApi` e proxy de desenvolvimento do Vite; não usa host direto de API no browser. |
| Mobile | Expo local | Smoke test valida o scaffold e telas base. |

## Regras de proxy no browser

- AdminWeb deve consumir somente `/AdminApi/*`.
- PublicWeb deve consumir somente `/PublicApi/*`.
- KioskWeb e Totem Vite devem consumir somente `/KioskApi/*`.
- Swagger via AdminWeb pode ser validado por `/AdminApi/swagger.json`, que proxy a API e entrega um contrato demo se a API estiver indisponível.
- `http://api:8080` é permitido apenas dentro do Docker/server-side via `ApiSettings:BaseUrl`.

## Checklist do fluxo vertical

1. Abrir AdminWeb em `/Admin/FullServiceFlow`.
2. Criar ou selecionar cliente.
3. Criar agendamento e confirmar.
4. Fazer check-in e iniciar atendimento.
5. Finalizar atendimento e abrir comanda.
6. Adicionar serviço/produto; validar baixa de estoque demo.
7. Pagar com PIX/cartão/dinheiro mock.
8. Gerar recibo.
9. Gerar cashback.
10. Registrar avaliação.
11. Conferir reflexos em Dashboard, Cliente 360, Comandas, Estoque, Loyalty/Cashback e Reviews.

## Smoke checks recomendados

```bash
npm test --prefix Totem
npm run build --prefix Totem
npm test --prefix MobileApp
```

Quando o SDK .NET e Docker estiverem disponíveis no ambiente, executar também:

```bash
dotnet build BarberSync.sln --nologo
docker compose config --quiet
docker compose up --build
```

## Critérios de aceite para demonstração

- Nenhuma tela crítica vazia: se a API real falhar, os proxies retornam dados demo com `success=true`.
- Nenhum botão do roteiro principal sem ação: ações persistem no DemoStore/EventBus ou executam fallback mock.
- Nenhuma chamada direta do browser ao host interno Docker `api:8080`.
- Logs críticos devem se limitar a indisponibilidade real de infraestrutura; proxies devem degradar para fallback demo.
- FullServiceFlow deve atualizar o estado demo compartilhado, permitindo apresentar o ciclo Cliente → Agendamento → Atendimento → Comanda → Pagamento → Recibo → Estoque → Cashback → Avaliação → Dashboard.
