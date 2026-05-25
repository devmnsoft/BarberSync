INSERT INTO platform_revenue_streams (stream_name, stream_type) VALUES
('SaaS Subscription','SAAS'),('B2B Marketplace Commission','MARKETPLACE'),('Transaction Fee','FINTECH')
ON CONFLICT DO NOTHING;
INSERT INTO commercial_opportunity_types (code, description, default_priority) VALUES
('EMPTY_SLOT_CAMPAIGN','Horário vazio com potencial de campanha',1),
('HIGH_STOCK_PROMOTION','Produto com estoque alto para promoção',2)
ON CONFLICT DO NOTHING;
-- Minimal demo seeds for new super-app ecosystem.
