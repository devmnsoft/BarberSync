INSERT INTO developer_accounts(name,email) VALUES ('Demo Integrator','dev@demo.com') ON CONFLICT DO NOTHING;
INSERT INTO technology_partners(name,partner_type) VALUES ('ERP Pro','ERP') ON CONFLICT DO NOTHING;
INSERT INTO governance_committees(name,scope) VALUES ('Comitê Estratégico','Nacional') ON CONFLICT DO NOTHING;
INSERT INTO risk_categories(name) VALUES ('Operacional') ON CONFLICT DO NOTHING;
INSERT INTO incident_categories(name) VALUES ('Queda de API') ON CONFLICT DO NOTHING;
INSERT INTO ai_agents(name,domain) VALUES ('OperationsAgent','Operations') ON CONFLICT DO NOTHING;
INSERT INTO compliance_frameworks(name,version) VALUES ('LGPD','1.0') ON CONFLICT DO NOTHING;
