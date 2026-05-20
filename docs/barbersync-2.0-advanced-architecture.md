# BarberSync 2.0 - Evolução Avançada

## Novos módulos
- **Advanced AI**: reconhecimento por vídeo, upsell, métricas de confiança/tempo e alertas de eficiência.
- **Loyalty & Finance**: pontos, cashback, recorrência e relatórios por serviço/profissional.
- **Smart Notifications**: push, Telegram, WhatsApp e alertas internos.
- **Analytics & BI**: KPIs de lucro/ocupação/satisfação, exportação para Power BI/Qlik.

## Clean Architecture e DDD
- **Presentation**: novos controllers REST em `Backend/Presentation/BarberSync.Api/Controllers`.
- **Application**: DTOs de orquestração de caso de uso em `Backend/Application/BarberSync.Application/DTOs`.
- **Infrastructure**: pronto para providers de mensageria/pagamento/cache.
- **Domain**: entidades e regras de negócio devem permanecer isoladas (próxima etapa: enriquecer agregados).

## Fluxo de notificações
1. Evento de agenda/pagamento/estoque gera `alerts_notifications`.
2. Job de despacho seleciona `status = queued`.
3. Publica nos conectores (Push/Telegram/WhatsApp).
4. Atualiza status (`sent`, `failed`) e registra auditoria.

## Fluxo IA vídeo + upsell
1. Cliente/profissional envia vídeo para endpoint de IA.
2. Modelo identifica serviço e calcula confiança.
3. Serviço grava em `ai_service_metrics`.
4. Motor de recomendação grava sugestões em `ai_upsell_recommendations`.
5. Painel Admin exibe eficiência e oportunidades de upsell em tempo real.

## DevOps e qualidade
- Criar pipelines separados para Backend, Frontend/Admin e Mobile/Totem.
- Executar testes unitários + integração + E2E em cada PR.
- Monitorar latência de IA, filas de notificação, taxa de conversão de upsell, ocupação e estoque crítico.
