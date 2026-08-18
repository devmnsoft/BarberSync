# Backlog de testes automatizados — fase final

O projeto `Backend/Tests/BarberSync.Tests` permanece versionado, mas está
temporariamente fora da solution principal e dos pipelines atuais. Nesta fase,
Debug e Release validam somente os projetos do sistema principal. A suíte será
reativada após o fechamento funcional, sem substituir validações de build, SQL,
JavaScript, Mobile ou Totem.

## Cobertura planejada

- ServiceRecognition
- AiSettings
- CopilotService
- UnconfiguredAiProvider
- PDV
- Caixa
- Agenda
- Atendimento
- Cliente 360
- Totem
- Mobile Cliente
- Mobile Profissional
- Financeiro
- Relatórios
- Auditoria
- Notificações
- Tenant/Branch
- Permissões

## Critério para reativação

Reincluir `BarberSync.Tests.csproj` em `BarberSync.sln` e restaurar a etapa
`dotnet test` nos pipelines somente na fase final, depois de atualizar testes,
mocks e stubs obsoletos para os contratos definitivos.
