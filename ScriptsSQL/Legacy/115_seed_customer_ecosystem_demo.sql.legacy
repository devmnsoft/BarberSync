-- Demo seed intentionally compact; IDs are deterministic for local/demo use.
INSERT INTO customer_subscription_plans (id, tenant_id, name, billing_cycle, price) VALUES
('11111111-1111-1111-1111-111111111111','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','Clube Corte Mensal','MONTHLY',99.90)
ON CONFLICT (id) DO NOTHING;
INSERT INTO benefit_clubs (id, tenant_id, name) VALUES
('22222222-2222-2222-2222-222222222222','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','Seu Clube VIP')
ON CONFLICT (id) DO NOTHING;
INSERT INTO customer_wallets (id, tenant_id, client_id, cashback_balance, credit_balance) VALUES
('33333333-3333-3333-3333-333333333333','aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa','bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',25,180)
ON CONFLICT (id) DO NOTHING;
