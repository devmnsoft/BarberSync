select schema_name from information_schema.schemata where schema_name='barber';
select count(*) as tenants from barber.tenants;
select count(*) as kiosk_devices from barber.kiosk_devices;
