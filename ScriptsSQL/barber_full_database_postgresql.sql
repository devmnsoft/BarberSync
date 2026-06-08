-- BarberSync PostgreSQL schema operacional consolidado.
-- Fonte principal para API/PostgreSQL real. DemoStore/localStorage deve ser usado apenas como fallback de front-end.

CREATE EXTENSION IF NOT EXISTS pgcrypto;
CREATE SCHEMA IF NOT EXISTS barber;

CREATE TABLE IF NOT EXISTS barber.tenants (
  id uuid primary key default gen_random_uuid(),
  slug varchar(120) unique not null,
  name varchar(180) not null,
  document varchar(40),
  status varchar(30) not null default 'Active',
  is_active boolean not null default true,
  created_at timestamp not null default now(),
  updated_at timestamp null,
  deleted_at timestamp null,
  created_by uuid null,
  updated_by uuid null
);

CREATE TABLE IF NOT EXISTS barber.branches (
  id uuid primary key default gen_random_uuid(),
  tenant_id uuid not null references barber.tenants(id),
  name varchar(180) not null,
  code varchar(60),
  status varchar(30) not null default 'Active',
  is_active boolean not null default true,
  created_at timestamp not null default now(),
  updated_at timestamp null,
  deleted_at timestamp null,
  created_by uuid null,
  updated_by uuid null,
  payload jsonb not null default '{}'::jsonb
);

DO $$
DECLARE t text;
BEGIN
  FOREACH t IN ARRAY ARRAY[
    'users','profiles','permissions','clients','professionals','professional_services','services','products',
    'appointments','attendances','service_orders','service_order_items','payments','cash_registers',
    'stock_movements','campaigns','coupons','loyalty_accounts','loyalty_transactions','reviews',
    'public_leads','kiosk_devices','kiosk_sessions','app_settings'
  ] LOOP
    EXECUTE format($fmt$
      CREATE TABLE IF NOT EXISTS barber.%I (
        id uuid primary key default gen_random_uuid(),
        tenant_id uuid not null references barber.tenants(id),
        branch_id uuid null references barber.branches(id),
        status varchar(30) not null default 'Active',
        is_active boolean not null default true,
        created_at timestamp not null default now(),
        updated_at timestamp null,
        deleted_at timestamp null,
        created_by uuid null,
        updated_by uuid null,
        payload jsonb not null default '{}'::jsonb
      );
    $fmt$, t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (tenant_id);', 'ix_' || t || '_tenant_id', t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (branch_id);', 'ix_' || t || '_branch_id', t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (status);', 'ix_' || t || '_status', t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (created_at);', 'ix_' || t || '_created_at', t);
    EXECUTE format('CREATE INDEX IF NOT EXISTS %I ON barber.%I (deleted_at);', 'ix_' || t || '_deleted_at', t);
  END LOOP;
END $$;

CREATE TABLE IF NOT EXISTS barber.audit_logs (
  id uuid primary key default gen_random_uuid(),
  tenant_id uuid null references barber.tenants(id),
  branch_id uuid null references barber.branches(id),
  user_id uuid null,
  module varchar(80) not null,
  action varchar(80) not null,
  entity_name varchar(120) not null,
  entity_id uuid null,
  description text not null,
  metadata jsonb not null default '{}'::jsonb,
  created_at timestamp not null default now(),
  updated_at timestamp null,
  deleted_at timestamp null,
  is_active boolean not null default true,
  created_by uuid null,
  updated_by uuid null
);

CREATE TABLE IF NOT EXISTS barber.notifications (
  id uuid primary key default gen_random_uuid(),
  tenant_id uuid null references barber.tenants(id),
  branch_id uuid null references barber.branches(id),
  title varchar(160) not null,
  message text not null,
  entity_name varchar(120) null,
  entity_id uuid null,
  payload jsonb not null default '{}'::jsonb,
  status varchar(30) not null default 'Unread',
  is_active boolean not null default true,
  created_at timestamp not null default now(),
  updated_at timestamp null,
  deleted_at timestamp null,
  created_by uuid null,
  updated_by uuid null
);

CREATE INDEX IF NOT EXISTS ix_audit_logs_tenant_id ON barber.audit_logs (tenant_id);
CREATE INDEX IF NOT EXISTS ix_audit_logs_branch_id ON barber.audit_logs (branch_id);
CREATE INDEX IF NOT EXISTS ix_audit_logs_created_at ON barber.audit_logs (created_at);
CREATE INDEX IF NOT EXISTS ix_notifications_tenant_id ON barber.notifications (tenant_id);
CREATE INDEX IF NOT EXISTS ix_notifications_branch_id ON barber.notifications (branch_id);
CREATE INDEX IF NOT EXISTS ix_notifications_status ON barber.notifications (status);
CREATE INDEX IF NOT EXISTS ix_notifications_created_at ON barber.notifications (created_at);
CREATE UNIQUE INDEX IF NOT EXISTS ux_clients_document_tenant_active ON barber.clients (tenant_id, (payload->>'document')) WHERE deleted_at IS NULL AND coalesce(payload->>'document','') <> '';
CREATE INDEX IF NOT EXISTS ix_appointments_professional_scheduled ON barber.appointments (tenant_id, (payload->>'professionalId'), (payload->>'scheduledAt')) WHERE deleted_at IS NULL;
CREATE INDEX IF NOT EXISTS ix_products_current_min_stock ON barber.products (((payload->>'currentStock')::numeric), ((payload->>'minStock')::numeric)) WHERE deleted_at IS NULL;

INSERT INTO barber.tenants (id, slug, name, document, status, is_active)
VALUES ('11111111-1111-1111-1111-111111111111', 'barbersync-demo', 'BarberSync Demo', '00.000.000/0001-00', 'Active', true)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, status = excluded.status, is_active = excluded.is_active;

INSERT INTO barber.branches (id, tenant_id, name, code, status, is_active, payload)
VALUES ('22222222-2222-2222-2222-222222222222', '11111111-1111-1111-1111-111111111111', 'Unidade Centro', 'CENTRO', 'Active', true, '{"city":"São Paulo","state":"SP"}'::jsonb)
ON CONFLICT (id) DO UPDATE SET name = excluded.name, payload = excluded.payload;

INSERT INTO barber.users (id, tenant_id, branch_id, payload, status)
VALUES ('33333333-3333-3333-3333-333333333333', '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', '{"name":"Administrador Demo","email":"admin@barbersync.demo","role":"Admin"}'::jsonb, 'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.clients (id, tenant_id, branch_id, payload, status) VALUES
('00000000-0000-0000-0000-000000000101','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Marcos Vinícius","type":"PF","document":"123.456.789-01","phone":"(11) 98801-1001","whatsapp":"(11) 98801-1001","email":"marcos@demo.com","city":"São Paulo","isVip":true,"acceptsPromotions":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000102','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Thiago Almeida","type":"PF","document":"123.456.789-02","phone":"(11) 98802-1002","email":"thiago@demo.com","city":"Santo André"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000103','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Fernanda Costa","type":"PF","document":"123.456.789-03","phone":"(11) 98803-1003","email":"fernanda@demo.com","city":"Osasco","isVip":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000104','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Eduardo Lima","type":"PF","document":"123.456.789-04","phone":"(11) 98804-1004","email":"eduardo@demo.com","city":"Guarulhos"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000105','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Barber Prime Ltda","type":"PJ","document":"12.345.678/0001-90","phone":"(11) 3000-1005","email":"prime@demo.com","city":"Campinas"}'::jsonb,'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.professionals (id, tenant_id, branch_id, payload, status) VALUES
('00000000-0000-0000-0000-000000000201','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Rafael Barber","specialty":"Fade e barba","phone":"(11) 97701-2001","email":"rafael@demo.com","commissionPercent":40,"rating":4.9,"services":["Corte Masculino","Corte + Barba"]}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000202','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Lucas Navalha","specialty":"Navalhado","phone":"(11) 97702-2002","email":"lucas@demo.com","commissionPercent":38,"rating":4.8,"services":["Barba Tradicional","Sobrancelha"]}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000203','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Camila Beauty","specialty":"Estética facial","phone":"(11) 97703-2003","email":"camila@demo.com","commissionPercent":42,"rating":5.0}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000204','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Amanda Nails","specialty":"Nail designer","phone":"(11) 97704-2004","email":"amanda@demo.com","commissionPercent":35,"rating":4.7}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000205','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Bianca Studio","specialty":"Cabelo e make","phone":"(11) 97705-2005","email":"bianca@demo.com","commissionPercent":37,"rating":4.8}'::jsonb,'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.services (id, tenant_id, branch_id, payload, status) VALUES
('00000000-0000-0000-0000-000000000301','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Corte Masculino","category":"Barbearia","description":"Corte moderno com acabamento profissional.","price":45,"durationMinutes":40,"commissionPercent":40,"visibleOnAdmin":true,"visibleOnPublicWeb":true,"visibleOnKiosk":true,"visibleOnMobile":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000302','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Barba Tradicional","category":"Barbearia","price":35,"durationMinutes":30,"commissionPercent":35,"visibleOnAdmin":true,"visibleOnPublicWeb":true,"visibleOnKiosk":true,"visibleOnMobile":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000303','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Corte + Barba","category":"Combo","price":75,"durationMinutes":70,"commissionPercent":40,"visibleOnAdmin":true,"visibleOnPublicWeb":true,"visibleOnKiosk":true,"visibleOnMobile":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000304','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Sobrancelha","category":"Estética","price":25,"durationMinutes":20,"commissionPercent":30,"visibleOnAdmin":true,"visibleOnPublicWeb":true,"visibleOnKiosk":true,"visibleOnMobile":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000305','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Hidratação Capilar","category":"Tratamento","price":60,"durationMinutes":45,"commissionPercent":35,"visibleOnPublicWeb":true,"visibleOnKiosk":true,"visibleOnMobile":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000306','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Manicure","category":"Beleza","price":40,"durationMinutes":50,"commissionPercent":35,"visibleOnPublicWeb":true,"visibleOnKiosk":true,"visibleOnMobile":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000307','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Pedicure","category":"Beleza","price":45,"durationMinutes":55,"commissionPercent":35,"visibleOnPublicWeb":true,"visibleOnKiosk":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000308','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Limpeza de Pele","category":"Estética","price":95,"durationMinutes":70,"commissionPercent":35,"visibleOnPublicWeb":true,"visibleOnKiosk":false}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000309','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Massagem Capilar","category":"Bem-estar","price":65,"durationMinutes":35,"commissionPercent":35,"visibleOnPublicWeb":true,"visibleOnKiosk":true}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000310','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Dia do Noivo","category":"Experiência","price":220,"durationMinutes":150,"commissionPercent":42,"visibleOnPublicWeb":true,"visibleOnKiosk":true}'::jsonb,'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.products (id, tenant_id, branch_id, payload, status) VALUES
('00000000-0000-0000-0000-000000000401','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Lâmina Platinum","category":"Insumo","sku":"BS-0001","costPrice":8,"salePrice":18,"currentStock":4,"minStock":12,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000402','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Pomada Modeladora","category":"Revenda","sku":"BS-0002","costPrice":12,"salePrice":39,"currentStock":5,"minStock":12,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000403','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Shampoo Anticaspa","category":"Revenda","sku":"BS-0003","costPrice":15,"salePrice":42,"currentStock":6,"minStock":12,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000404','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Balm para Barba","category":"Revenda","sku":"BS-0004","costPrice":11,"salePrice":36,"currentStock":7,"minStock":12,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000405','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Toalha Premium","category":"Insumo","sku":"BS-0005","costPrice":10,"salePrice":35,"currentStock":8,"minStock":12,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000406','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Óleo Essencial","category":"Tratamento","sku":"BS-0006","costPrice":14,"salePrice":48,"currentStock":20,"minStock":10,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000407','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Gel Cola","category":"Revenda","sku":"BS-0007","costPrice":9,"salePrice":29,"currentStock":24,"minStock":10,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000408','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Máscara Facial","category":"Estética","sku":"BS-0008","costPrice":18,"salePrice":55,"currentStock":18,"minStock":10,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000409','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Creme Hidratante","category":"Tratamento","sku":"BS-0009","costPrice":16,"salePrice":49,"currentStock":19,"minStock":10,"unit":"un"}'::jsonb,'Active'),
('00000000-0000-0000-0000-000000000410','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Pós-Barba Ice","category":"Revenda","sku":"BS-0010","costPrice":13,"salePrice":44,"currentStock":22,"minStock":10,"unit":"un"}'::jsonb,'Active')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.appointments (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((500 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('clientName', (ARRAY['Marcos Vinícius','Thiago Almeida','Fernanda Costa','Eduardo Lima','Barber Prime Ltda'])[1 + ((i-1)%5)], 'serviceName', (ARRAY['Corte Masculino','Barba Tradicional','Corte + Barba','Sobrancelha','Hidratação Capilar'])[1 + ((i-1)%5)], 'professionalName', (ARRAY['Rafael Barber','Lucas Navalha','Camila Beauty','Amanda Nails','Bianca Studio'])[1 + ((i-1)%5)], 'scheduledAt', to_char(now() + (i || ' hours')::interval, 'YYYY-MM-DD"T"HH24:MI:SS'), 'origin', 'Admin', 'notes', 'Seed desenvolvimento'), 'Scheduled'
FROM generate_series(1,10) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.service_orders (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((600 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('number', 'SO-2026-' || (1000+i), 'clientName', (ARRAY['Marcos Vinícius','Thiago Almeida','Fernanda Costa','Eduardo Lima','Barber Prime Ltda'])[i], 'professionalName', 'Rafael Barber', 'total', 80 + i, 'paidAmount', case when i > 2 then 80 + i else 0 end), case when i > 2 then 'Paid' else 'Open' end
FROM generate_series(1,5) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.payments (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((700 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('method', (ARRAY['PIX','Cartão','Dinheiro','Misto','PIX'])[i], 'amount', 75 + i, 'receiptNumber', 'REC-2026-' || (1000+i)), 'Paid'
FROM generate_series(1,5) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.campaigns (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((800 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('name', (ARRAY['Volte e Ganhe','Combo do Mês','Aniversariantes'])[i], 'audience', 'Base ativa', 'channel', (ARRAY['WhatsApp','Instagram','SMS'])[i], 'message', 'Campanha seed'), 'Active'
FROM generate_series(1,3) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.coupons (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((900 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('code', (ARRAY['VIP10','VOLTE20','COMBO15'])[i], 'type', 'Percentual', 'percent', (ARRAY[10,20,15])[i], 'usageLimit', 100), 'Active'
FROM generate_series(1,3) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.reviews (id, tenant_id, branch_id, payload, status)
SELECT ('00000000-0000-0000-0000-' || lpad((1000 + i)::text, 12, '0'))::uuid, '11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222', jsonb_build_object('clientName', (ARRAY['Marcos','Thiago','Fernanda','Eduardo','Renata'])[i], 'rating', (ARRAY[5,5,4,5,4])[i], 'comment', 'Avaliação seed BarberSync'), 'Published'
FROM generate_series(1,5) i
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.kiosk_devices (id, tenant_id, branch_id, payload, status)
VALUES ('00000000-0000-0000-0000-000000001101','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"deviceCode":"KIOSK-DEMO-001","name":"Totem Demo Centro","lastSeenAt":"2026-06-07T00:00:00Z"}'::jsonb,'Online')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.audit_logs (tenant_id, branch_id, module, action, entity_name, description, metadata)
SELECT '11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','Seed','DatabaseSeeded','barber_full_database_postgresql','Seeds mínimos de desenvolvimento aplicados.','{"source":"ScriptsSQL/barber_full_database_postgresql.sql"}'::jsonb
WHERE NOT EXISTS (select 1 from barber.audit_logs where module = 'Seed' and action = 'DatabaseSeeded' and entity_name = 'barber_full_database_postgresql');

-- Regras operacionais consolidadas para CRUD real BarberSync.
CREATE UNIQUE INDEX IF NOT EXISTS ux_clients_document_alias_tenant_active
ON barber.clients (tenant_id, lower(coalesce(payload->>'document', payload->>'cpfCnpj', payload->>'cpf', payload->>'cnpj', '')))
WHERE deleted_at IS NULL AND coalesce(payload->>'document', payload->>'cpfCnpj', payload->>'cpf', payload->>'cnpj', '') <> '';

INSERT INTO barber.public_leads (id, tenant_id, branch_id, payload, status)
VALUES ('00000000-0000-0000-0000-000000001201','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','{"name":"Lead PublicWeb Demo","phone":"(11) 97777-0000","serviceName":"Corte + Barba","origin":"PublicWeb"}'::jsonb,'New')
ON CONFLICT (id) DO NOTHING;

INSERT INTO barber.notifications (id, tenant_id, branch_id, title, message, entity_name, payload, status)
VALUES
('00000000-0000-0000-0000-000000001301','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','Estoque crítico','Pomada Modeladora está abaixo do mínimo.','products','{"source":"seed","severity":"warning"}'::jsonb,'Unread'),
('00000000-0000-0000-0000-000000001302','11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','Novo agendamento público','Lead PublicWeb pronto para confirmação.','appointments','{"source":"seed","severity":"info"}'::jsonb,'Unread')
ON CONFLICT (id) DO NOTHING;
