-- Validação operacional do banco/schema barber
SET search_path TO barber, public;

SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'barber';
SELECT COUNT(*) AS total_tables FROM information_schema.tables WHERE table_schema = 'barber';
SELECT COUNT(*) AS total_views FROM information_schema.views WHERE table_schema = 'barber';

-- Tabelas principais
SELECT to_regclass('barber.users') AS users_table;
SELECT to_regclass('barber.clients') AS clients_table;
SELECT to_regclass('barber.services') AS services_table;
SELECT to_regclass('barber.products') AS products_table;
SELECT to_regclass('barber.appointments') AS appointments_table;
SELECT to_regclass('barber.audit_logs') AS audit_logs_table;
SELECT to_regclass('barber.immutable_audit_logs') AS immutable_audit_logs_table;

-- Seeds e usuários demo
SELECT COUNT(*) AS users_count FROM barber.users;
SELECT email FROM barber.users WHERE email IN ('admin@barbersync.com', 'cliente@demo.com');

-- Domínio de operação
SELECT COUNT(*) AS services_count FROM barber.services;
SELECT COUNT(*) AS products_count FROM barber.products;
SELECT COUNT(*) AS appointments_count FROM barber.appointments;

-- Dashboard
SELECT to_regclass('barber.vw_admin_dashboard') AS admin_dashboard_view;
SELECT COUNT(*) AS dashboard_rows FROM barber.vw_admin_dashboard;
