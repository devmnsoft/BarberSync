SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'barber';
SELECT COUNT(*) AS total_tables FROM information_schema.tables WHERE table_schema='barber';
SELECT COUNT(*) FROM barber.users;
SELECT COUNT(*) FROM barber.tenants;
SELECT COUNT(*) FROM barber.services;
SELECT COUNT(*) FROM barber.appointments;
SELECT COUNT(*) FROM barber.products;
SELECT COUNT(*) FROM barber.vw_admin_dashboard;
SELECT email FROM barber.users WHERE email IN ('admin@barbersync.com','cliente@demo.com');
