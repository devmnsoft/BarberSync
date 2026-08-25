\set ON_ERROR_STOP on
BEGIN;

-- This fixture is reserved for the disposable ProductionReadiness database.
-- Refuse to run anywhere else; this prevents fixed smoke identities reaching a real tenant.
DO $$
BEGIN
  IF current_setting('barbersync.environment', true) IS DISTINCT FROM 'ProductionReadiness' THEN
    RAISE EXCEPTION 'production_readiness_seed.sql requires barbersync.environment=ProductionReadiness';
  END IF;
END $$;

INSERT INTO barber.tenants(id,slug,name) VALUES ('70000000-0000-4000-8000-000000000001','production-readiness','BarberSync Readiness Tenant') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.branches(id,tenant_id,name,code) VALUES ('70000000-0000-4000-8000-000000000002','70000000-0000-4000-8000-000000000001','Unidade Readiness','READINESS') ON CONFLICT(id) DO NOTHING;

-- ASP.NET Core Identity PasswordHasher V3 hash for the local-only password documented in the runbook.
INSERT INTO barber.users(id,tenant_id,branch_id,email,password_hash,full_name,payload) VALUES
('70000000-0000-4000-8000-000000000010','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','admin@readiness.local','AQAAAAEAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v9O6Tusg+7E8aapRZB79KACECBW4AYajXeJw4KnjJX6EQ==','Readiness Admin','{"readiness":true}'),
('70000000-0000-4000-8000-000000000011','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','cashier@readiness.local','AQAAAAEAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v9O6Tusg+7E8aapRZB79KACECBW4AYajXeJw4KnjJX6EQ==','Readiness Cashier','{"readiness":true}'),
('70000000-0000-4000-8000-000000000012','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','professional@readiness.local','AQAAAAEAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v9O6Tusg+7E8aapRZB79KACECBW4AYajXeJw4KnjJX6EQ==','Readiness Professional','{"readiness":true}'),
('70000000-0000-4000-8000-000000000013','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','client@readiness.local','AQAAAAEAAYagAAAAEAARIjNEVWZ3iJmqu8zd7v9O6Tusg+7E8aapRZB79KACECBW4AYajXeJw4KnjJX6EQ==','Readiness Client','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.roles(id,tenant_id,name,code) VALUES
('70000000-0000-4000-8000-000000000020','70000000-0000-4000-8000-000000000001','Admin','Admin'),
('70000000-0000-4000-8000-000000000021','70000000-0000-4000-8000-000000000001','Cashier','Cashier'),
('70000000-0000-4000-8000-000000000022','70000000-0000-4000-8000-000000000001','Professional','Professional'),
('70000000-0000-4000-8000-000000000023','70000000-0000-4000-8000-000000000001','Client','Client') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.user_roles(user_id,role_id) VALUES
('70000000-0000-4000-8000-000000000010','70000000-0000-4000-8000-000000000020'),('70000000-0000-4000-8000-000000000011','70000000-0000-4000-8000-000000000021'),
('70000000-0000-4000-8000-000000000012','70000000-0000-4000-8000-000000000022'),('70000000-0000-4000-8000-000000000013','70000000-0000-4000-8000-000000000023') ON CONFLICT DO NOTHING;
-- The two operational smoke roles receive the schema's explicit permissions, never an auth bypass.
INSERT INTO barber.role_permissions(role_id,permission_id) SELECT r.id,p.id FROM barber.roles r CROSS JOIN barber.permissions p WHERE r.id IN ('70000000-0000-4000-8000-000000000020','70000000-0000-4000-8000-000000000021') ON CONFLICT DO NOTHING;

INSERT INTO barber.clients(id,tenant_id,branch_id,name,email,payload) VALUES ('70000000-0000-4000-8000-000000000013','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','Readiness Client','client@readiness.local','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.professionals(id,tenant_id,branch_id,name,specialty,default_commission,work_schedule,payload) VALUES ('70000000-0000-4000-8000-000000000012','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','Readiness Professional','Barber',20,'{"readiness":true}','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.services(id,tenant_id,branch_id,category,name,duration_minutes,price,commission_percent,available_mobile,available_kiosk,payload) VALUES ('70000000-0000-4000-8000-000000000030','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','Readiness','Readiness Haircut',30,40,20,true,true,'{"visibleOnKiosk":true,"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.professional_services(professional_id,service_id,commission_percent) VALUES ('70000000-0000-4000-8000-000000000012','70000000-0000-4000-8000-000000000030',20) ON CONFLICT DO NOTHING;
INSERT INTO barber.professional_working_hours(id,tenant_id,branch_id,professional_id,day_of_week,start_time,end_time,break_start,break_end) SELECT '70000000-0000-4000-8000-000000000031','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','70000000-0000-4000-8000-000000000012',1,'08:00','18:00','12:00','13:00' WHERE NOT EXISTS (SELECT 1 FROM barber.professional_working_hours WHERE id='70000000-0000-4000-8000-000000000031');
INSERT INTO barber.products(id,tenant_id,branch_id,sku,name,cost_price,sale_price,current_stock,minimum_stock,payload) VALUES ('70000000-0000-4000-8000-000000000040','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','READINESS-PRODUCT','Readiness Pomade',10,25,10,2,'{"readiness":true}') ON CONFLICT(id) DO UPDATE SET current_stock=greatest(barber.products.current_stock,10),minimum_stock=2,updated_at=now() WHERE barber.products.tenant_id='70000000-0000-4000-8000-000000000001' AND barber.products.payload->>'readiness'='true';
INSERT INTO barber.suppliers(id,tenant_id,branch_id,name,status,payload) VALUES ('70000000-0000-4000-8000-000000000041','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','Readiness Supplier','Active','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.kiosk_devices(id,tenant_id,branch_id,code,name,status,payload) VALUES ('70000000-0000-4000-8000-000000000050','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','READINESS-KIOSK-001','Readiness Kiosk','Online','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.cash_registers(id,tenant_id,branch_id,opened_by,opening_balance,status,payload) VALUES ('70000000-0000-4000-8000-000000000060','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','70000000-0000-4000-8000-000000000011',100,'Open','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.notifications(id,tenant_id,branch_id,user_id,title,message,payload) VALUES ('70000000-0000-4000-8000-000000000070','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','70000000-0000-4000-8000-000000000010','Readiness notification','Controlled readiness notification','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.client_profiles(id,tenant_id,branch_id,client_id,email,last_visit_at,total_spent,visit_count,preferences_json) VALUES ('70000000-0000-4000-8000-000000000080','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','70000000-0000-4000-8000-000000000013','client@readiness.local',now(),40,1,'{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.coupons(id,tenant_id,branch_id,code,name,discount_type,discount_value,discount_percent,valid_from,valid_until,status,payload) VALUES ('70000000-0000-4000-8000-000000000081','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','READINESS10','Readiness Coupon','Percentage',10,10,now()-interval '1 day',now()+interval '30 days','Active','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.packages(id,tenant_id,branch_id,status,payload) VALUES ('70000000-0000-4000-8000-000000000082','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','Active','{"name":"Readiness Package","price":40,"validityDays":30,"services":[{"serviceId":"70000000-0000-4000-8000-000000000030","quantity":1}],"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.client_packages(id,tenant_id,branch_id,status,payload) VALUES ('70000000-0000-4000-8000-000000000083','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','Active','{"clientId":"70000000-0000-4000-8000-000000000013","packageId":"70000000-0000-4000-8000-000000000082","remainingSessions":1,"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.loyalty_accounts(id,tenant_id,branch_id,client_id,points,points_balance,cashback_balance,status,payload) VALUES ('70000000-0000-4000-8000-000000000084','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','70000000-0000-4000-8000-000000000013',10,10,2,'Active','{"clientId":"70000000-0000-4000-8000-000000000013","pointsBalance":10,"cashbackBalance":2,"readiness":true}') ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.loyalty_transactions(id,tenant_id,branch_id,loyalty_account_id,client_id,points,type,points_delta,cashback_delta,description,status,payload) VALUES ('70000000-0000-4000-8000-000000000085','70000000-0000-4000-8000-000000000001','70000000-0000-4000-8000-000000000002','70000000-0000-4000-8000-000000000084','70000000-0000-4000-8000-000000000013',10,'Accrual',10,2,'Readiness accrual','Posted','{"readiness":true}') ON CONFLICT(id) DO NOTHING;
COMMIT;
