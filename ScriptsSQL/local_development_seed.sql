\if :{?seed_environment}
\else
  \echo 'FALHA: seed_environment não informado.'
  \quit 3
\endif
\if :{?password_hash}
\else
  \echo 'FALHA: password_hash não informado.'
  \quit 3
\endif

BEGIN;
SELECT set_config('barbersync.seed_environment', :'seed_environment', false);
DO $$ BEGIN
  IF current_setting('barbersync.seed_environment') <> 'Development' THEN
    RAISE EXCEPTION 'local_development_seed.sql é exclusivo para Development.';
  END IF;
END $$;

INSERT INTO barber.tenants(id,slug,name,status,is_active)
VALUES ('10000000-0000-0000-0000-000000000001','barbersync-local','BarberSync Local','Active',true)
ON CONFLICT (id) DO UPDATE SET slug=excluded.slug,name=excluded.name,status='Active',is_active=true,deleted_at=NULL;

INSERT INTO barber.branches(id,tenant_id,name,code,status,is_active)
VALUES ('10000000-0000-0000-0000-000000000002','10000000-0000-0000-0000-000000000001','Unidade Local','LOCAL','Active',true)
ON CONFLICT (id) DO UPDATE SET name=excluded.name,code=excluded.code,status='Active',is_active=true,deleted_at=NULL;

INSERT INTO barber.permissions(id,code,description) VALUES
 ('10000000-0000-0000-0000-000000000101','admin.access','Acesso administrativo local'),
 ('10000000-0000-0000-0000-000000000102','cash.manage','Operação de caixa local'),
 ('10000000-0000-0000-0000-000000000103','appointments.manage','Gestão de agenda local'),
 ('10000000-0000-0000-0000-000000000104','services.read','Consulta de serviços locais')
ON CONFLICT (id) DO UPDATE SET code=excluded.code,description=excluded.description;

INSERT INTO barber.roles(id,tenant_id,name,code,is_system) VALUES
 ('10000000-0000-0000-0000-000000000111','10000000-0000-0000-0000-000000000001','Administrador local','admin',false),
 ('10000000-0000-0000-0000-000000000112','10000000-0000-0000-0000-000000000001','Caixa local','cashier',false),
 ('10000000-0000-0000-0000-000000000113','10000000-0000-0000-0000-000000000001','Profissional local','professional',false)
ON CONFLICT (id) DO UPDATE SET name=excluded.name,code=excluded.code;

INSERT INTO barber.role_permissions(role_id,permission_id)
SELECT r.id,p.id FROM barber.roles r CROSS JOIN barber.permissions p
WHERE r.id='10000000-0000-0000-0000-000000000111' AND p.id IN ('10000000-0000-0000-0000-000000000101','10000000-0000-0000-0000-000000000102','10000000-0000-0000-0000-000000000103','10000000-0000-0000-0000-000000000104')
ON CONFLICT DO NOTHING;
INSERT INTO barber.role_permissions(role_id,permission_id) VALUES
 ('10000000-0000-0000-0000-000000000112','10000000-0000-0000-0000-000000000102'),
 ('10000000-0000-0000-0000-000000000113','10000000-0000-0000-0000-000000000103') ON CONFLICT DO NOTHING;

INSERT INTO barber.users(id,tenant_id,branch_id,email,password_hash,full_name,status,is_active) VALUES
 ('10000000-0000-0000-0000-000000000121','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','admin@barbersync.local',:'password_hash','Administrador Local','Active',true),
 ('10000000-0000-0000-0000-000000000122','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','caixa@barbersync.local',:'password_hash','Caixa Local','Active',true),
 ('10000000-0000-0000-0000-000000000123','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','profissional@barbersync.local',:'password_hash','Profissional Local','Active',true)
ON CONFLICT (id) DO UPDATE SET email=excluded.email,password_hash=excluded.password_hash,full_name=excluded.full_name,status='Active',is_active=true,deleted_at=NULL;
INSERT INTO barber.user_roles(user_id,role_id) VALUES
 ('10000000-0000-0000-0000-000000000121','10000000-0000-0000-0000-000000000111'),
 ('10000000-0000-0000-0000-000000000122','10000000-0000-0000-0000-000000000112'),
 ('10000000-0000-0000-0000-000000000123','10000000-0000-0000-0000-000000000113') ON CONFLICT DO NOTHING;

INSERT INTO barber.clients(id,tenant_id,branch_id,name,email,status,is_active)
VALUES ('10000000-0000-0000-0000-000000000131','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','Cliente Local','cliente@barbersync.local','Active',true)
ON CONFLICT (id) DO UPDATE SET name=excluded.name,email=excluded.email,status='Active',is_active=true,deleted_at=NULL;
INSERT INTO barber.professionals(id,tenant_id,branch_id,name,specialty,status,is_active)
VALUES ('10000000-0000-0000-0000-000000000132','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','Profissional Local','Barbearia','Active',true)
ON CONFLICT (id) DO UPDATE SET name=excluded.name,specialty=excluded.specialty,status='Active',is_active=true,deleted_at=NULL;
INSERT INTO barber.services(id,tenant_id,branch_id,category,name,duration_minutes,price,available_admin,available_public,available_mobile,available_kiosk,status,is_active)
VALUES ('10000000-0000-0000-0000-000000000133','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','Cabelo','Corte Local',30,50,true,true,true,true,'Active',true)
ON CONFLICT (id) DO UPDATE SET name=excluded.name,price=excluded.price,available_public=true,available_kiosk=true,status='Active',is_active=true,deleted_at=NULL;
INSERT INTO barber.professional_services(professional_id,service_id) VALUES ('10000000-0000-0000-0000-000000000132','10000000-0000-0000-0000-000000000133') ON CONFLICT DO NOTHING;
INSERT INTO barber.products(id,tenant_id,branch_id,sku,name,cost_price,sale_price,current_stock,minimum_stock,status,is_active)
VALUES ('10000000-0000-0000-0000-000000000134','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','LOCAL-001','Pomada Local',15,35,10,2,'Active',true)
ON CONFLICT (id) DO UPDATE SET name=excluded.name,sale_price=excluded.sale_price,status='Active',is_active=true,deleted_at=NULL;
INSERT INTO barber.cash_registers(id,tenant_id,branch_id,opened_by,opening_balance,status,is_active)
VALUES ('10000000-0000-0000-0000-000000000135','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','10000000-0000-0000-0000-000000000122',0,'Open',true)
ON CONFLICT (id) DO UPDATE SET status='Open',is_active=true,deleted_at=NULL;
INSERT INTO barber.kiosk_devices(id,tenant_id,branch_id,code,name,status,is_active)
VALUES ('10000000-0000-0000-0000-000000000136','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','KIOSK-LOCAL-001','Totem Local','Online',true)
ON CONFLICT (id) DO UPDATE SET code=excluded.code,name=excluded.name,status='Online',is_active=true,deleted_at=NULL;

INSERT INTO barber.saas_plans(id,code,name,description,monthly_price,annual_price,status)
VALUES ('10000000-0000-0000-0000-000000000141','LOCAL-DEVELOPMENT','Plano local','Exclusivo para desenvolvimento local',0,0,'Active')
ON CONFLICT (id) DO UPDATE SET name=excluded.name,status='Active';
INSERT INTO barber.tenant_subscriptions(id,tenant_id,plan_id,status,billing_cycle,starts_at)
VALUES ('10000000-0000-0000-0000-000000000142','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000141','Active','Monthly',TIMESTAMPTZ '2026-01-01 00:00:00+00')
ON CONFLICT (id) DO UPDATE SET status='Active',updated_at=now();
-- Quality local seed: registros determinísticos para desenvolvimento, não fallback de runtime.
INSERT INTO barber.client_reviews(id,tenant_id,branch_id,client_id,professional_id,rating,nps_score,comment,status,submitted_at) VALUES ('10000000-0000-0000-0000-000000000151','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','10000000-0000-0000-0000-000000000131','10000000-0000-0000-0000-000000000132',5,10,'Avaliação local controlada','Submitted',now()) ON CONFLICT(id) DO NOTHING;
INSERT INTO barber.quality_service_return_rules(id,tenant_id,branch_id,service_id,min_days,max_days,recommended_days,message,status,created_by) VALUES ('10000000-0000-0000-0000-000000000152','10000000-0000-0000-0000-000000000001','10000000-0000-0000-0000-000000000002','10000000-0000-0000-0000-000000000133',20,35,28,'Agende seu retorno quando for conveniente.','Active','10000000-0000-0000-0000-000000000122') ON CONFLICT(id) DO NOTHING;

COMMIT;

SELECT 'tenant='||name FROM barber.tenants WHERE id='10000000-0000-0000-0000-000000000001';
SELECT 'users='||count(*) FROM barber.users WHERE tenant_id='10000000-0000-0000-0000-000000000001';

-- A Central de Controle não fabrica KPIs locais. Registra apenas configuração desconhecida
-- para que a UI mostre o sourceStatus correto até uma verificação real ser executada.
INSERT INTO barber.command_center_integration_checks(tenant_id,branch_id,integration_key,source_module,status,message)
SELECT b.tenant_id,b.id,'source-integrity','Readiness','Unknown','Execute scripts/validate-source-integrity.sh para produzir evidência.' FROM barber.branches b
ON CONFLICT(tenant_id,branch_id,integration_key) DO NOTHING;
