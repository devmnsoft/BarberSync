# BarberSync Enterprise Consolidation — 2026-06-05

## Objetivo

Consolidar a demonstração enterprise do BarberSync como um fluxo vertical navegável e seguro para cliente, mantendo API ASP.NET Core, AdminWeb, PublicWeb, KioskWeb, MobileApp, Docker Compose, Seq, DemoStore e EventBus coerentes entre si.

## Topologia validada

| Componente | Porta local | Responsabilidade | Observação |
| --- | ---: | --- | --- |
| API ASP.NET Core | 8080 | Swagger, endpoints demo, FullServiceFlow, operações e configurações | Navegador acessa diretamente apenas para Swagger/health em validação técnica. |
| AdminWeb MVC | 8081 | Dashboard, Cliente 360, CRUDs, Fluxo Comercial e FullServiceFlow | Browser usa `/AdminApi/*`; chamadas server-side usam `ApiSettings:BaseUrl`. |
| PublicWeb MVC | 8082 | Landing page e agendamento público demo | Browser usa `/PublicApi/*`; nunca chama `http://api:8080`. |
| KioskWeb MVC | 8083 | Totem, check-in, seleção de serviço/profissional, pagamento mock e avaliação | Browser usa `/KioskApi/*`; nunca chama `http://api:8080`. |
| Postgres | 5432 | Banco demo inicializado por SQL | Montagem via `ScriptsSQL/barber_full_database_postgresql.sql`. |
| Seq | 5341 | Logs estruturados | API aponta para Seq no ambiente Docker. |
| MobileApp | Expo | Telas mobile demo | Consome API quando `EXPO_PUBLIC_API_URL` está definido e usa fallback local offline. |

## Fluxo FullServiceFlow vertical

1. Cliente criado/selecionado no Admin ou recebido via PublicWeb.
2. Agendamento criado e confirmado.
3. Check-in executado no Admin ou Kiosk.
4. Atendimento iniciado e finalizado.
5. Comanda aberta com serviço/produto.
6. Pagamento mock aprovado.
7. Recibo gerado e disponível para impressão/compartilhamento demo.
8. Estoque atualizado/reservado em modo demo.
9. Cashback gerado para o cliente.
10. Avaliação/NPS registrada.
11. Dashboard, Cliente 360, EventBus e DemoStore refletem o evento.

## Checklist de demonstração ao cliente

### Preparação

- Subir `docker compose up --build` em ambiente com Docker e .NET SDK disponíveis.
- Abrir Seq em `http://localhost:5341` e confirmar logs sem exceções críticas.
- Abrir Swagger em `http://localhost:8080/swagger` e confirmar carregamento do JSON.
- Abrir Admin em `http://localhost:8081/Admin/Dashboard`.
- Abrir PublicWeb em `http://localhost:8082`.
- Abrir Totem em `http://localhost:8083/Kiosk/Services`.

### Demonstração guiada

- No PublicWeb, enviar um agendamento demo e copiar o protocolo.
- No Admin, abrir **Lead to Cash** e demonstrar o lead criado/persistido localmente.
- No Admin, abrir **Atendimento Completo** e executar todos os passos do FullServiceFlow.
- No Totem, selecionar serviço, informar cliente, escolher profissional, confirmar, pagar e avaliar.
- No Admin, abrir Dashboard, Clientes, Comandas, Estoque, Fidelidade e Avaliações para demonstrar atualização visual.
- No MobileApp, mostrar resumo, próximo agendamento, cashback e histórico em fallback/integração demo.

## Validações HTTP recomendadas

### PowerShell

```powershell
$ErrorActionPreference = 'Stop'
$urls = @(
  'http://localhost:8080/health',
  'http://localhost:8080/swagger/v1/swagger.json',
  'http://localhost:8081/Admin/Dashboard',
  'http://localhost:8081/AdminApi/dashboard',
  'http://localhost:8082/',
  'http://localhost:8082/PublicApi/services',
  'http://localhost:8083/Kiosk/Services',
  'http://localhost:8083/KioskApi/services'
)
foreach ($url in $urls) {
  $r = Invoke-WebRequest -Uri $url -UseBasicParsing
  "{0} {1}" -f $r.StatusCode, $url
}
```

### Navegador

- `http://localhost:8081/Admin/Dashboard`
- `http://localhost:8081/Admin/FullServiceFlow`
- `http://localhost:8081/Admin/Clients`
- `http://localhost:8082/#agendamento`
- `http://localhost:8083/Kiosk/Services`
- `http://localhost:8080/swagger`

## Regras de estabilidade consolidadas

- Browser chama somente proxies locais (`/AdminApi`, `/PublicApi`, `/KioskApi`).
- `http://api:8080` fica restrito a comunicação server-side dentro do Docker Compose.
- Proxies retornam fallback demo com HTTP 200 quando a API está indisponível ou retorna payload vazio.
- Rotas duplicadas da API devem ser evitadas para não gerar `AmbiguousMatchException` ou HTTP 500.
- Serviços usados por controllers devem estar registrados em DI.
- DemoStore local persiste estado do fluxo e EventBus registra eventos para atualização de dashboard/Cliente 360.
