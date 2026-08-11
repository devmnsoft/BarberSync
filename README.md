# BarberSync 2.0

Plataforma multiempresa para agenda, atendimento, comandas, caixa, estoque, relacionamento e operação de barbearias e salões.

## Stack suportada

- ASP.NET Core / .NET 10
- PostgreSQL 14+
- AdminWeb, PublicWeb e KioskWeb em ASP.NET Core MVC
- MobileApp e Totem em Node.js 20

## Início rápido

1. Configure os segredos a partir de `.env.example`; nunca grave senhas ou chaves no repositório.
2. Inicialize o banco exclusivamente com `ScriptsSQL/script_completo.sql`.
3. Execute `dotnet restore BarberSync.sln`, build e testes.
4. Inicie os serviços com `Scripts/start-local.ps1` ou Docker Compose.
5. Crie o primeiro acesso com `Scripts/create-superadmin.ps1`.

Não há usuário ou senha padrão. O fluxo de primeiro administrador é bloqueado automaticamente quando já existe um usuário ativo.

Os comandos completos de banco, replay idempotente, criação segura de SuperAdmin, URLs e validações estão em [Docs/SETUP_LOCAL.md](Docs/SETUP_LOCAL.md).

## Projetos

- `Backend/Domain`: entidades e regras de domínio.
- `Backend/Application`: contratos, DTOs e casos de uso.
- `Backend/Infrastructure`: PostgreSQL, autenticação e integrações.
- `Backend/Presentation/BarberSync.Api`: API HTTP protegida por JWT.
- `Web/BarberSync.AdminWeb`: administração autenticada.
- `Web/BarberSync.PublicWeb`: experiência pública.
- `Web/BarberSync.KioskWeb`: experiência de autoatendimento.
- `Backend/Tests/BarberSync.Tests`: testes automatizados.

## Qualidade

O workflow principal restaura, compila e testa a solution, verifica a sintaxe JavaScript e executa o script SQL canônico três vezes contra PostgreSQL 16 com `ON_ERROR_STOP=1`.
