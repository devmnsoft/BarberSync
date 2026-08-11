# Instalação local — BarberSync 2.0

## Pré-requisitos

- .NET SDK 10
- PostgreSQL 14 ou superior (ou Docker)
- PowerShell 7 para os scripts operacionais
- Node.js 20 para os clientes JavaScript

## Banco canônico

`ScriptsSQL/script_completo.sql` é o único script oficial. Com o PostgreSQL disponível em `localhost:5432`:

```bash
createdb -h localhost -U postgres barber
for replay in 1 2 3; do
  psql -v ON_ERROR_STOP=1 -h localhost -U postgres -d barber -f ScriptsSQL/script_completo.sql
done
```

O script usa transação, advisory lock e mantém o histórico em `barber.schema_versions`.

## Inicialização e primeiro administrador

Configure `ConnectionStrings__DefaultConnection` e `Jwt__SigningKey` (mínimo de 32 caracteres), inicie a API e execute:

```powershell
$password = Read-Host 'Senha forte' -AsSecureString
./Scripts/create-superadmin.ps1 -Email 'voce@empresa.com.br' -Password $password `
  -FullName 'Administrador da Empresa' -TenantSlug 'minha-empresa' -BranchCode 'MATRIZ'
```

O script envia a senha somente ao endpoint local protegido `POST /api/setup/first-admin`. A API gera um hash ASP.NET Identity, cria ou reutiliza empresa/unidade, atribui `SuperAdmin` e `Owner` e registra auditoria. Depois que existe um usuário ativo, novas chamadas recebem `409 Conflict`.

## Build e testes

```bash
dotnet restore BarberSync.sln
dotnet build BarberSync.sln --configuration Release --no-restore
dotnet test BarberSync.sln --configuration Release --no-build
find Web -type f -name '*.js' -print0 | xargs -0 -n1 node --check
```

## Execução

Use `./Scripts/start-local.ps1` ou `docker compose up --build`. Portas padrão:

| Aplicação | URL |
|---|---|
| API / Swagger | `http://localhost:5080/swagger` |
| Admin | `http://localhost:5081/Account/Login` |
| Site público | `http://localhost:5082/` |
| Totem | `http://localhost:5083/` |

Não existe senha padrão. Entre no Admin com o e-mail e a senha fornecidos de forma segura ao script de criação.
