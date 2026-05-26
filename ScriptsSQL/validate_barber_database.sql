-- Validação operacional do banco/schema barber
SET search_path TO barber, public;

SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'barber';
SELECT COUNT(*) AS total_tables FROM information_schema.tables WHERE table_schema = 'barber';
SELECT COUNT(*) AS total_views FROM information_schema.views WHERE table_schema = 'barber';

-- Tabelas principais
SELECT to_regclass('barber.users') AS users_table;
SELECT to_regclass('barber.clients') AS clients_table;
SELECT to_regclass('barber.professionals') AS professionals_table;
SELECT to_regclass('barber.services') AS services_table;
SELECT to_regclass('barber.products') AS products_table;
SELECT to_regclass('barber.appointments') AS appointments_table;
SELECT to_regclass('barber.cash_register_sessions') AS cash_register_sessions_table;

-- Usuários demo
SELECT email FROM barber.users
WHERE email IN (
 'admin@barbersync.com','gerente@barbeariaelite.com','recepcao@barbeariaelite.com',
 'financeiro@barbeariaelite.com','profissional@barbeariaelite.com','cliente@demo.com','totem@barbeariaelite.com'
)
ORDER BY email;

-- Seeds comerciais mínimos para demo
SELECT COUNT(*) AS demo_clients FROM barber.clients WHERE name ILIKE 'Cliente Demo %';
SELECT COUNT(*) AS demo_services FROM barber.services;
SELECT COUNT(*) AS demo_products FROM barber.products;
SELECT COUNT(*) AS demo_appointments FROM barber.appointments;

-- Views principais
SELECT to_regclass('barber.vw_admin_dashboard') AS admin_dashboard_view;
SELECT to_regclass('barber.vw_financial_dashboard') AS financial_dashboard_view;
SELECT to_regclass('barber.vw_stock_critical') AS stock_critical_view;
SELECT COUNT(*) AS dashboard_rows FROM barber.vw_admin_dashboard;


-- Tabelas evolucao 2026
SELECT to_regclass('barber.service_packages') AS service_packages_table;
SELECT to_regclass('barber.service_package_items') AS service_package_items_table;
SELECT to_regclass('barber.customer_service_packages') AS customer_service_packages_table;
SELECT to_regclass('barber.customer_service_package_usage') AS customer_service_package_usage_table;
SELECT to_regclass('barber.appointment_recurrences') AS appointment_recurrences_table;
SELECT to_regclass('barber.appointment_reschedules') AS appointment_reschedules_table;
SELECT to_regclass('barber.cancellation_policies') AS cancellation_policies_table;
SELECT to_regclass('barber.cancellation_fees') AS cancellation_fees_table;
SELECT to_regclass('barber.client_no_show_history') AS client_no_show_history_table;
SELECT to_regclass('barber.client_booking_blocks') AS client_booking_blocks_table;
SELECT to_regclass('barber.campaign_coupon_usage') AS campaign_coupon_usage_table;
SELECT to_regclass('barber.stock_transfers') AS stock_transfers_table;
SELECT to_regclass('barber.client_timeline') AS client_timeline_table;
SELECT to_regclass('barber.client_internal_notes') AS client_internal_notes_table;
SELECT to_regclass('barber.client_scores') AS client_scores_table;
SELECT to_regclass('barber.client_next_best_actions') AS client_next_best_actions_table;
