-- BarberSync consolidated database validation script

-- Schemas
SELECT schema_name FROM information_schema.schemata
WHERE schema_name IN (
'core','identity','tenant','admin','scheduling','operations','financial','fiscal','inventory','crm','marketing','loyalty','customer','marketplace','b2b','hr','quality','compliance','documents','academy','community','partners','developer','integration','communication','ai','analytics','growth','franchise','platform','audit','workflow','forms','dashboard','support','public_api','observability','messaging','simulation','optimization','mobile','kiosk'
)
ORDER BY schema_name;

-- Core entities
SELECT to_regclass('identity.users') AS identity_users;
SELECT to_regclass('tenant.tenants') AS tenant_tenants;
SELECT to_regclass('scheduling.services') AS scheduling_services;
SELECT to_regclass('financial.payments') AS financial_payments;
SELECT to_regclass('inventory.products') AS inventory_products;
SELECT to_regclass('dashboard.vw_admin_dashboard') AS vw_admin_dashboard;

-- Seed existence
SELECT COUNT(*) AS users_count FROM identity.users;
SELECT COUNT(*) AS tenants_count FROM tenant.tenants;
SELECT COUNT(*) AS services_count FROM scheduling.services;
SELECT COUNT(*) AS products_count FROM inventory.products;

-- Demo users
SELECT email, status FROM identity.users
WHERE email IN (
'admin@barbersync.com','gerente@barbeariaelite.com','recepcao@barbeariaelite.com',
'financeiro@barbeariaelite.com','profissional@barbeariaelite.com','cliente@demo.com',
'totem@barbeariaelite.com','suporte@barbersync.com','comercial@barbersync.com','developer@barbersync.com'
)
ORDER BY email;

-- Important indexes
SELECT schemaname, tablename, indexname
FROM pg_indexes
WHERE (schemaname='identity' AND tablename='users')
   OR (schemaname='scheduling' AND tablename='appointments')
   OR (schemaname='financial' AND tablename='payments')
   OR (schemaname='inventory' AND tablename='products')
   OR (schemaname='analytics' AND tablename='data_events')
   OR (schemaname='messaging' AND tablename='outbox_messages')
ORDER BY schemaname, tablename, indexname;
