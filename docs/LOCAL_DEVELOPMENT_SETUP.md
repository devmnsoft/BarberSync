# Execução local do BarberSync

## Como rodar BarberSync local com API + Admin + Public + Kiosk

Os gateways Web leem a API em `ApiSettings:BaseUrl`. O Kiosk também exige explicitamente
`Kiosk:DeviceCode`; não existe identidade padrão em runtime. Os scripts abaixo armazenam os
valores em **user-secrets**, nunca nos `appsettings` versionados.

```powershell
cd C:\MNSOFT\BarberSync

.\Scripts\setup-api-local-dev.ps1 -ConnectionString "Host=localhost;Port=5432;Database=barbersync;Username=postgres;Password=SUA_SENHA"
.\Scripts\setup-web-local-dev.ps1 -ApiBaseUrl "https://localhost:7088" -KioskDeviceCode "KIOSK-LOCAL-001"

.\Scripts\check-api-config.ps1
.\Scripts\check-web-config.ps1
```

Abra quatro terminais e execute:

```powershell
dotnet run --project .\Backend\Presentation\BarberSync.Api\BarberSync.Api.csproj --launch-profile https
dotnet run --project .\Web\BarberSync.AdminWeb\BarberSync.AdminWeb.csproj
dotnet run --project .\Web\BarberSync.PublicWeb\BarberSync.PublicWeb.csproj
dotnet run --project .\Web\BarberSync.KioskWeb\BarberSync.KioskWeb.csproj
```

URLs previsíveis dos perfis locais:

| Projeto | URL |
|---|---|
| API | `https://localhost:7088` ou `http://localhost:5080` |
| Admin | `http://localhost:5081` |
| Public | `http://localhost:5082` |
| Kiosk | `http://localhost:5083/Kiosk` |

O certificado HTTPS local pode ser confiado com `dotnet dev-certs https --trust`. Se o Visual
Studio iniciar a API em uma porta dinâmica (por exemplo, `https://localhost:59932`), execute
novamente `setup-web-local-dev.ps1` com essa URL real. Não misture a porta do Admin com a da API.

### Configurar somente o totem

```powershell
.\Scripts\setup-kiosk-local-dev.ps1 -DeviceCode "KIOSK-LOCAL-001" -ApiBaseUrl "https://localhost:7088"
```

Também é possível provisionar o código por `Kiosk__DeviceCode` ou
`BARBERSYNC_Kiosk__DeviceCode`. Uma query string `deviceCode` válida prevalece apenas naquela
requisição, é registrada como origem `QueryString` e não é persistida. São aceitos de 5 a 64
caracteres alfanuméricos, ponto, hífen e sublinhado, sem espaços.

Sem configuração, o Kiosk mostra **Totem não configurado** e a API do gateway retorna um erro
controlado com `KIOSK_DEVICE_NOT_CONFIGURED` e `traceId`. Se a API operacional estiver offline,
Admin, Public e Kiosk retornam erro operacional explícito; nenhum deles fabrica dados ou sucesso.

### Diagnóstico

`check-web-config.ps1` confirma a chave da API nos três projetos, oculta o DeviceCode e consulta
`/health`. Se falhar, confira primeiro a URL/porta apresentada e mantenha o processo
`BarberSync.Api` aberto. O `localhost:5080` é a porta HTTP fixa do launch profile e também é usado
pelos contratos de readiness; `https://localhost:7088` é a alternativa HTTPS recomendada.
