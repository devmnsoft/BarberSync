# Classificação de demo/fallback — Sprint de Produção 17

Classificação objetiva do resultado do comando literal solicitado, executado em 20 de agosto de 2026. O scan encontrou **48 arquivos**. A classificação não transforma ocorrência literal em defeito automaticamente e, principalmente, não declara como resolvido um asset operacional que ainda depende de estado demo.

## Resultado

- **1 operacional corrigido agora:** o backend deixou de fabricar os UUIDs fixos de tenant/unidade para requisições anônimas.
- **26 dev-only:** permitidos apenas quando a rota correspondente está protegida por `DevelopmentOnly`, que exige ambiente Development e papel SuperAdmin.
- **1 testes:** não alterados nesta sprint.
- **4 documentação/legacy:** não carregados pela view operacional atual; remoção estrutural segue pendente.
- **1 gerados/build:** ocorrências transitivas do lockfile.
- **15 operacionais pendentes:** inventário explícito para migração; não são considerados aprovados.

| Arquivo | Classe | Evidência/decisão |
| --- | --- | --- |
| `Backend/Presentation/BarberSync.Api/Services/Enterprise/EnterpriseDataService.cs` | Operacional — corrigido agora | Fallback de tenant/unidade fixos removido; escopo anônimo agora exige UUID provisionado na configuração. |
| `MobileApp/package-lock.json` | Gerado/build | Lockfile do npm; ocorrências pertencem às dependências Jest de timers/mocks, não ao runtime publicado. |
| `Web/BarberSync.AdminWeb/Views/Admin/Automations.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/Views/Admin/CommercialFlow.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/Views/Admin/DemoCenter.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/Views/Admin/DemoWizard.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/Views/Admin/Diagnostics.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/Views/Admin/Help.cshtml` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/Views/Admin/Index.cshtml` | Documentação/legacy | Asset/view legado não referenciado pela view operacional atual; manter até remoção estrutural em sprint própria. |
| `Web/BarberSync.AdminWeb/Views/Admin/Integrations.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/Views/Admin/Kiosk.cshtml` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/Views/Admin/LeadToCash.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/Views/Admin/PlatformSettings.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/Views/Admin/SaasControlCenter.cshtml` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/css/admin-commercial-flow.css` | Documentação/legacy | Asset/view legado não referenciado pela view operacional atual; manter até remoção estrutural em sprint própria. |
| `Web/BarberSync.AdminWeb/wwwroot/css/admin-platform-settings.css` | Documentação/legacy | Asset/view legado não referenciado pela view operacional atual; manter até remoção estrutural em sprint própria. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-addons.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-api-client.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-automations.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-campaigns.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-channel-manager.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-client-onboarding.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-commercial-flow.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-copilot.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-customer-journey.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-dashboard.js` | Documentação/legacy | Asset/view legado não referenciado pela view operacional atual; manter até remoção estrutural em sprint própria. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-demo-center.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-demo-experience.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-demo-mode.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-demo-store.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-demo-tour.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-demo-wizard.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-diagnostics.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-full-service-flow.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-help.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-integrations.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-knowledge-base.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-lead-to-cash.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-operation-flow.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-platform-settings.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-reviews.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-saas-control-center.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-saas7.js` | Dev-only | Recurso de demonstração acessível somente pela action `DevelopmentOnly` (Development + SuperAdmin), ou asset exclusivo dessas views. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-service-orders.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-stock.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/admin-subscription.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |
| `Web/BarberSync.AdminWeb/wwwroot/js/tests/demo-store-tests.js` | Teste | Teste de navegador da ferramenta de demonstração; permanece reservado à fase final. |
| `Web/BarberSync.PublicWeb/wwwroot/js/public.js` | Operacional — pendente | Asset/view alcançável na aplicação; ocorrência precisa migrar para contrato persistido/API ou ser removida, sem simular sucesso. |

## Decisão

A ocorrência operacional do backend foi corrigida sem novo fallback: chamadas autenticadas continuam exigindo claims válidas, enquanto PublicWeb/Totem passam a exigir `BarberSync:DefaultTenantId` e `BarberSync:DefaultBranchId` válidos no deployment. O compose do gate fornece explicitamente os UUIDs do banco efêmero.

Os itens **Operacional — pendente** não estão liberados para produção apenas por estarem classificados. A próxima limpeza deve substituir persistência `localStorage`/`DemoStore` e ações mock por APIs reais ou retirar a ação da superfície operacional. O estado do candidato permanece **NO-GO**.
