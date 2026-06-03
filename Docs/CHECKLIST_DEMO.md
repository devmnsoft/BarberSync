# Checklist de demonstração BarberSync 7.0

1. Abrir Swagger em `http://localhost:8080/swagger`.
2. Abrir Admin em `http://localhost:8081/Admin`.
3. Confirmar menu final por OPERAÇÃO, CADASTROS, RELACIONAMENTO, GESTÃO, CANAIS e SISTEMA.
4. Configurar identidade em `/Admin/PlatformSettings` e salvar.
5. Ajustar PublicWeb em `/Admin/PublicSite` e atualizar preview.
6. Ajustar Totem em `/Admin/Kiosk`, simular atendimento e abrir `http://localhost:8083/Kiosk/Services`.
7. Demonstrar usuários/perfis, unidades, auditoria e notificações.
8. Demonstrar Reports 2.0 e Dashboard 7.0 com filtros.
9. Acionar Copilot 5.0 e executar ações contextuais.
10. Validar que o browser não chama URLs internas; deve usar `/AdminApi`, `/PublicApi` e `/KioskApi`.
