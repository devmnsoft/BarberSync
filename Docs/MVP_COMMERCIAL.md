# BarberSync 2.0 — MVP Comercial

## Escopo MVP vendável
Prioridade dos fluxos para venda:
1. Login/permissões multiempresa.
2. Dashboard com KPIs operacionais e financeiros.
3. Clientes, profissionais e serviços (CRUD completo).
4. Agenda -> atendimento -> comanda -> pagamento -> caixa.
5. Estoque com baixa automática e alerta crítico.
6. Cashback/fidelidade.
7. Relatórios essenciais e Copilot mockado para demo.
8. Mobile Cliente e Totem navegáveis.

## Pronto para demonstração
- Base PostgreSQL no schema `barber` com seed comercial demo.
- Usuários demo e massa de dados para cenários comerciais.
- Mensagens amigáveis padronizadas.
- Soft delete, auditoria e validações mínimas de produção.

## Checklist de aceite
- Login ADMIN mostra dashboard e menu por perfil.
- Fluxo ponta a ponta: agendamento -> check-in -> comanda -> pagamento -> cashback.
- Dashboard com receita, agenda, operação e estoque crítico.
- SQL `barber_full_database_postgresql.sql` + `validate_barber_database.sql` executam sem ajustes manuais.
