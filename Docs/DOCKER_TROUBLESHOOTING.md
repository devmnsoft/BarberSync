# Docker troubleshooting - BarberSync

Este guia separa problemas de **codigo/aplicacao** de problemas de **rede, registry, DNS, proxy, firewall ou Docker Desktop** durante builds Docker do BarberSync.

## Erro ao baixar imagem `mcr.microsoft.com/dotnet/sdk:8.0`

Sintomas comuns:

```text
failed to resolve source metadata for mcr.microsoft.com/dotnet/sdk:8.0
failed to resolve source metadata for mcr.microsoft.com/dotnet/aspnet:8.0
failed to do request: Head "https://mcr.microsoft.com/v2/dotnet/sdk/manifests/8.0": EOF
```

Quando o erro acontece antes das etapas `COPY`, `dotnet restore`, `dotnet build` ou `dotnet publish`, o Docker falhou ao resolver ou baixar a imagem base no Microsoft Container Registry (`mcr.microsoft.com`). Isso indica problema de rede/registry/ambiente Docker, **nao erro de C# nem de Razor**.

As imagens esperadas para este projeto sao:

- `mcr.microsoft.com/dotnet/sdk:8.0`
- `mcr.microsoft.com/dotnet/aspnet:8.0`

O projeto deve permanecer em .NET 8 enquanto os `.csproj` usarem `net8.0`; nao troque para .NET 9/10 para contornar falha de rede.

## Pre-check automatizado

No Windows/PowerShell, execute na raiz do repositorio:

```powershell
powershell -ExecutionPolicy Bypass -File Scripts/docker-precheck.ps1
```

O script testa conectividade HTTPS com `mcr.microsoft.com`, valida o Docker CLI/daemon e faz pull das duas imagens base .NET 8. Se falhar, trate como problema de rede/registry/Docker Desktop antes de investigar codigo.

## Comandos de diagnostico manual

### 1. Testar pulls das imagens base

```powershell
docker pull mcr.microsoft.com/dotnet/sdk:8.0
docker pull mcr.microsoft.com/dotnet/aspnet:8.0
```

Se qualquer pull falhar com `failed to resolve source metadata`, `Head`, `EOF`, timeout, TLS, DNS ou proxy, o problema esta fora do codigo da aplicacao.

### 2. Testar conectividade com o registry

```powershell
Test-NetConnection mcr.microsoft.com -Port 443
```

`TcpTestSucceeded: True` confirma que a porta 443 esta acessivel a partir do host. Se estiver `False`, investigue rede local, firewall, proxy, VPN ou DNS.

### 3. Limpar caches/builders do Docker

Use quando houver suspeita de cache corrompido ou metadata antiga:

```powershell
docker buildx prune -af
docker builder prune -af
```

### 4. Rebuild verboso sem cache

```powershell
docker compose build --no-cache --progress=plain
```

### 5. Subir ambiente

```powershell
docker compose up -d --build
```

## Checklist para falha de rede/registry

- Reiniciar Docker Desktop.
- Verificar internet.
- Verificar proxy/firewall corporativo.
- Verificar DNS.
- Tentar novamente sem VPN, se estiver usando.
- Confirmar se Docker Desktop esta logado e funcional.
- Verificar se `https://mcr.microsoft.com` esta acessivel pelo navegador.
- Testar em outra rede para isolar bloqueio local/corporativo.

## Validacao local sem Docker

Antes de atribuir erro ao codigo, valide a solucao fora do Docker:

```powershell
dotnet build
dotnet build Backend/Presentation/BarberSync.Api/BarberSync.Api.csproj
dotnet build Web/BarberSync.AdminWeb/BarberSync.AdminWeb.csproj
dotnet build Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj
dotnet build Web/BarberSync.KioskWeb/BarberSync.KioskWeb.csproj
```

Se esses comandos passarem, mas o Docker falhar ao baixar `mcr.microsoft.com/dotnet/sdk:8.0` ou `mcr.microsoft.com/dotnet/aspnet:8.0`, a causa provavel e rede/registry/Docker Desktop.

## Portas esperadas no Docker Compose

- API: `http://localhost:8080/swagger`
- Admin Web: `http://localhost:8081/Admin`
- Public Web: `http://localhost:8082/`
- Kiosk Web: `http://localhost:8083/Kiosk/Services`
- Seq: `http://localhost:5341`

Os web apps usam `ApiSettings__BaseUrl=http://api:8080` para comunicacao server-side entre containers. Esse endereco e interno da rede Docker Compose e nao deve ser exposto como URL chamada diretamente por JavaScript no browser.
