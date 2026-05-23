# AI Copilot (BarberSync Copilot)

## Endpoints
- `GET /api/copilot/conversations`
- `GET /api/copilot/messages`
- `POST /api/copilot/ask`
- `GET /api/copilot/suggestions`
- `POST /api/copilot/actions`
- `POST /api/copilot/feedback`

## Fluxo inicial
1. Admin envia pergunta para `/api/copilot/ask`.
2. `MockAiProvider` retorna resposta simulada.
3. Sistema armazena mensagens e gera sugestões inteligentes.

## Próximos passos
- Implementar `OpenAiProvider` e `AzureOpenAiProvider` mantendo a interface `IAiProvider`.
- Persistir dados em banco no lugar de coleção em memória.
