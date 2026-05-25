# Planejamento inicial do app BarberSync

## 1. Objetivo
Entregar um app mobile para profissionais e gestão acompanharem agenda, convites, pagamentos e notificações em tempo real.

## 2. Público
- Profissionais (barbeiros)
- Coordenação/gestão
- Financeiro (fase futura)

## 3. Stack recomendada
- React Native + Expo (MVP mais rápido)
- Alternativa: .NET MAUI (sinergia com stack .NET)

## 4. Telas MVP
Login, Dashboard, Plantões disponíveis, Detalhes do plantão, Solicitar plantão, Convites, Aceitar/Recusar, Minhas escalas, Meus pagamentos, Notificações, Perfil, Disponibilidade e Preferências.

## 5. Fluxo JWT
1. Login em `/api/mobile/auth/login`.
2. Recebe `accessToken` curto.
3. Armazena em secure storage.
4. Envia `Authorization: Bearer` nas requisições.

## 6. Endpoints usados
Ver `docs/mobile-api-endpoints.md`.

## 7. Navegação
- Bottom Tabs: Dashboard, Agenda, Convites, Pagamentos, Perfil.
- Stack por módulo para detalhes e edições.

## 8. Modelo visual
- Cards grandes, foco touch.
- Contraste alto e tipografia legível.
- Estado vazio e feedback de carregamento.

## 9. Segurança
- JWT expirável.
- Sem exibir dados sensíveis.
- Filtros por usuário autenticado.
- Logging sem payload sensível.

## 10. Próximos passos
1. Adicionar refresh token.
2. Integrar endpoints reais de domínio.
3. Implementar push notifications.
4. Publicar build interno via Expo EAS.
