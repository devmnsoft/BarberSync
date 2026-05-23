INSERT INTO competitors(id, tenant_id, name, positioning) VALUES (gen_random_uuid(), gen_random_uuid(), 'Studio Alpha', 'Premium');
INSERT INTO supplier_profiles(id, tenant_id, name, category) VALUES (gen_random_uuid(), gen_random_uuid(), 'Fornecedor Prime', 'Cosméticos');
INSERT INTO dynamic_price_suggestions(id, service_id, current_price, suggested_price, reason) VALUES (gen_random_uuid(), gen_random_uuid(), 90, 105, 'Alta demanda em horários de pico');
