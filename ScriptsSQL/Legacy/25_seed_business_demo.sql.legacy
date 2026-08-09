insert into business_insights (id, tenant_id, insight_type, priority, description, expected_impact, suggested_action, executable)
values (gen_random_uuid(), gen_random_uuid(), 'LOW_OCCUPANCY', 'ALTA', 'Horário 15h com baixa ocupação', '+10% de agendamentos', 'Criar promoção por horário ocioso', true)
on conflict do nothing;
